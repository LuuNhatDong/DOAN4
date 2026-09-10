using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThucTap.Models
{
    [Table("dot_thuc_tap")]
    public class DotThucTap
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("ten_dot")]
        [StringLength(150)]
        public string TenDot { get; set; } = string.Empty;

        [Required]
        [Column("nam_hoc")]
        [StringLength(20)]
        public string NamHoc { get; set; } = string.Empty;

        [Column("ngay_bat_dau")]
        public DateTime NgayBatDau { get; set; }

        [Column("ngay_ket_thuc")]
        public DateTime NgayKetThuc { get; set; }

        [Column("han_dang_ky_cty")]
        public DateTime HanDangKyCty { get; set; }

        [Column("han_nop_de_cuong")]
        public DateTime HanNopDeCuong { get; set; }

        [Column("trang_thai_kich_hoat")]
        public bool TrangThaiKichHoat { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<PhanCongHuongDan> DanhSachPhanCong { get; set; } = new List<PhanCongHuongDan>();
    }
}
