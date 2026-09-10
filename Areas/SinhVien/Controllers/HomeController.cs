using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThucTap.Models;

namespace QuanLyThucTap.Areas.SinhVien.Controllers
{
    [Area("SinhVien")]
    [Authorize(Roles = "sinh_vien")]
    public class HomeController : Controller
    {
        private readonly ThucTapDbContext _context;

        public HomeController(ThucTapDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var username = User.FindFirst("Username")?.Value;
            var sinhVien = await _context.SinhViens
                .Include(s => s.DanhSachPhanCong)
                    .ThenInclude(p => p.GiangVien)
                .Include(s => s.DanhSachPhanCong)
                    .ThenInclude(p => p.DoanhNghiep)
                .Include(s => s.DanhSachPhanCong)
                    .ThenInclude(p => p.DotThucTap)
                .Include(s => s.DanhSachPhanCong)
                    .ThenInclude(p => p.DeCuongThucTap)
                .Include(s => s.DanhSachPhanCong)
                    .ThenInclude(p => p.DanhSachBaoCaoDinhKy)
                .Include(s => s.DanhSachPhanCong)
                    .ThenInclude(p => p.DanhSachBaoCaoTongKet)
                .Include(s => s.DanhSachPhanCong)
                    .ThenInclude(p => p.DanhGiaKetQua)
                .FirstOrDefaultAsync(s => s.Mssv == username);

            if (sinhVien == null) return NotFound();

            var phanCong = sinhVien.DanhSachPhanCong.OrderByDescending(p => p.Id).FirstOrDefault();
            ViewBag.SinhVien = sinhVien;
            ViewBag.PhanCong = phanCong;

            return View(phanCong);
        }
    }
}
