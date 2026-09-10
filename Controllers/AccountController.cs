using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThucTap.Models;

namespace QuanLyThucTap.Controllers
{
    public class AccountController : Controller
    {
        private readonly ThucTapDbContext _context;

        public AccountController(ThucTapDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                if (role == "admin") return RedirectToAction("Index", "Home", new { area = "Admin" });
                if (role == "giang_vien") return RedirectToAction("Index", "Home", new { area = "GiangVien" });
                if (role == "sinh_vien") return RedirectToAction("Index", "Home", new { area = "SinhVien" });
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string tenDangNhap, string matKhau, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrWhiteSpace(matKhau))
            {
                ViewBag.Error = "Vui lòng nhập tên đăng nhập và mật khẩu.";
                return View();
            }

            var taiKhoan = await _context.TaiKhoans
                .Include(t => t.SinhVien)
                .Include(t => t.GiangVien)
                .FirstOrDefaultAsync(t => t.TenDangNhap == tenDangNhap && t.TrangThai == "hoat_dong");

            if (taiKhoan == null)
            {
                ViewBag.Error = "Tên đăng nhập không tồn tại hoặc tài khoản đã bị khóa.";
                return View();
            }

            // Kiểm tra mật khẩu (hỗ trợ test 123456 hoặc hash)
            bool isPasswordCorrect = matKhau == "123456" || 
                                     taiKhoan.MatKhauHash.Contains("123456") || 
                                     taiKhoan.MatKhauHash == matKhau;

            if (!isPasswordCorrect)
            {
                ViewBag.Error = "Mật khẩu không chính xác.";
                return View();
            }

            string displayName = taiKhoan.TenDangNhap;
            string userDetailId = "";

            if (taiKhoan.VaiTro == "sinh_vien" && taiKhoan.SinhVien != null)
            {
                displayName = taiKhoan.SinhVien.Hovaten;
                userDetailId = taiKhoan.SinhVien.Id.ToString();
            }
            else if (taiKhoan.VaiTro == "giang_vien" && taiKhoan.GiangVien != null)
            {
                displayName = taiKhoan.GiangVien.Hovaten;
                userDetailId = taiKhoan.GiangVien.Id.ToString();
            }
            else if (taiKhoan.VaiTro == "admin")
            {
                displayName = "Quản trị viên Khoa";
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, taiKhoan.Id.ToString()),
                new Claim(ClaimTypes.Name, displayName),
                new Claim("Username", taiKhoan.TenDangNhap),
                new Claim(ClaimTypes.Role, taiKhoan.VaiTro),
                new Claim("UserDetailId", userDetailId)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Điều hướng theo vai trò
            return taiKhoan.VaiTro switch
            {
                "admin" => RedirectToAction("Index", "Home", new { area = "Admin" }),
                "giang_vien" => RedirectToAction("Index", "Home", new { area = "GiangVien" }),
                "sinh_vien" => RedirectToAction("Index", "Home", new { area = "SinhVien" }),
                _ => RedirectToAction("Login")
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
