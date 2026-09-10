using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThucTap.Models
{
    [Table("bao_cao_tong_ket")]
    public class BaoCaoTongKet
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("phan_cong_id")]
        public long PhanCongId { get; set; }

        [Required]
        [Column("loai_bao_cao")]
        [StringLength(20)]
        public string LoaiBaoCao { get; set; } = "giua_ky"; // giua_ky, cuoi_ky

        [Required]
        [Column("tieu_de")]
        [StringLength(255)]
        public string TieuDe { get; set; } = string.Empty;

        [Required]
        [Column("tom_tat")]
        public string TomTat { get; set; } = string.Empty;

        [Required]
        [Column("file_bao_cao_url")]
        [StringLength(500)]
        public string FileBaoCaoUrl { get; set; } = string.Empty;

        [Column("link_github")]
        [StringLength(255)]
        public string? LinkGithub { get; set; }

        [Column("diem_so")]
        public decimal? DiemSo { get; set; }

        [Column("nhan_xet_gvhd")]
        public string? NhanXetGvhd { get; set; }

        [Column("ngay_nop")]
        public DateTime NgayNop { get; set; } = DateTime.UtcNow;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("PhanCongId")]
        public PhanCongHuongDan? PhanCongHuongDan { get; set; }
    }
}
