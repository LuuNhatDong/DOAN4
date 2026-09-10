using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThucTap.Models;

namespace QuanLyThucTap.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    [Authorize(Roles = "giang_vien")]
    public class SinhVienController : Controller
    {
        private readonly ThucTapDbContext _context;

        public SinhVienController(ThucTapDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var username = User.FindFirst("Username")?.Value;
            var gv = await _context.GiangViens.FirstOrDefaultAsync(g => g.Magv == username);
            if (gv == null) return NotFound();

            var list = await _context.PhanCongHuongDans
                .Include(p => p.SinhVien)
                .Include(p => p.DoanhNghiep)
                .Include(p => p.DeCuongThucTap)
                .Include(p => p.DanhSachBaoCaoDinhKy)
                .Include(p => p.DanhSachBaoCaoTongKet)
                .Include(p => p.DanhGiaKetQua)
                .Where(p => p.GiangVienId == gv.Id && p.SinhVien!.TrangThai == "hoat_dong" && p.TrangThaiDuyet != "cho_gv_duyet" && p.TrangThaiDuyet != "gv_tu_choi")
                .ToListAsync();

            return View(list);
        }

        public async Task<IActionResult> Detail(long phanCongId)
        {
            var phanCong = await _context.PhanCongHuongDans
                .Include(p => p.SinhVien)
                .Include(p => p.GiangVien)
                .Include(p => p.DoanhNghiep)
                .Include(p => p.DotThucTap)
                .Include(p => p.DeCuongThucTap)
                .Include(p => p.DanhSachBaoCaoDinhKy)
                .Include(p => p.DanhSachBaoCaoTongKet)
                .Include(p => p.DanhGiaKetQua)
                .FirstOrDefaultAsync(p => p.Id == phanCongId);

            if (phanCong == null) return NotFound();
            return View(phanCong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CauHinhTheoDoi(long phanCongId, string loaiChuKyBaoCao, bool yeuCauBaoCaoGiuaKy)
        {
            var phanCong = await _context.PhanCongHuongDans.FindAsync(phanCongId);
            if (phanCong == null) return NotFound();

            phanCong.LoaiChuKyBaoCao = loaiChuKyBaoCao;
            phanCong.YeuCauBaoCaoGiuaKy = yeuCauBaoCaoGiuaKy;
            phanCong.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật thiết lập chu kỳ theo dõi thành công!";
            return RedirectToAction(nameof(Detail), new { phanCongId });
        }
    }
}
