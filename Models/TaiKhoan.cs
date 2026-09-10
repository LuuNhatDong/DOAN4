using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThucTap.Models
{
    [Table("tai_khoan")]
    public class TaiKhoan
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("ten_dang_nhap")]
        [StringLength(100)]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required]
        [Column("mat_khau_hash")]
        [StringLength(255)]
        public string MatKhauHash { get; set; } = string.Empty;

        [Required]
        [Column("vai_tro")]
        [StringLength(20)]
        public string VaiTro { get; set; } = "sinh_vien"; // admin, giang_vien, sinh_vien

        [Column("trang_thai")]
        [StringLength(20)]
        public string TrangThai { get; set; } = "hoat_dong"; // hoat_dong, khoa

        [Column("auth_user_id")]
        public Guid? AuthUserId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public SinhVien? SinhVien { get; set; }
        public GiangVien? GiangVien { get; set; }
    }
}
