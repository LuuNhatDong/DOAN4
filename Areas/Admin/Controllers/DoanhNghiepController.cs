using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThucTap.Models;

namespace QuanLyThucTap.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class DoanhNghiepController : Controller
    {
        private readonly ThucTapDbContext _context;

        public DoanhNghiepController(ThucTapDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _context.DoanhNghieps
                .Include(d => d.DanhSachPhanCong)
                .OrderBy(d => d.TenVietTat)
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
        public async Task<IActionResult> Create(DoanhNghiep model)
        {
            if (string.IsNullOrWhiteSpace(model.MaSoThue) || string.IsNullOrWhiteSpace(model.TenDoanhNghiep) || string.IsNullOrWhiteSpace(model.TenVietTat))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin doanh nghiệp.");
                return View(model);
            }

            model.TrangThai = "hoat_dong";
            _context.DoanhNghieps.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm đối tác doanh nghiệp thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var dn = await _context.DoanhNghieps.FindAsync(id);
            if (dn == null) return NotFound();
            return View(dn);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DoanhNghiep model)
        {
            var dn = await _context.DoanhNghieps.FindAsync(model.Id);
            if (dn == null) return NotFound();

            dn.TenDoanhNghiep = model.TenDoanhNghiep;
            dn.TenVietTat = model.TenVietTat;
            dn.DiaChi = model.DiaChi;
            dn.Website = model.Website;
            dn.NguoiLienHe = model.NguoiLienHe;
            dn.SdtLienHe = model.SdtLienHe;
            dn.TrangThai = model.TrangThai;
            dn.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật doanh nghiệp thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
