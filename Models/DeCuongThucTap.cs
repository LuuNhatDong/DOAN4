using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThucTap.Models
{
    [Table("de_cuong_thuc_tap")]
    public class DeCuongThucTap
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("phan_cong_id")]
        public long PhanCongId { get; set; }

        [Required]
        [Column("ten_de_tai")]
        [StringLength(255)]
        public string TenDeTai { get; set; } = string.Empty;

        [Required]
        [Column("muc_tieu")]
        public string MucTieu { get; set; } = string.Empty;

        [Required]
        [Column("ket_qua_du_kien")]
        public string KetQuaDuKien { get; set; } = string.Empty;

        [Required]
        [Column("cong_nghe_su_dung")]
        [StringLength(255)]
        public string CongNgheSuDung { get; set; } = string.Empty;

        [Required]
        [Column("noi_dung_ke_hoach", TypeName = "jsonb")]
        public string NoiDungKeHoach { get; set; } = "[]";

        [Column("nhan_xet_gvhd")]
        public string? NhanXetGvhd { get; set; }

        [Column("trang_thai")]
        [StringLength(30)]
        public string TrangThai { get; set; } = "cho_duyet"; // cho_duyet, da_duyet, yeu_cau_chinh_sua

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("PhanCongId")]
        public PhanCongHuongDan? PhanCongHuongDan { get; set; }
    }
}
