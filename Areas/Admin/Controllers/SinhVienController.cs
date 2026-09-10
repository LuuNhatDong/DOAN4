using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThucTap.Helpers;
using QuanLyThucTap.Models;

namespace QuanLyThucTap.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class SinhVienController : Controller
    {
        private readonly ThucTapDbContext _context;

        public SinhVienController(ThucTapDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, string? lop, string? chuyenNganh, string? trangThaiPhanCong, int page = 1)
        {
            // 1. Lấy đợt thực tập đang chọn
            var dotId = await SemesterHelper.GetSelectedSemesterIdAsync(HttpContext, _context);
            ViewBag.DotId = dotId;
            ViewBag.DotHienTai = await _context.DotThucTaps.FindAsync(dotId);

            // 2. Query sinh viên
            var query = _context.SinhViens
                .Include(s => s.DanhSachPhanCong)
                    .ThenInclude(p => p.GiangVien)
                .Include(s => s.DanhSachPhanCong)
                    .ThenInclude(p => p.DoanhNghiep)
                .Where(s => s.TrangThai == "hoat_dong")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(s => s.Mssv.ToLower().Contains(search) || 
                                         s.Hovaten.ToLower().Contains(search) || 
                                         s.Email.ToLower().Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(lop))
            {
                query = query.Where(s => s.Lop == lop);
            }

            if (!string.IsNullOrWhiteSpace(chuyenNganh))
            {
                query = query.Where(s => s.ChuyenNganh == chuyenNganh);
            }

            // Lọc theo trạng thái phân công trong đợt đang xem
            if (!string.IsNullOrWhiteSpace(trangThaiPhanCong))
            {
                if (trangThaiPhanCong == "da_phan_cong")
                {
                    query = query.Where(s => s.DanhSachPhanCong.Any(p => p.DotThucTapId == dotId && p.GiangVienId != null && p.TrangThaiDuyet != "gv_tu_choi"));
                }
                else if (trangThaiPhanCong == "cho_duyet")
                {
                    query = query.Where(s => s.DanhSachPhanCong.Any(p => p.DotThucTapId == dotId && p.GiangVienId != null && (p.TrangThaiDuyet == "cho_duyet" || p.TrangThaiDuyet == "cho_gv_duyet")));
                }
                else if (trangThaiPhanCong == "chua_phan_cong")
                {
                    query = query.Where(s => !s.DanhSachPhanCong.Any(p => p.DotThucTapId == dotId && p.GiangVienId != null && p.TrangThaiDuyet != "gv_tu_choi"));
                }
            }

            // Phân trang 10 sinh viên / trang
            int pageSize = 10;
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var sinhViens = await query.OrderBy(s => s.Mssv)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Danh sách Giảng viên cố định của khoa để phân công kèm số lượng đang hướng dẫn
            var giangViens = await _context.GiangViens
                .Include(g => g.DanhSachPhanCong)
                .Where(g => g.TrangThai == "hoat_dong")
                .OrderBy(g => g.Magv)
                .ToListAsync();

            ViewBag.DanhSachGiangVien = giangViens;
            ViewBag.Search = search;
            ViewBag.Lop = lop;
            ViewBag.ChuyenNganh = chuyenNganh;
            ViewBag.TrangThaiPhanCong = trangThaiPhanCong;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;
            ViewBag.DanhSachLop = await _context.SinhViens.Where(s => s.TrangThai == "hoat_dong").Select(s => s.Lop).Distinct().OrderBy(x => x).ToListAsync();
            ViewBag.DanhSachChuyenNganh = await _context.SinhViens.Where(s => s.TrangThai == "hoat_dong").Select(s => s.ChuyenNganh).Distinct().OrderBy(x => x).ToListAsync();

            return View(sinhViens);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PhanCongGVHD(long sinhVienId, long giangVienId)
        {
            var dotId = await SemesterHelper.GetSelectedSemesterIdAsync(HttpContext, _context);
            if (dotId == 0)
            {
                TempData["Error"] = "Chưa chọn đợt thực tập hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            var gv = await _context.GiangViens
                .Include(g => g.DanhSachPhanCong)
                .FirstOrDefaultAsync(g => g.Id == giangVienId);

            if (gv == null)
            {
                TempData["Error"] = "Không tìm thấy giảng viên được chọn.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra hạn ngạch hướng dẫn
            int dangHuongDan = gv.DanhSachPhanCong.Count(p => p.DotThucTapId == dotId && p.TrangThaiDuyet != "tu_choi");
            if (dangHuongDan >= gv.SoLuongToiDa)
            {
                TempData["Error"] = $"Giảng viên {gv.Hovaten} đã đủ chỉ tiêu hướng dẫn ({dangHuongDan}/{gv.SoLuongToiDa} SV). Vui lòng chọn giảng viên khác.";
                return RedirectToAction(nameof(Index));
            }

            var phanCong = await _context.PhanCongHuongDans
                .FirstOrDefaultAsync(p => p.SinhVienId == sinhVienId && p.DotThucTapId == dotId);

            if (phanCong == null)
            {
                phanCong = new PhanCongHuongDan
                {
                    SinhVienId = sinhVienId,
                    DotThucTapId = dotId,
                    GiangVienId = giangVienId,
                    TrangThaiDuyet = "dang_thuc_tap",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.PhanCongHuongDans.Add(phanCong);
            }
            else
            {
                phanCong.GiangVienId = giangVienId;
                phanCong.TrangThaiDuyet = "dang_thuc_tap";
                phanCong.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã phân công GVHD {gv.Hovaten} cho sinh viên thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuyetNguyenVong(long phanCongId)
        {
            var phanCong = await _context.PhanCongHuongDans
                .Include(p => p.GiangVien)
                .FirstOrDefaultAsync(p => p.Id == phanCongId);

            if (phanCong == null) return NotFound();

            phanCong.TrangThaiDuyet = "dang_thuc_tap";
            phanCong.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã phê duyệt nguyện vọng GVHD của sinh viên!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyPhanCong(long phanCongId)
        {
            var phanCong = await _context.PhanCongHuongDans.FindAsync(phanCongId);
            if (phanCong != null)
            {
                phanCong.GiangVienId = null;
                phanCong.TrangThaiDuyet = "cho_duyet";
                phanCong.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã hủy phân công GVHD cho sinh viên này.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuanLyThucTap.Models.SinhVien model)
        {
            if (string.IsNullOrWhiteSpace(model.Mssv) || string.IsNullOrWhiteSpace(model.Hovaten) || string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("", "Vui lòng điền đầy đủ các thông tin bắt buộc.");
                return View(model);
            }

            // Kiểm tra trùng MSSV hoặc Email
            bool exists = await _context.SinhViens.AnyAsync(s => s.Mssv == model.Mssv || s.Email == model.Email);
            if (exists)
            {
                ModelState.AddModelError("", "MSSV hoặc Email đã tồn tại trong hệ thống.");
                return View(model);
            }

            // 1. Tạo tài khoản đăng nhập (mặc định 123456)
            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = model.Mssv,
                MatKhauHash = "$2b$10$w8uTqN82nQh92E4z524ZMe5JvXw0r0rFq6w3W8.123456hashed",
                VaiTro = "sinh_vien",
                TrangThai = "hoat_dong"
            };
            _context.TaiKhoans.Add(taiKhoan);
            await _context.SaveChangesAsync();

            // 2. Tạo hồ sơ sinh viên
            model.TaiKhoanId = taiKhoan.Id;
            model.TrangThai = "hoat_dong";
            _context.SinhViens.Add(model);
            await _context.SaveChangesAsync();

            // 3. Tự động tạo bản ghi phân công trong đợt đang xem
            var dotId = await SemesterHelper.GetSelectedSemesterIdAsync(HttpContext, _context);
            if (dotId > 0)
            {
                var phanCong = new PhanCongHuongDan
                {
                    SinhVienId = model.Id,
                    DotThucTapId = dotId,
                    TrangThaiDuyet = "cho_duyet"
                };
                _context.PhanCongHuongDans.Add(phanCong);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Thêm sinh viên mới thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var sv = await _context.SinhViens.FindAsync(id);
            if (sv == null || sv.TrangThai == "da_an") return NotFound();
            return View(sv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(QuanLyThucTap.Models.SinhVien model)
        {
            var sv = await _context.SinhViens.FindAsync(model.Id);
            if (sv == null) return NotFound();

            // Khóa cứng các thông tin học thuật, chỉ cho phép chỉnh sửa SĐT liên hệ
            sv.Sdt = model.Sdt;
            sv.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Cập nhật số điện thoại cho sinh viên {sv.Hovaten} thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var sv = await _context.SinhViens.FindAsync(id);
            if (sv != null)
            {
                sv.TrangThai = "da_an";
                sv.UpdatedAt = DateTime.UtcNow;

                var tk = await _context.TaiKhoans.FindAsync(sv.TaiKhoanId);
                if (tk != null)
                {
                    tk.TrangThai = "khoa";
                    tk.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã ẩn sinh viên và khóa tài khoản thành công!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
