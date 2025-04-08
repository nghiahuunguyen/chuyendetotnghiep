namespace chuyende.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDatabase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ChiTietHoaDon",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128),
                        MaHD = c.String(maxLength: 128),
                        MaSP = c.String(maxLength: 128),
                        SoLuong = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.HoaDon", t => t.MaHD)
                .ForeignKey("dbo.SanPham", t => t.MaSP)
                .Index(t => t.MaHD)
                .Index(t => t.MaSP);
            
            CreateTable(
                "dbo.HoaDon",
                c => new
                    {
                        MaHD = c.String(nullable: false, maxLength: 128),
                        TenKH = c.String(nullable: false, maxLength: 255),
                        SoDienThoai = c.String(nullable: false, maxLength: 20),
                        Email = c.String(nullable: false),
                        DiaChi = c.String(nullable: false, maxLength: 500),
                        PhuongThucThanhToan = c.Int(nullable: false),
                        TrangThai = c.Int(nullable: false),
                        NguoiTao = c.String(),
                        NgayTao = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.MaHD);
            
            CreateTable(
                "dbo.SanPham",
                c => new
                    {
                        MaSP = c.String(nullable: false, maxLength: 128),
                        MaLoaiSP = c.String(nullable: false, maxLength: 128),
                        MaHang = c.String(nullable: false, maxLength: 128),
                        TenSP = c.String(nullable: false, maxLength: 255),
                        HinhAnh = c.String(),
                        SoLuong = c.Int(nullable: false),
                        KhuyenMai = c.String(),
                        TuKhoa = c.String(),
                        GiaNhap = c.Decimal(precision: 18, scale: 2),
                        GiaDau = c.Decimal(precision: 18, scale: 2),
                        SoGiam = c.Int(),
                        MoTa = c.String(),
                        Status = c.Int(nullable: false),
                        Link = c.String(),
                    })
                .PrimaryKey(t => t.MaSP)
                .ForeignKey("dbo.Hang", t => t.MaHang, cascadeDelete: true)
                .ForeignKey("dbo.LoaiSanPham", t => t.MaLoaiSP, cascadeDelete: true)
                .Index(t => t.MaLoaiSP)
                .Index(t => t.MaHang);
            
            CreateTable(
                "dbo.Hang",
                c => new
                    {
                        MaHang = c.String(nullable: false, maxLength: 128),
                        TenHang = c.String(),
                        Logo = c.String(),
                        SoDienThoai = c.String(),
                        Email = c.String(),
                        DiaChi = c.String(),
                        TuKhoa = c.String(),
                        Status = c.Int(nullable: false),
                        Link = c.String(),
                    })
                .PrimaryKey(t => t.MaHang);
            
            CreateTable(
                "dbo.LoaiSanPham",
                c => new
                    {
                        MaLoaiSP = c.String(nullable: false, maxLength: 128),
                        TenLoaiSP = c.String(nullable: false, maxLength: 255),
                        Link = c.String(),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MaLoaiSP);
            
            CreateTable(
                "dbo.ChucVu",
                c => new
                    {
                        MaCV = c.String(nullable: false, maxLength: 128),
                        TenCV = c.String(nullable: false, maxLength: 255),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MaCV);
            
            CreateTable(
                "dbo.KhachHangs",
                c => new
                    {
                        MaKH = c.String(nullable: false, maxLength: 128),
                        TenKH = c.String(nullable: false, maxLength: 100),
                        SoDienThoai = c.String(nullable: false, maxLength: 15),
                        Email = c.String(nullable: false, maxLength: 100),
                        MatKhau = c.String(nullable: false, maxLength: 255),
                        IsActive = c.Boolean(nullable: false),
                        ActivationToken = c.String(),
                    })
                .PrimaryKey(t => t.MaKH);
            
            CreateTable(
                "dbo.NhanVien",
                c => new
                    {
                        MaNV = c.String(nullable: false, maxLength: 128),
                        TenNV = c.String(nullable: false, maxLength: 255),
                        SoDienThoai = c.String(nullable: false, maxLength: 10),
                        Email = c.String(nullable: false),
                        NgaySinh = c.DateTime(nullable: false),
                        GioiTinh = c.Boolean(nullable: false),
                        CCCD = c.String(nullable: false, maxLength: 12),
                        DiaChi = c.String(nullable: false, maxLength: 500),
                        TenDN = c.String(nullable: false, maxLength: 255),
                        MatKhau = c.String(nullable: false),
                        MaCV = c.String(nullable: false, maxLength: 128),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MaNV)
                .ForeignKey("dbo.ChucVu", t => t.MaCV, cascadeDelete: true)
                .Index(t => t.MaCV);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.NhanVien", "MaCV", "dbo.ChucVu");
            DropForeignKey("dbo.SanPham", "MaLoaiSP", "dbo.LoaiSanPham");
            DropForeignKey("dbo.SanPham", "MaHang", "dbo.Hang");
            DropForeignKey("dbo.ChiTietHoaDon", "MaSP", "dbo.SanPham");
            DropForeignKey("dbo.ChiTietHoaDon", "MaHD", "dbo.HoaDon");
            DropIndex("dbo.NhanVien", new[] { "MaCV" });
            DropIndex("dbo.SanPham", new[] { "MaHang" });
            DropIndex("dbo.SanPham", new[] { "MaLoaiSP" });
            DropIndex("dbo.ChiTietHoaDon", new[] { "MaSP" });
            DropIndex("dbo.ChiTietHoaDon", new[] { "MaHD" });
            DropTable("dbo.NhanVien");
            DropTable("dbo.KhachHangs");
            DropTable("dbo.ChucVu");
            DropTable("dbo.LoaiSanPham");
            DropTable("dbo.Hang");
            DropTable("dbo.SanPham");
            DropTable("dbo.HoaDon");
            DropTable("dbo.ChiTietHoaDon");
        }
    }
}
