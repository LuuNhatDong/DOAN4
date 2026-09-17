using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThucTap.Models;

namespace QuanLyThucTap.Areas.SinhVien.Controllers
{
    [Area("SinhVien")]
    [Authorize(Roles = "sinh_vien")]
    public class BaoCaoController : Controller
    {
        private readonly ThucTapDbContext _context;

        public BaoCaoController(ThucTapDbContext context)
        {
            _context = context;
        }

        // =====================================
        // 1. NHẬT KÝ ĐỊNH KỲ (TUẦN / THÁNG)
        // =====================================
        public async Task<IActionResult> DinhKy()
        {
            var username = User.FindFirst("Username")?.Value;
            var sinhVien = await _context.SinhViens
                .Include(s => s.DanhSachPhanCong)
                    .ThenInclude(p => p.DanhSachBaoCaoDinhKy)
                .Include(s => s.DanhSachPhanCong)
                    .ThenInclude(p => p.GiangVien)
                .Include(s => s.DanhSachPhanCong)
                    .ThenInclude(p => p.DeCuongThucTap)
                .Include(s => s.DanhSachPhanCong)
                    .ThenInclude(p => p.DotThucTap)
                .FirstOrDefaultAsync(s => s.Mssv == username);

            if (sinhVien == null) return NotFound();

            var phanCong = sinhVien.DanhSachPhanCong.OrderByDescending(p => p.Id).FirstOrDefault();
            ViewBag.PhanCong = phanCong;
            ViewBag.SinhVien = sinhVien;

            return View(phanCong?.DanhSachBaoCaoDinhKy.OrderBy(b => b.KyThu).ToList() ?? new List<BaoCaoDinhKy>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDinhKy(BaoCaoDinhKy model, IFormFile? filePdf, string? linkDrive)
        {
            if (model.PhanCongId == 0 || model.KyThu <= 0)
            {
                TempData["Error"] = "Dữ liệu kỳ báo cáo không hợp lệ.";
                return RedirectToAction(nameof(DinhKy));
            }

            var phanCong = await _context.PhanCongHuongDans
                .Include(p => p.DotThucTap)
                .FirstOrDefaultAsync(p => p.Id == model.PhanCongId);

            if (phanCong == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin phân công hướng dẫn.";
                return RedirectToAction(nameof(DinhKy));
            }

            // Kiểm tra số ngày làm việc: chu kỳ tháng tối đa 31 ngày, chu kỳ tuần tối đa 7 ngày, không âm
            bool isThang = phanCong.LoaiChuKyBaoCao == "theo_thang";
            int maxNgayLam = isThang ? 31 : 7;
            if (model.SoNgayLamViec < 0 || model.SoNgayLamViec > maxNgayLam)
            {
                TempData["Error"] = $"Số ngày làm việc không hợp lệ (phải từ 0 đến {maxNgayLam} ngày đối với chu kỳ {(isThang ? "tháng" : "tuần")}).";
                return RedirectToAction(nameof(DinhKy));
            }

            // Tự động tính toán ngày bắt đầu và ngày kết thúc chuẩn theo Đợt thực tập và Chu kỳ (không phụ thuộc client)
            DateTime dotStart = phanCong.DotThucTap?.NgayBatDau ?? (model.NgayBatDau != default ? model.NgayBatDau : DateTime.Today);
            DateTime dotEnd = phanCong.DotThucTap?.NgayKetThuc ?? dotStart.AddMonths(phanCong.SoThangThucTap > 0 ? phanCong.SoThangThucTap : 3);
            int soThang = phanCong.SoThangThucTap > 0 ? phanCong.SoThangThucTap : 3;
            int soKy = isThang ? soThang : soThang * 4;

            DateTime startDate;
            DateTime endDate;
            if (isThang)
            {
                startDate = dotStart.AddMonths(model.KyThu - 1);
                endDate = dotStart.AddMonths(model.KyThu).AddDays(-1);
            }
            else
            {
                startDate = dotStart.AddDays((model.KyThu - 1) * 7);
                endDate = dotStart.AddDays(model.KyThu * 7 - 1);
            }
            if (model.KyThu >= soKy || endDate > dotEnd)
            {
                endDate = dotEnd;
            }
            if (startDate > endDate)
            {
                startDate = dotEnd;
            }

            model.NgayBatDau = startDate;
            model.NgayKetThuc = endDate;

            // Xử lý upload file PDF (nếu có)
            string? uploadedFileUrl = null;
            if (filePdf != null && filePdf.Length > 0)
            {
                var ext = Path.GetExtension(filePdf.FileName).ToLowerInvariant();
                if (ext != ".pdf")
                {
                    TempData["Error"] = "Chỉ chấp nhận file đính kèm định dạng PDF (.pdf).";
                    return RedirectToAction(nameof(DinhKy));
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "baocao");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var username = User.FindFirst("Username")?.Value ?? "SV";
                var fileName = $"BaoCao_Ky{model.KyThu}_{username}_{DateTime.UtcNow.Ticks}.pdf";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await filePdf.CopyToAsync(stream);
                }

                uploadedFileUrl = $"/uploads/baocao/{fileName}";
            }

            var existing = await _context.BaoCaoDinhKies
                .FirstOrDefaultAsync(b => b.PhanCongId == model.PhanCongId && b.KyThu == model.KyThu);

            if (existing == null)
            {
                model.TrangThai = "da_nop";
                model.NgayNop = DateTime.UtcNow;
                model.CreatedAt = DateTime.UtcNow;
                model.UpdatedAt = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(uploadedFileUrl))
                {
                    model.FileBaoCaoUrl = uploadedFileUrl;
                }
                if (!string.IsNullOrWhiteSpace(linkDrive))
                {
                    model.LinkDrive = linkDrive.Trim();
                }
                _context.BaoCaoDinhKies.Add(model);
            }
            else
            {
                existing.NgayBatDau = startDate;
                existing.NgayKetThuc = endDate;
                existing.SoNgayLamViec = model.SoNgayLamViec;
                existing.CongViecHoanThanh = model.CongViecHoanThanh;
                existing.KienThucHocDuoc = model.KienThucHocDuoc;
                existing.KhoKhanVuongMac = model.KhoKhanVuongMac;
                if (!string.IsNullOrEmpty(uploadedFileUrl))
                {
                    existing.FileBaoCaoUrl = uploadedFileUrl;
                }
                if (!string.IsNullOrWhiteSpace(linkDrive))
                {
                    existing.LinkDrive = linkDrive.Trim();
                }
                existing.NgayNop = DateTime.UtcNow;
                existing.TrangThai = "da_nop";
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã nộp nhật ký kỳ {model.KyThu} thành công!";

            return RedirectToAction(nameof(DinhKy));
        }

        // =====================================
        // 2. BÁO CÁO TỔNG KẾT (GIỮA KỲ / CUỐI KỲ)
        // =====================================
        public async Task<IActionResult> TongKet()
        {
            var username = User.FindFirst("Username")?.Value;
            var sinhVien = await _context.SinhViens
                .Include(s => s.DanhSachPhanCong)
                    .ThenInclude(p => p.DanhSachBaoCaoTongKet)
                .Include(s => s.DanhSachPhanCong)
                    .ThenInclude(p => p.GiangVien)
                .FirstOrDefaultAsync(s => s.Mssv == username);

            if (sinhVien == null) return NotFound();

            var phanCong = sinhVien.DanhSachPhanCong.OrderByDescending(p => p.Id).FirstOrDefault();
            ViewBag.PhanCong = phanCong;
            ViewBag.SinhVien = sinhVien;

            return View(phanCong?.DanhSachBaoCaoTongKet.ToList() ?? new List<BaoCaoTongKet>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTongKet(BaoCaoTongKet model, IFormFile? fileBaoCao)
        {
            if (model.PhanCongId == 0 || string.IsNullOrWhiteSpace(model.LoaiBaoCao))
            {
                TempData["Error"] = "Thông tin bản báo cáo không hợp lệ.";
                return RedirectToAction(nameof(TongKet));
            }

            // Nếu upload file trực tiếp
            if (fileBaoCao != null && fileBaoCao.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var fileName = $"BaoCao_{model.LoaiBaoCao}_{DateTime.Now.Ticks}_{Path.GetFileName(fileBaoCao.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await fileBaoCao.CopyToAsync(stream);
                }

                model.FileBaoCaoUrl = $"/uploads/{fileName}";
            }

            if (string.IsNullOrWhiteSpace(model.FileBaoCaoUrl))
            {
                TempData["Error"] = "Vui lòng tải lên file báo cáo hoặc nhập liên kết file PDF.";
                return RedirectToAction(nameof(TongKet));
            }

            var existing = await _context.BaoCaoTongKets
                .FirstOrDefaultAsync(b => b.PhanCongId == model.PhanCongId && b.LoaiBaoCao == model.LoaiBaoCao);

            if (existing == null)
            {
                model.NgayNop = DateTime.UtcNow;
                model.CreatedAt = DateTime.UtcNow;
                model.UpdatedAt = DateTime.UtcNow;
                _context.BaoCaoTongKets.Add(model);
            }
            else
            {
                existing.TieuDe = model.TieuDe;
                existing.TomTat = model.TomTat;
                existing.FileBaoCaoUrl = model.FileBaoCaoUrl;
                existing.LinkGithub = model.LinkGithub;
                existing.NgayNop = DateTime.UtcNow;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã nộp báo cáo {(model.LoaiBaoCao == "giua_ky" ? "giữa kỳ" : "cuối kỳ")} thành công!";

            return RedirectToAction(nameof(TongKet));
        }
    }
}
