using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThucTap.Helpers;
using QuanLyThucTap.Models;

namespace QuanLyThucTap.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class GiangVienController : Controller
    {
        private readonly ThucTapDbContext _context;

        public GiangVienController(ThucTapDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, string? trangThai, int page = 1)
        {
            var dotId = await SemesterHelper.GetSelectedSemesterIdAsync(HttpContext, _context);
            ViewBag.DotId = dotId;

            var query = _context.GiangViens
                .Include(g => g.DanhSachPhanCong)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(g => g.Magv.ToLower().Contains(search) ||
                                         g.Hovaten.ToLower().Contains(search) ||
                                         g.Email.ToLower().Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(g => g.TrangThai == trangThai);
            }

            int pageSize = 10;
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var giangViens = await query.OrderBy(g => g.Magv)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.TrangThai = trangThai;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;

            return View(giangViens);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatSoLuong(long id, int soLuongToiDa)
        {
            if (soLuongToiDa < 1 || soLuongToiDa > 100)
            {
                TempData["Error"] = "Số lượng sinh viên hướng dẫn tối đa phải từ 1 đến 100.";
                return RedirectToAction(nameof(Index));
            }

            var gv = await _context.GiangViens.FindAsync(id);
            if (gv == null) return NotFound();

            gv.SoLuongToiDa = soLuongToiDa;
            gv.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã cập nhật chỉ tiêu hướng dẫn cho GV {gv.Hovaten}: {soLuongToiDa} SV.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAnHien(long id)
        {
            var gv = await _context.GiangViens.FindAsync(id);
            if (gv == null) return NotFound();

            var tk = await _context.TaiKhoans.FindAsync(gv.TaiKhoanId);

            if (gv.TrangThai == "hoat_dong")
            {
                gv.TrangThai = "da_an";
                if (tk != null) tk.TrangThai = "khoa";
                TempData["Success"] = $"Đã ẩn giảng viên {gv.Hovaten}.";
            }
            else
            {
                gv.TrangThai = "hoat_dong";
                if (tk != null) tk.TrangThai = "hoat_dong";
                TempData["Success"] = $"Đã khôi phục hiển thị giảng viên {gv.Hovaten}.";
            }

            gv.UpdatedAt = DateTime.UtcNow;
            if (tk != null) tk.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuanLyThucTap.Models.GiangVien model)
        {
            if (string.IsNullOrWhiteSpace(model.Magv) || string.IsNullOrWhiteSpace(model.Hovaten) || string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin giảng viên.");
                return View(model);
            }

            bool exists = await _context.GiangViens.AnyAsync(g => g.Magv == model.Magv || g.Email == model.Email);
            if (exists)
            {
                ModelState.AddModelError("", "Mã giảng viên hoặc Email đã tồn tại.");
                return View(model);
            }

            // Tạo tài khoản
            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = model.Magv,
                MatKhauHash = "$2b$10$w8uTqN82nQh92E4z524ZMe5JvXw0r0rFq6w3W8.123456hashed",
                VaiTro = "giang_vien",
                TrangThai = "hoat_dong"
            };
            _context.TaiKhoans.Add(taiKhoan);
            await _context.SaveChangesAsync();

            model.TaiKhoanId = taiKhoan.Id;
            model.TrangThai = "hoat_dong";
            model.BoMon = string.IsNullOrWhiteSpace(model.BoMon) ? "Hệ thống thông tin" : model.BoMon;
            _context.GiangViens.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm giảng viên thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var gv = await _context.GiangViens.FindAsync(id);
            if (gv == null) return NotFound();
            return View(gv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(QuanLyThucTap.Models.GiangVien model)
        {
            var gv = await _context.GiangViens.FindAsync(model.Id);
            if (gv == null) return NotFound();

            // Khóa cứng các thông tin học hàm, học vị, tài khoản, chỉ cho phép chỉnh sửa SĐT
            gv.Sdt = model.Sdt;
            gv.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Cập nhật số điện thoại cho giảng viên {gv.Hovaten} thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            return await ToggleAnHien(id);
        }
    }
}
