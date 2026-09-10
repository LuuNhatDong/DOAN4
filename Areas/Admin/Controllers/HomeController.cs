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

            // 5. Thống kê phân công Giảng viên hướng dẫn trong đợt
            var topGiangVien = danhSachPhanCong
                .Where(p => p.GiangVien != null)
                .GroupBy(p => new { p.GiangVien!.Hovaten, p.GiangVien.HocVi, p.GiangVien.BoMon })
                .Select(g => new { 
                    Ten = g.Key.Hovaten, 
                    HocVi = g.Key.HocVi,
                    BoMon = g.Key.BoMon,
                    SoLuong = g.Count() 
                })
                .OrderByDescending(x => x.SoLuong)
                .Take(6)
                .ToList();

            ViewBag.TopGiangVien = topGiangVien;

            return View();
        }
    }
}
