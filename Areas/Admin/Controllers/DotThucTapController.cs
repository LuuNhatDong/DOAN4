using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThucTap.Models;

namespace QuanLyThucTap.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class DotThucTapController : Controller
    {
        private readonly ThucTapDbContext _context;

        public DotThucTapController(ThucTapDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _context.DotThucTaps
                .OrderByDescending(d => d.Id)
                .ToListAsync();

            return View(list);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DotThucTap model)
        {
            if (model.TrangThaiKichHoat)
            {
                var others = await _context.DotThucTaps.ToListAsync();
                others.ForEach(o => o.TrangThaiKichHoat = false);
            }

            _context.DotThucTaps.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Tạo đợt thực tập mới thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var dot = await _context.DotThucTaps.FindAsync(id);
            if (dot == null) return NotFound();
            return View(dot);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, DotThucTap model)
        {
            if (id != model.Id) return NotFound();

            var dot = await _context.DotThucTaps.FindAsync(id);
            if (dot == null) return NotFound();

            dot.TenDot = model.TenDot;
            dot.NamHoc = model.NamHoc;
            dot.NgayBatDau = model.NgayBatDau;
            dot.NgayKetThuc = model.NgayKetThuc;
            dot.HanDangKyCty = model.HanDangKyCty;
            dot.HanNopDeCuong = model.HanNopDeCuong;
            dot.UpdatedAt = DateTime.UtcNow;

            if (model.TrangThaiKichHoat && !dot.TrangThaiKichHoat)
            {
                var others = await _context.DotThucTaps.Where(d => d.Id != id).ToListAsync();
                others.ForEach(o => o.TrangThaiKichHoat = false);
                dot.TrangThaiKichHoat = true;
            }
            else if (!model.TrangThaiKichHoat)
            {
                dot.TrangThaiKichHoat = false;
            }

            // Đồng bộ thời gian thực tập cố định cho tất cả sinh viên thuộc đợt này
            if (dot.NgayKetThuc > dot.NgayBatDau)
            {
                var days = (dot.NgayKetThuc - dot.NgayBatDau).TotalDays;
                int soThang = Math.Max(1, (int)Math.Round(days / 30.0));
                var phanCongs = await _context.PhanCongHuongDans.Where(p => p.DotThucTapId == id).ToListAsync();
                foreach (var pc in phanCongs)
                {
                    pc.SoThangThucTap = soThang;
                    pc.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật đợt thực tập và đồng bộ thời gian cố định thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetActive(long id)
        {
            var list = await _context.DotThucTaps.ToListAsync();
            foreach (var dot in list)
            {
                dot.TrangThaiKichHoat = (dot.Id == id);
            }

            await _context.SaveChangesAsync();

            // Đặt luôn cookie để đồng bộ kỳ đang xem
            Response.Cookies.Append(QuanLyThucTap.Helpers.SemesterHelper.CookieKey, id.ToString(), new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                Path = "/"
            });

            TempData["Success"] = "Đã kích hoạt đợt thực tập chính thức!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChonKy(long dotId, string? returnUrl = null)
        {
            Response.Cookies.Append(QuanLyThucTap.Helpers.SemesterHelper.CookieKey, dotId.ToString(), new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                Path = "/"
            });

            TempData["Success"] = "Đã chuyển sang xem dữ liệu của đợt thực tập được chọn!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }

        [HttpGet]
        public async Task<IActionResult> ChiTiet(long id, int page = 1)
        {
            var dot = await _context.DotThucTaps
                .Include(d => d.DanhSachPhanCong)
                    .ThenInclude(p => p.SinhVien)
                .Include(d => d.DanhSachPhanCong)
                    .ThenInclude(p => p.GiangVien)
                .Include(d => d.DanhSachPhanCong)
                    .ThenInclude(p => p.DoanhNghiep)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dot == null) return NotFound();

            // Lấy toàn bộ danh sách phân công hợp lệ trong đợt
            var allPhanCongs = dot.DanhSachPhanCong
                .Where(p => p.SinhVien != null && p.SinhVien.TrangThai == "hoat_dong")
                .OrderBy(p => p.SinhVien!.Mssv)
                .ToList();

            // Phân trang 10 sinh viên / trang
            int pageSize = 10;
            int totalItems = allPhanCongs.Count;
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var pagedPhanCongs = allPhanCongs
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.AllPhanCongs = allPhanCongs;
            ViewBag.PagedPhanCongs = pagedPhanCongs;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(dot);
        }
    }
}
