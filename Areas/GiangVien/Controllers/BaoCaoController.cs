using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThucTap.Models;

namespace QuanLyThucTap.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    [Authorize(Roles = "giang_vien")]
    public class BaoCaoController : Controller
    {
        private readonly ThucTapDbContext _context;

        public BaoCaoController(ThucTapDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChamDiemDinhKy(long id, decimal diemSo, string? nhanXetGvhd, string? nhanXetMentor)
        {
            var baoCao = await _context.BaoCaoDinhKies.FindAsync(id);
            if (baoCao == null) return NotFound();

            baoCao.DiemSo = diemSo;
            baoCao.NhanXetGvhd = nhanXetGvhd;
            baoCao.NhanXetMentor = nhanXetMentor;
            baoCao.TrangThai = "da_cham";
            baoCao.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã chấm điểm nhật ký kỳ {baoCao.KyThu} thành công!";
            return RedirectToAction("Detail", "SinhVien", new { phanCongId = baoCao.PhanCongId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChamDiemTongKet(long id, decimal diemSo, string? nhanXetGvhd)
        {
            var baoCao = await _context.BaoCaoTongKets.FindAsync(id);
            if (baoCao == null) return NotFound();

            baoCao.DiemSo = diemSo;
            baoCao.NhanXetGvhd = nhanXetGvhd;
            baoCao.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã chấm điểm báo cáo {(baoCao.LoaiBaoCao == "giua_ky" ? "giữa kỳ" : "cuối kỳ")} thành công!";
            return RedirectToAction("Detail", "SinhVien", new { phanCongId = baoCao.PhanCongId });
        }
    }
}
