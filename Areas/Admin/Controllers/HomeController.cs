using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThucTap.Models;

namespace QuanLyThucTap.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class HomeController : Controller
    {
        private readonly ThucTapDbContext _context;

        public HomeController(ThucTapDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Đợt thực tập đang xem
            var dotHienTai = await QuanLyThucTap.Helpers.SemesterHelper.GetSelectedSemesterAsync(HttpContext, _context);
            ViewBag.DotHienTai = dotHienTai;

            // 2. Thống kê tổng quan cố định trường/khoa
            ViewBag.TongSinhVien = await _context.SinhViens.CountAsync(s => s.TrangThai == "hoat_dong");
            ViewBag.TongGiangVien = await _context.GiangViens.CountAsync(g => g.TrangThai == "hoat_dong");
            ViewBag.TongDoanhNghiep = await _context.DoanhNghieps.CountAsync(d => d.TrangThai == "hoat_dong");

            // 3. Phân bố sinh viên theo 4 ngành Khoa CNTT
            var svQuery = _context.SinhViens.Where(s => s.TrangThai == "hoat_dong");
            ViewBag.SvHttt = await svQuery.CountAsync(s => s.ChuyenNganh.Contains("Hệ thống"));
            ViewBag.SvCntt = await svQuery.CountAsync(s => s.ChuyenNganh.Contains("Công nghệ"));
            ViewBag.SvKhmt = await svQuery.CountAsync(s => s.ChuyenNganh.Contains("Khoa học"));
            ViewBag.SvKtpm = await svQuery.CountAsync(s => s.ChuyenNganh.Contains("phần mềm"));

            // 4. Phân bố trạng thái thực tập trong đợt đang xem
            var query = _context.PhanCongHuongDans.AsQueryable();
            if (dotHienTai != null)
            {
                query = query.Where(p => p.DotThucTapId == dotHienTai.Id);
            }

            var danhSachPhanCong = await query
                .Include(p => p.DoanhNghiep)
                .Include(p => p.GiangVien)
                .Include(p => p.SinhVien)
                .ToListAsync();

            ViewBag.TongPhanCongDot = danhSachPhanCong.Count;
            ViewBag.DaPhanCongGVHD = danhSachPhanCong.Count(p => p.GiangVienId != null);
            ViewBag.ChuaPhanCongGVHD = danhSachPhanCong.Count(p => p.GiangVienId == null);

            ViewBag.ChoDuyet = danhSachPhanCong.Count(p => p.TrangThaiDuyet == "cho_duyet");
            ViewBag.DangThucTap = danhSachPhanCong.Count(p => p.TrangThaiDuyet == "dang_thuc_tap");
            ViewBag.HoanThanh = danhSachPhanCong.Count(p => p.TrangThaiDuyet == "hoan_thanh");
            ViewBag.TuChoi = danhSachPhanCong.Count(p => p.TrangThaiDuyet == "tu_choi");

            // 5. Doanh nghiệp tiếp nhận nhiều nhất (kết hợp cả cty đối tác và cty tự liên hệ)
            var topDoanhNghiep = danhSachPhanCong
                .Select(p => !string.IsNullOrEmpty(p.TenCtyNgoai) ? p.TenCtyNgoai : (p.DoanhNghiep != null ? p.DoanhNghiep.TenVietTat : "Chưa xác định"))
                .GroupBy(ten => ten)
                .Select(g => new { Ten = g.Key, SoLuong = g.Count() })
                .OrderByDescending(x => x.SoLuong)
                .Take(5)
                .ToList();

            ViewBag.TopDoanhNghiep = topDoanhNghiep;

            return View();
        }
    }
}
