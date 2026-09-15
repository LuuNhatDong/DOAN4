using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThucTap.Models;

namespace QuanLyThucTap.Areas.SinhVien.Controllers
{
    [Area("SinhVien")]
    [Authorize(Roles = "sinh_vien")]
    public class DangKyController : Controller
    {
        private readonly ThucTapDbContext _context;

        public DangKyController(ThucTapDbContext context)
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
                .FirstOrDefaultAsync(s => s.Mssv == username);

            if (sinhVien == null) return NotFound();

            var phanCong = sinhVien.DanhSachPhanCong.OrderByDescending(p => p.Id).FirstOrDefault();
            
            // Lấy thông tin đợt thực tập cấu hình bởi Admin
            var dot = phanCong?.DotThucTap ?? 
                      await _context.DotThucTaps.FirstOrDefaultAsync(d => d.TrangThaiKichHoat) ?? 
                      await _context.DotThucTaps.OrderByDescending(d => d.Id).FirstOrDefaultAsync();

            int soTuan = 12;
            int soThang = 3;
            if (dot != null && dot.NgayKetThuc > dot.NgayBatDau)
            {
                var days = (dot.NgayKetThuc - dot.NgayBatDau).TotalDays;
                soTuan = Math.Max(1, (int)Math.Round(days / 7.0));
                soThang = Math.Max(1, (int)Math.Round(days / 30.0));
            }

            // Danh sách Giảng viên cố định của trường để SV đăng ký nguyện vọng
            ViewBag.DanhSachGiangVien = await _context.GiangViens
                .Include(g => g.DanhSachPhanCong)
                .Where(g => g.TrangThai == "hoat_dong")
                .OrderBy(g => g.Magv)
                .ToListAsync();

            ViewBag.DotId = dot?.Id ?? 0;
            ViewBag.DotThucTap = dot;
            ViewBag.SoTuan = soTuan;
            ViewBag.SoThang = soThang;
            ViewBag.SinhVien = sinhVien;

            return View(phanCong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(
            long? phanCongId, 
            long? giangVienId,
            string? tenCtyNgoai, 
            string? viTriThucTap)
        {
            var username = User.FindFirst("Username")?.Value;
            var sinhVien = await _context.SinhViens.FirstOrDefaultAsync(s => s.Mssv == username);
            if (sinhVien == null) return NotFound();

            PhanCongHuongDan? phanCong = null;
            if (phanCongId.HasValue && phanCongId.Value > 0)
            {
                phanCong = await _context.PhanCongHuongDans.FindAsync(phanCongId.Value);
            }

            if (phanCong == null)
            {
                var dotActive = await _context.DotThucTaps.FirstOrDefaultAsync(d => d.TrangThaiKichHoat)
                                ?? await _context.DotThucTaps.OrderByDescending(d => d.Id).FirstOrDefaultAsync();

                if (dotActive == null)
                {
                    TempData["Error"] = "Hiện chưa có đợt thực tập nào được mở đăng ký.";
                    return RedirectToAction(nameof(Index));
                }

                phanCong = new PhanCongHuongDan
                {
                    SinhVienId = sinhVien.Id,
                    DotThucTapId = dotActive.Id,
                    TrangThaiDuyet = "cho_duyet"
                };
                _context.PhanCongHuongDans.Add(phanCong);
            }

            // Thời gian thực tập CỐ ĐỊNH theo cấu hình của Admin trong Đợt thực tập
            var dot = await _context.DotThucTaps.FindAsync(phanCong.DotThucTapId);
            if (dot != null && dot.NgayKetThuc > dot.NgayBatDau)
            {
                var days = (dot.NgayKetThuc - dot.NgayBatDau).TotalDays;
                phanCong.SoThangThucTap = Math.Max(1, (int)Math.Round(days / 30.0));
            }
            else
            {
                phanCong.SoThangThucTap = 3;
            }

            // Kiểm tra GVHD nếu có chọn giảng viên
            if (giangVienId.HasValue && giangVienId.Value > 0)
            {
                var gv = await _context.GiangViens
                    .Include(g => g.DanhSachPhanCong)
                    .FirstOrDefaultAsync(g => g.Id == giangVienId.Value);

                if (gv != null)
                {
                    int count = gv.DanhSachPhanCong.Count(p => p.DotThucTapId == phanCong.DotThucTapId && p.TrangThaiDuyet != "tu_choi" && p.TrangThaiDuyet != "gv_tu_choi");
                    if (count >= gv.SoLuongToiDa && phanCong.GiangVienId != gv.Id)
                    {
                        TempData["Error"] = $"Giảng viên {gv.Hovaten} đã đủ chỉ tiêu hướng dẫn ({count}/{gv.SoLuongToiDa} SV). Vui lòng chọn giảng viên khác.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Nếu đổi GVHD hoặc gửi yêu cầu mới hoặc gửi lại sau khi bị từ chối
                    if (phanCong.GiangVienId != giangVienId || phanCong.TrangThaiDuyet == "gv_tu_choi" || phanCong.TrangThaiDuyet == "cho_duyet")
                    {
                        phanCong.GiangVienId = giangVienId;
                        phanCong.TrangThaiDuyet = "cho_gv_duyet"; // Chờ GVHD xác nhận
                        phanCong.LyDoTuChoi = null; // Xóa lý do từ chối cũ
                    }
                }
            }

            phanCong.TenCtyNgoai = string.IsNullOrWhiteSpace(tenCtyNgoai) ? "Doanh nghiệp CNTT thực tập" : tenCtyNgoai;
            phanCong.ViTriThucTap = string.IsNullOrWhiteSpace(viTriThucTap) ? "Thực tập sinh" : viTriThucTap;
            phanCong.MentorDoanhNghiep = null;
            phanCong.SdtMentor = null;
            phanCong.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đăng ký thông tin nơi thực tập và gửi yêu cầu đến Giảng viên hướng dẫn thành công!";

            return RedirectToAction(nameof(Index));
        }
    }
}
