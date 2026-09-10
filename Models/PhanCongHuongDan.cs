using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThucTap.Models
{
    [Table("phan_cong_huong_dan")]
    public class PhanCongHuongDan
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("sinh_vien_id")]
        public long SinhVienId { get; set; }

        [Column("giang_vien_id")]
        public long? GiangVienId { get; set; }

        [Column("doanh_nghiep_id")]
        public long? DoanhNghiepId { get; set; }

        [Column("dot_thuc_tap_id")]
        public long DotThucTapId { get; set; }

        [Column("ten_cty_ngoai")]
        [StringLength(255)]
        public string? TenCtyNgoai { get; set; }

        [Column("vi_tri_thuc_tap")]
        [StringLength(100)]
        public string? ViTriThucTap { get; set; }

        [Column("mentor_doanh_nghiep")]
        [StringLength(100)]
        public string? MentorDoanhNghiep { get; set; }

        [Column("sdt_mentor")]
        [StringLength(15)]
        public string? SdtMentor { get; set; }

        [Column("loai_chu_ky_bao_cao")]
        [StringLength(20)]
        public string LoaiChuKyBaoCao { get; set; } = "theo_tuan"; // theo_tuan, theo_thang

        [Column("yeu_cau_bao_cao_giua_ky")]
        public bool YeuCauBaoCaoGiuaKy { get; set; } = true;

        [Column("so_thang_thuc_tap")]
        public int SoThangThucTap { get; set; } = 3; // 2 tháng, 3 tháng, 4 tháng...

        [Column("ly_do_tu_choi")]
        public string? LyDoTuChoi { get; set; }

        [Column("trang_thai_duyet")]
        [StringLength(30)]
        public string TrangThaiDuyet { get; set; } = "cho_duyet"; // cho_duyet, cho_gv_duyet, gv_tu_choi, da_duyet, dang_thuc_tap, hoan_thanh

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("SinhVienId")]
        public SinhVien? SinhVien { get; set; }

        [ForeignKey("GiangVienId")]
        public GiangVien? GiangVien { get; set; }

        [ForeignKey("DoanhNghiepId")]
        public DoanhNghiep? DoanhNghiep { get; set; }

        [ForeignKey("DotThucTapId")]
        public DotThucTap? DotThucTap { get; set; }

        public DeCuongThucTap? DeCuongThucTap { get; set; }
        public DanhGiaKetQua? DanhGiaKetQua { get; set; }
        public ICollection<BaoCaoDinhKy> DanhSachBaoCaoDinhKy { get; set; } = new List<BaoCaoDinhKy>();
        public ICollection<BaoCaoTongKet> DanhSachBaoCaoTongKet { get; set; } = new List<BaoCaoTongKet>();
    }
}
