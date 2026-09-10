using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThucTap.Models;

namespace QuanLyThucTap.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    [Authorize(Roles = "giang_vien")]
    public class HomeController : Controller
    {
        private readonly ThucTapDbContext _context;

        public HomeController(ThucTapDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userDetailIdStr = User.FindFirst("UserDetailId")?.Value;
            if (!long.TryParse(userDetailIdStr, out long gvId))
            {
                // Tìm theo username
                var username = User.FindFirst("Username")?.Value;
                var gv = await _context.GiangViens.FirstOrDefaultAsync(g => g.Magv == username);
                if (gv == null) return RedirectToAction("Login", "Account", new { area = "" });
                gvId = gv.Id;
            }

            var giangVien = await _context.GiangViens
                .Include(g => g.DanhSachPhanCong)
                    .ThenInclude(p => p.SinhVien)
                .Include(g => g.DanhSachPhanCong)
                    .ThenInclude(p => p.DoanhNghiep)
                .Include(g => g.DanhSachPhanCong)
                    .ThenInclude(p => p.DeCuongThucTap)
                .Include(g => g.DanhSachPhanCong)
                    .ThenInclude(p => p.DanhGiaKetQua)
                .FirstOrDefaultAsync(g => g.Id == gvId);

            if (giangVien == null) return NotFound();

            ViewBag.GiangVien = giangVien;
            ViewBag.TongSinhVien = giangVien.DanhSachPhanCong.Count;
            ViewBag.DeCuongChoDuyet = giangVien.DanhSachPhanCong.Count(p => p.DeCuongThucTap != null && p.DeCuongThucTap.TrangThai == "cho_duyet");
            ViewBag.DaHoanThanh = giangVien.DanhSachPhanCong.Count(p => p.TrangThaiDuyet == "hoan_thanh");

            return View(giangVien.DanhSachPhanCong.ToList());
        }
    }
}
