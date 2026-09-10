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
        public async Task<IActionResult> ChiTiet(long id)
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

            return View(dot);
        }
    }
}
