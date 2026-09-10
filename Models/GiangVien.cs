using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThucTap.Models
{
    [Table("giang_vien")]
    public class GiangVien
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("tai_khoan_id")]
        public long TaiKhoanId { get; set; }

        [Required]
        [Column("magv")]
        [StringLength(20)]
        public string Magv { get; set; } = string.Empty;

        [Required]
        [Column("hovaten")]
        [StringLength(100)]
        public string Hovaten { get; set; } = string.Empty;

        [Required]
        [Column("hoc_vi")]
        [StringLength(50)]
        public string HocVi { get; set; } = string.Empty;

        [Required]
        [Column("bo_mon")]
        [StringLength(100)]
        public string BoMon { get; set; } = string.Empty;

        [Required]
        [Column("email")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Column("sdt")]
        [StringLength(15)]
        public string? Sdt { get; set; }

        [Column("so_luong_toi_da")]
        public int SoLuongToiDa { get; set; } = 15;

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
