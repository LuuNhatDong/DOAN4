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
                .FirstOrDefaultAsync(s => s.Mssv == username);

            if (sinhVien == null) return NotFound();

            var phanCong = sinhVien.DanhSachPhanCong.OrderByDescending(p => p.Id).FirstOrDefault();
            ViewBag.PhanCong = phanCong;
            ViewBag.SinhVien = sinhVien;

            return View(phanCong?.DanhSachBaoCaoDinhKy.OrderBy(b => b.KyThu).ToList() ?? new List<BaoCaoDinhKy>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDinhKy(BaoCaoDinhKy model)
        {
            if (model.PhanCongId == 0 || model.KyThu <= 0)
            {
                TempData["Error"] = "Dữ liệu kỳ báo cáo không hợp lệ.";
                return RedirectToAction(nameof(DinhKy));
            }

            var existing = await _context.BaoCaoDinhKies
                .FirstOrDefaultAsync(b => b.PhanCongId == model.PhanCongId && b.KyThu == model.KyThu);

            if (existing == null)
            {
                model.TrangThai = "da_nop";
                model.NgayNop = DateTime.UtcNow;
                model.CreatedAt = DateTime.UtcNow;
                model.UpdatedAt = DateTime.UtcNow;
                _context.BaoCaoDinhKies.Add(model);
            }
            else
            {
                existing.NgayBatDau = model.NgayBatDau;
                existing.NgayKetThuc = model.NgayKetThuc;
                existing.SoNgayLamViec = model.SoNgayLamViec;
                existing.CongViecHoanThanh = model.CongViecHoanThanh;
                existing.KienThucHocDuoc = model.KienThucHocDuoc;
                existing.KhoKhanVuongMac = model.KhoKhanVuongMac;
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
