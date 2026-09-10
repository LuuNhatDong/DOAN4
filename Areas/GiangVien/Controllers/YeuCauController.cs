using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThucTap.Models;

namespace QuanLyThucTap.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    [Authorize(Roles = "giang_vien")]
    public class YeuCauController : Controller
    {
        private readonly ThucTapDbContext _context;

        public YeuCauController(ThucTapDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var username = User.FindFirst("Username")?.Value;
            var gv = await _context.GiangViens
                .Include(g => g.DanhSachPhanCong)
                .FirstOrDefaultAsync(g => g.Magv == username);

            if (gv == null) return NotFound();

            // Lấy danh sách các yêu cầu sinh viên gửi đến GV này
            var yeuCaus = await _context.PhanCongHuongDans
                .Include(p => p.SinhVien)
                .Include(p => p.DoanhNghiep)
                .Include(p => p.DotThucTap)
                .Where(p => p.GiangVienId == gv.Id && (p.TrangThaiDuyet == "cho_gv_duyet" || p.TrangThaiDuyet == "gv_tu_choi"))
                .OrderByDescending(p => p.UpdatedAt)
                .ToListAsync();

            // Đếm số sinh viên đang hướng dẫn chính thức
            int dangHuongDan = gv.DanhSachPhanCong.Count(p => p.TrangThaiDuyet == "dang_thuc_tap" || p.TrangThaiDuyet == "da_duyet" || p.TrangThaiDuyet == "hoan_thanh");

            ViewBag.GiangVien = gv;
            ViewBag.DangHuongDan = dangHuongDan;
            ViewBag.ConLai = Math.Max(0, gv.SoLuongToiDa - dangHuongDan);

            return View(yeuCaus);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChapNhan(long phanCongId)
        {
            var username = User.FindFirst("Username")?.Value;
            var gv = await _context.GiangViens
                .Include(g => g.DanhSachPhanCong)
                .FirstOrDefaultAsync(g => g.Magv == username);

            if (gv == null) return NotFound();

            var phanCong = await _context.PhanCongHuongDans
                .Include(p => p.SinhVien)
                .FirstOrDefaultAsync(p => p.Id == phanCongId && p.GiangVienId == gv.Id);

            if (phanCong == null)
            {
                TempData["Error"] = "Không tìm thấy yêu cầu hướng dẫn.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra hạn mức chỉ tiêu
            int dangHuongDan = gv.DanhSachPhanCong.Count(p => p.DotThucTapId == phanCong.DotThucTapId && (p.TrangThaiDuyet == "dang_thuc_tap" || p.TrangThaiDuyet == "da_duyet" || p.TrangThaiDuyet == "hoan_thanh"));
            if (dangHuongDan >= gv.SoLuongToiDa)
            {
                TempData["Error"] = $"Bạn đã đạt giới hạn chỉ tiêu hướng dẫn ({dangHuongDan}/{gv.SoLuongToiDa} sinh viên). Không thể tiếp nhận thêm.";
                return RedirectToAction(nameof(Index));
            }

            phanCong.TrangThaiDuyet = "dang_thuc_tap";
            phanCong.LyDoTuChoi = null;
            phanCong.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã chấp nhận hướng dẫn sinh viên {phanCong.SinhVien?.Hovaten} ({phanCong.SinhVien?.Mssv}) thành công!";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TuChoi(long phanCongId, string lyDo)
        {
            var username = User.FindFirst("Username")?.Value;
            var gv = await _context.GiangViens.FirstOrDefaultAsync(g => g.Magv == username);
            if (gv == null) return NotFound();

            if (string.IsNullOrWhiteSpace(lyDo))
            {
                TempData["Error"] = "Vui lòng nhập lý do từ chối để sinh viên có căn cứ chọn giảng viên khác.";
                return RedirectToAction(nameof(Index));
            }

            var phanCong = await _context.PhanCongHuongDans
                .Include(p => p.SinhVien)
                .FirstOrDefaultAsync(p => p.Id == phanCongId && p.GiangVienId == gv.Id);

            if (phanCong == null)
            {
                TempData["Error"] = "Không tìm thấy yêu cầu hướng dẫn.";
                return RedirectToAction(nameof(Index));
            }

            phanCong.TrangThaiDuyet = "gv_tu_choi";
            phanCong.LyDoTuChoi = lyDo.Trim();
            phanCong.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã từ chối tiếp nhận sinh viên {phanCong.SinhVien?.Hovaten}. Sinh viên đã nhận được phản hồi.";

            return RedirectToAction(nameof(Index));
        }
    }
}
