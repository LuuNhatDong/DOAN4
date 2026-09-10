using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThucTap.Models
{
    [Table("sinh_vien")]
    public class SinhVien
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("tai_khoan_id")]
        public long TaiKhoanId { get; set; }

        [Required]
        [Column("mssv")]
        [StringLength(20)]
        public string Mssv { get; set; } = string.Empty;

        [Required]
        [Column("hovaten")]
        [StringLength(100)]
        public string Hovaten { get; set; } = string.Empty;

        [Required]
        [Column("lop")]
        [StringLength(20)]
        public string Lop { get; set; } = string.Empty;

        [Required]
        [Column("chuyen_nganh")]
        [StringLength(100)]
        public string ChuyenNganh { get; set; } = string.Empty;

        [Required]
        [Column("email")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Column("sdt")]
        [StringLength(15)]
        public string? Sdt { get; set; }

        [Column("gpa")]
        public decimal Gpa { get; set; } = 0.0m;

        [Column("trang_thai")]
        [StringLength(20)]
        public string TrangThai { get; set; } = "hoat_dong"; // hoat_dong, da_an

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("TaiKhoanId")]
        public TaiKhoan? TaiKhoan { get; set; }

        public ICollection<PhanCongHuongDan> DanhSachPhanCong { get; set; } = new List<PhanCongHuongDan>();
    }
}
