using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThucTap.Models;

namespace QuanLyThucTap.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class PhanCongController : Controller
    {
        private readonly ThucTapDbContext _context;

        public PhanCongController(ThucTapDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(long? dotId)
        {
            var dotHienTai = dotId.HasValue 
                ? await _context.DotThucTaps.FindAsync(dotId.Value)
                : await _context.DotThucTaps.FirstOrDefaultAsync(d => d.TrangThaiKichHoat);

            var query = _context.PhanCongHuongDans
                .Include(p => p.SinhVien)
                .Include(p => p.GiangVien)
                .Include(p => p.DoanhNghiep)
                .Include(p => p.DotThucTap)
                .AsQueryable();

            if (dotHienTai != null)
            {
                query = query.Where(p => p.DotThucTapId == dotHienTai.Id);
            }

            var list = await query.OrderBy(p => p.SinhVien!.Mssv).ToListAsync();

            ViewBag.DotHienTai = dotHienTai;
            ViewBag.DanhSachDot = await _context.DotThucTaps.OrderByDescending(d => d.Id).ToListAsync();
            ViewBag.GiangViens = await _context.GiangViens.Where(g => g.TrangThai == "hoat_dong").ToListAsync();

            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GanGiangVien(long phanCongId, long giangVienId)
        {
            var phanCong = await _context.PhanCongHuongDans.FindAsync(phanCongId);
            if (phanCong == null) return NotFound();

            phanCong.GiangVienId = giangVienId;
            phanCong.TrangThaiDuyet = "da_duyet";
            phanCong.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã phân công giảng viên hướng dẫn thành công!";
            return RedirectToAction(nameof(Index), new { dotId = phanCong.DotThucTapId });
        }
    }
}
