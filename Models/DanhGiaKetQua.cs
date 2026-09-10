using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThucTap.Models
{
    [Table("danh_gia_ket_qua")]
    public class DanhGiaKetQua
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("phan_cong_id")]
        public long PhanCongId { get; set; }

        [Column("diem_dinh_ky")]
        public decimal DiemDinhKy { get; set; }

        [Column("diem_giua_ky")]
        public decimal DiemGiuaKy { get; set; }

        [Column("diem_cuoi_ky")]
        public decimal DiemCuoiKy { get; set; }

        [Column("diem_doanh_nghiep")]
        public decimal DiemDoanhNghiep { get; set; }

        [Column("diem_tong_ket_he10")]
        public decimal DiemTongKetHe10 { get; set; }

        [Required]
        [Column("diem_chu")]
        [StringLength(5)]
        public string DiemChu { get; set; } = "F";

        [Required]
        [Column("ket_qua")]
        [StringLength(20)]
        public string KetQua { get; set; } = "KHONG_DAT"; // DAT, KHONG_DAT

        [Column("nhan_xet_tong_ket")]
        public string? NhanXetTongKet { get; set; }

        [Column("ngay_chot_diem")]
        public DateTime NgayChotDiem { get; set; } = DateTime.UtcNow;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("PhanCongId")]
        public PhanCongHuongDan? PhanCongHuongDan { get; set; }
    }
}
