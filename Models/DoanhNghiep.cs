using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThucTap.Models
{
    [Table("doanh_nghiep")]
    public class DoanhNghiep
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("ma_so_thue")]
        [StringLength(20)]
        public string MaSoThue { get; set; } = string.Empty;

        [Required]
        [Column("ten_doanh_nghiep")]
        [StringLength(255)]
        public string TenDoanhNghiep { get; set; } = string.Empty;

        [Required]
        [Column("ten_viet_tat")]
        [StringLength(50)]
        public string TenVietTat { get; set; } = string.Empty;

        [Required]
        [Column("dia_chi")]
        public string DiaChi { get; set; } = string.Empty;

        [Column("website")]
        [StringLength(255)]
        public string? Website { get; set; }

        [Column("nguoi_lien_he")]
        [StringLength(100)]
        public string? NguoiLienHe { get; set; }

        [Column("sdt_lien_he")]
        [StringLength(15)]
        public string? SdtLienHe { get; set; }

        [Column("trang_thai")]
        [StringLength(20)]
        public string TrangThai { get; set; } = "hoat_dong"; // hoat_dong, ngung_hop_tac

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<PhanCongHuongDan> DanhSachPhanCong { get; set; } = new List<PhanCongHuongDan>();
    }
}
