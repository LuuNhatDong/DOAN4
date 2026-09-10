using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThucTap.Models
{
    [Table("bao_cao_dinh_ky")]
    public class BaoCaoDinhKy
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("phan_cong_id")]
        public long PhanCongId { get; set; }

        [Column("ky_thu")]
        public int KyThu { get; set; }

        [Column("ngay_bat_dau")]
        public DateTime NgayBatDau { get; set; }

        [Column("ngay_ket_thuc")]
        public DateTime NgayKetThuc { get; set; }

        [Column("so_ngay_lam_viec")]
        public int SoNgayLamViec { get; set; } = 5;

        [Required]
        [Column("cong_viec_hoan_thanh")]
        public string CongViecHoanThanh { get; set; } = string.Empty;

        [Required]
        [Column("kien_thuc_hoc_duoc")]
        public string KienThucHocDuoc { get; set; } = string.Empty;

        [Column("kho_khan_vuong_mac")]
        public string? KhoKhanVuongMac { get; set; }

        [Column("diem_so")]
        public decimal? DiemSo { get; set; }

        [Column("nhan_xet_gvhd")]
        public string? NhanXetGvhd { get; set; }

        [Column("nhan_xet_mentor")]
        public string? NhanXetMentor { get; set; }

        [Column("ngay_nop")]
        public DateTime NgayNop { get; set; } = DateTime.UtcNow;

        [Column("trang_thai")]
        [StringLength(20)]
        public string TrangThai { get; set; } = "da_nop"; // da_nop, da_cham, tre_han

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("PhanCongId")]
        public PhanCongHuongDan? PhanCongHuongDan { get; set; }
    }
}
