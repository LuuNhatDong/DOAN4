using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThucTap.Models;

namespace QuanLyThucTap.Areas.SinhVien.Controllers
{
    [Area("SinhVien")]
    [Authorize(Roles = "sinh_vien")]
    public class DeCuongController : Controller
    {
        private readonly ThucTapDbContext _context;

        public DeCuongController(ThucTapDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var username = User.FindFirst("Username")?.Value;
            var sinhVien = await _context.SinhViens
                .Include(s => s.DanhSachPhanCong)
                    .ThenInclude(p => p.DeCuongThucTap)
                .Include(s => s.DanhSachPhanCong)
                    .ThenInclude(p => p.GiangVien)
                .FirstOrDefaultAsync(s => s.Mssv == username);

            if (sinhVien == null) return NotFound();

            var phanCong = sinhVien.DanhSachPhanCong.OrderByDescending(p => p.Id).FirstOrDefault();
            ViewBag.PhanCong = phanCong;
            ViewBag.SinhVien = sinhVien;

            var deCuong = phanCong?.DeCuongThucTap ?? new DeCuongThucTap { PhanCongId = phanCong?.Id ?? 0 };
            return View(deCuong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(DeCuongThucTap model)
        {
            if (model.PhanCongId == 0)
            {
                TempData["Error"] = "Chưa có thông tin phân công thực tập.";
                return RedirectToAction(nameof(Index));
            }

            var deCuong = await _context.DeCuongThucTaps.FirstOrDefaultAsync(d => d.PhanCongId == model.PhanCongId);

            if (deCuong == null)
            {
                model.TrangThai = "cho_duyet";
                model.CreatedAt = DateTime.UtcNow;
                model.UpdatedAt = DateTime.UtcNow;
                _context.DeCuongThucTaps.Add(model);
            }
            else
            {
                deCuong.TenDeTai = model.TenDeTai;
                deCuong.MucTieu = model.MucTieu;
                deCuong.KetQuaDuKien = model.KetQuaDuKien;
                deCuong.CongNgheSuDung = model.CongNgheSuDung;
                deCuong.NoiDungKeHoach = model.NoiDungKeHoach;
                deCuong.TrangThai = "cho_duyet";
                deCuong.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Nộp đề cương kế hoạch thực tập thành công!";

            return RedirectToAction(nameof(Index));
        }
    }
}
