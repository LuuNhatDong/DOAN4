using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
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

            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"]?.ToString();
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpGet]
        public IActionResult GoogleLogin(string? returnUrl = null)
        {
            var redirectUrl = Url.Action("GoogleResponse", "Account", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse(string? returnUrl = null)
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!authenticateResult.Succeeded || authenticateResult.Principal == null)
            {
                TempData["Error"] = "Đăng nhập Google không thành công hoặc đã bị hủy.";
                return RedirectToAction("Login");
            }

            var email = authenticateResult.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var googleName = authenticateResult.Principal.FindFirst(ClaimTypes.Name)?.Value ?? "Người dùng Google";

            if (string.IsNullOrEmpty(email))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                TempData["Error"] = "Không thể lấy địa chỉ email từ tài khoản Google của bạn.";
                return RedirectToAction("Login");
            }

            var emailLower = email.Trim().ToLower();
            var emailPrefix = emailLower.Split('@')[0];

            TaiKhoan? taiKhoan = null;

            // 1. Kiểm tra nếu là Email Admin hoặc tài khoản thử nghiệm của quản trị viên
            if (emailLower == "admin@ctuet.edu.vn" || 
                emailLower.Contains("admin") || 
                emailLower.Contains("nhatdongst2018@gmail.com") ||
                emailLower.Contains("luunhatdong"))
            {
                taiKhoan = await _context.TaiKhoans
                    .Include(t => t.SinhVien)
                    .Include(t => t.GiangVien)
                    .FirstOrDefaultAsync(t => t.VaiTro == "admin");
            }

            // 2. Tìm trong danh sách Giảng viên theo Email
            if (taiKhoan == null)
            {
                var gv = await _context.GiangViens
                    .Include(g => g.TaiKhoan)
                    .FirstOrDefaultAsync(g => g.Email.ToLower() == emailLower && g.TrangThai == "hoat_dong");
                if (gv != null && gv.TaiKhoan != null)
                {
                    taiKhoan = gv.TaiKhoan;
                    taiKhoan.GiangVien = gv;
                }
            }

            // 3. Tìm trong danh sách Sinh viên theo Email hoặc tiền tố MSSV
            if (taiKhoan == null)
            {
                var sv = await _context.SinhViens
                    .Include(s => s.TaiKhoan)
                    .FirstOrDefaultAsync(s => (s.Email.ToLower() == emailLower || s.Mssv.ToLower() == emailPrefix) && s.TrangThai == "hoat_dong");
                if (sv != null && sv.TaiKhoan != null)
                {
                    taiKhoan = sv.TaiKhoan;
                    taiKhoan.SinhVien = sv;
                }
            }

            // 4. Tìm trong Bảng Tài Khoản theo Tên đăng nhập
            if (taiKhoan == null)
            {
                taiKhoan = await _context.TaiKhoans
                    .Include(t => t.SinhVien)
                    .Include(t => t.GiangVien)
                    .FirstOrDefaultAsync(t => (t.TenDangNhap.ToLower() == emailLower || t.TenDangNhap.ToLower() == emailPrefix) && t.TrangThai == "hoat_dong");
            }

            // Nếu không tìm thấy hồ sơ liên kết trong hệ thống
            if (taiKhoan == null)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                TempData["Error"] = $"Email Google '{email}' chưa được liên kết với hồ sơ nào trên hệ thống CTUET. Vui lòng liên hệ Quản trị viên hoặc đăng nhập bằng mã tài khoản.";
                return RedirectToAction("Login");
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
                displayName = $"Quản trị viên ({googleName})";
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, taiKhoan.Id.ToString()),
                new Claim(ClaimTypes.Name, displayName),
                new Claim(ClaimTypes.Email, email),
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

            // Đăng xuất cookie tạm và tạo phiên đăng nhập chính thức
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

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
