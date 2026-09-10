using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThucTap.Models;

namespace QuanLyThucTap.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    [Authorize(Roles = "giang_vien")]
    public class DanhGiaController : Controller
    {
        private readonly ThucTapDbContext _context;

        public DanhGiaController(ThucTapDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChotDiem(
            long phanCongId, 
            decimal diemDinhKy, 
            decimal diemGiuaKy, 
            decimal diemCuoiKy, 
            decimal diemDoanhNghiep, 
            string? nhanXetTongKet, 
            DateTime? ngayChotDiem)
        {
            var phanCong = await _context.PhanCongHuongDans
                .Include(p => p.DanhGiaKetQua)
                .FirstOrDefaultAsync(p => p.Id == phanCongId);

            if (phanCong == null) return NotFound();

            // Tính điểm tổng kết hệ 10 có trọng số: 20% định kỳ + 20% giữa kỳ + 30% cuối kỳ + 30% doanh nghiệp
            decimal diemTongKet = Math.Round((diemDinhKy * 0.20m) + (diemGiuaKy * 0.20m) + (diemCuoiKy * 0.30m) + (diemDoanhNghiep * 0.30m), 2);

            // Quy đổi thang điểm chữ
            string diemChu = "F";
            if (diemTongKet >= 9.0m) diemChu = "A+";
            else if (diemTongKet >= 8.5m) diemChu = "A";
            else if (diemTongKet >= 8.0m) diemChu = "B+";
            else if (diemTongKet >= 7.0m) diemChu = "B";
            else if (diemTongKet >= 6.5m) diemChu = "C+";
            else if (diemTongKet >= 5.5m) diemChu = "C";
            else if (diemTongKet >= 5.0m) diemChu = "D+";
            else if (diemTongKet >= 4.0m) diemChu = "D";
            else diemChu = "F";

            string ketQua = diemTongKet >= 4.0m ? "DAT" : "KHONG_DAT";
            var ngayChot = ngayChotDiem ?? DateTime.UtcNow;

            if (phanCong.DanhGiaKetQua == null)
            {
                var dg = new DanhGiaKetQua
                {
                    PhanCongId = phanCongId,
                    DiemDinhKy = diemDinhKy,
                    DiemGiuaKy = diemGiuaKy,
                    DiemCuoiKy = diemCuoiKy,
                    DiemDoanhNghiep = diemDoanhNghiep,
                    DiemTongKetHe10 = diemTongKet,
                    DiemChu = diemChu,
                    KetQua = ketQua,
                    NhanXetTongKet = nhanXetTongKet,
                    NgayChotDiem = ngayChot
                };
                _context.DanhGiaKetQuas.Add(dg);
            }
            else
            {
                var dg = phanCong.DanhGiaKetQua;
                dg.DiemDinhKy = diemDinhKy;
                dg.DiemGiuaKy = diemGiuaKy;
                dg.DiemCuoiKy = diemCuoiKy;
                dg.DiemDoanhNghiep = diemDoanhNghiep;
                dg.DiemTongKetHe10 = diemTongKet;
                dg.DiemChu = diemChu;
                dg.KetQua = ketQua;
                dg.NhanXetTongKet = nhanXetTongKet;
                dg.NgayChotDiem = ngayChot;
                dg.UpdatedAt = DateTime.UtcNow;
            }

            // Đổi trạng thái sang 'hoan_thanh'
            phanCong.TrangThaiDuyet = "hoan_thanh";
            phanCong.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã chốt bảng điểm tổng kết và hoàn thành học kỳ thực tập cho sinh viên!";

            return RedirectToAction("Detail", "SinhVien", new { phanCongId });
        }
    }
}
