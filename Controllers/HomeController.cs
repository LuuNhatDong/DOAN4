using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace QuanLyThucTap.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                return role switch
                {
                    "admin" => RedirectToAction("Index", "Home", new { area = "Admin" }),
                    "giang_vien" => RedirectToAction("Index", "Home", new { area = "GiangVien" }),
                    "sinh_vien" => RedirectToAction("Index", "Home", new { area = "SinhVien" }),
                    _ => RedirectToAction("Login", "Account")
                };
            }

            return RedirectToAction("Login", "Account");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
