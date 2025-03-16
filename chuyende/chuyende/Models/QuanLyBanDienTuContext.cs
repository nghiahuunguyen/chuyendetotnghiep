using chuyende.Models;
using System.Data.Entity;

public class QuanLyBanDienTuContext : DbContext
{
    public QuanLyBanDienTuContext()
        : base("name=QuanLyBanDienTu")
    {
    }

    public DbSet<Hang> Hangs { get; set; }
    public DbSet<LoaiSanPham> LoaiSanPhams { get; set; }
    public DbSet<SanPham> SanPhams { get; set; }
    public DbSet<ChucVu> ChucVus { get; set; }
    public DbSet<KhachHang> KhachHangs { get; set; }
    public DbSet<NhanVien> NhanViens { get; set; }
    public DbSet<HoaDon> HoaDons { get; set; }
    public DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; }
}
