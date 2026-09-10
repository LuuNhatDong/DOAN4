using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThucTap.Models;

namespace QuanLyThucTap.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    [Authorize(Roles = "giang_vien")]
    public class DeCuongController : Controller
    {
        private readonly ThucTapDbContext _context;

        public DeCuongController(ThucTapDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duyet(long deCuongId, string trangThai, string? nhanXetGvhd)
        {
            var deCuong = await _context.DeCuongThucTaps
                .Include(d => d.PhanCongHuongDan)
                .FirstOrDefaultAsync(d => d.Id == deCuongId);

            if (deCuong == null) return NotFound();

            deCuong.TrangThai = trangThai;
            deCuong.NhanXetGvhd = nhanXetGvhd;
            deCuong.UpdatedAt = DateTime.UtcNow;

            // Nếu duyệt đề cương, chuyển trạng thái phân công sang "dang_thuc_tap"
            if (trangThai == "da_duyet" && deCuong.PhanCongHuongDan != null)
            {
                deCuong.PhanCongHuongDan.TrangThaiDuyet = "dang_thuc_tap";
                deCuong.PhanCongHuongDan.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = trangThai == "da_duyet" ? "Đã phê duyệt đề cương thực tập!" : "Đã gửi yêu cầu chỉnh sửa đề cương!";

            return RedirectToAction("Detail", "SinhVien", new { phanCongId = deCuong.PhanCongId });
        }
    }
}
