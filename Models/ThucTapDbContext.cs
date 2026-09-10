using Microsoft.EntityFrameworkCore;

namespace QuanLyThucTap.Models
{
    public class ThucTapDbContext : DbContext
    {
        public ThucTapDbContext(DbContextOptions<ThucTapDbContext> options) : base(options)
        {
        }

        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<SinhVien> SinhViens { get; set; }
        public DbSet<GiangVien> GiangViens { get; set; }
        public DbSet<DoanhNghiep> DoanhNghieps { get; set; }
        public DbSet<DotThucTap> DotThucTaps { get; set; }
        public DbSet<PhanCongHuongDan> PhanCongHuongDans { get; set; }
        public DbSet<DeCuongThucTap> DeCuongThucTaps { get; set; }
        public DbSet<BaoCaoDinhKy> BaoCaoDinhKies { get; set; }
        public DbSet<BaoCaoTongKet> BaoCaoTongKets { get; set; }
        public DbSet<DanhGiaKetQua> DanhGiaKetQuas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Quan hệ 1 - 1: TaiKhoan - SinhVien
            modelBuilder.Entity<SinhVien>()
                .HasOne(s => s.TaiKhoan)
                .WithOne(t => t.SinhVien)
                .HasForeignKey<SinhVien>(s => s.TaiKhoanId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ 1 - 1: TaiKhoan - GiangVien
            modelBuilder.Entity<GiangVien>()
                .HasOne(g => g.TaiKhoan)
                .WithOne(t => t.GiangVien)
                .HasForeignKey<GiangVien>(g => g.TaiKhoanId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ 1 - 1: PhanCongHuongDan - DeCuongThucTap
            modelBuilder.Entity<DeCuongThucTap>()
                .HasOne(d => d.PhanCongHuongDan)
                .WithOne(p => p.DeCuongThucTap)
                .HasForeignKey<DeCuongThucTap>(d => d.PhanCongId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ 1 - 1: PhanCongHuongDan - DanhGiaKetQua
            modelBuilder.Entity<DanhGiaKetQua>()
                .HasOne(d => d.PhanCongHuongDan)
                .WithOne(p => p.DanhGiaKetQua)
                .HasForeignKey<DanhGiaKetQua>(d => d.PhanCongId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ràng buộc Unique
            modelBuilder.Entity<PhanCongHuongDan>()
                .HasIndex(p => new { p.SinhVienId, p.DotThucTapId })
                .IsUnique();

            modelBuilder.Entity<BaoCaoDinhKy>()
                .HasIndex(b => new { b.PhanCongId, b.KyThu })
                .IsUnique();

            modelBuilder.Entity<BaoCaoTongKet>()
                .HasIndex(b => new { b.PhanCongId, b.LoaiBaoCao })
                .IsUnique();
        }
    }
}
