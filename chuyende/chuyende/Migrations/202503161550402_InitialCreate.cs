namespace chuyende.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ChiTietHoaDon",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128),
                        MaHD = c.String(),
                        MaSP = c.String(),
                        SoLuong = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.ChucVu",
                c => new
                    {
                        MaCV = c.String(nullable: false, maxLength: 128),
                        TenCV = c.String(nullable: false, maxLength: 255),
                    })
                .PrimaryKey(t => t.MaCV);
            
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
                    })
                .PrimaryKey(t => t.MaHang);
            
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
                        GiaNhap = c.Decimal(nullable: false, precision: 18, scale: 2),
                        GiaDau = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SoGiam = c.Int(nullable: false),
                        Chip = c.String(),
                        Pin = c.String(),
                        CongSac = c.String(),
                        HeDieuHanh = c.String(),
                        DungLuong = c.String(),
                        Ram = c.String(),
                        ManHinh = c.String(),
                        TheSim = c.String(),
                        CamTruoc = c.String(),
                        CamSau = c.String(),
                        Mau = c.String(),
                        KetNoi = c.String(),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MaSP)
                .ForeignKey("dbo.Hang", t => t.MaHang, cascadeDelete: true)
                .ForeignKey("dbo.LoaiSanPham", t => t.MaLoaiSP, cascadeDelete: true)
                .Index(t => t.MaLoaiSP)
                .Index(t => t.MaHang);
            
            CreateTable(
                "dbo.LoaiSanPham",
                c => new
                    {
                        MaLoaiSP = c.String(nullable: false, maxLength: 128),
                        TenLoaiSP = c.String(nullable: false, maxLength: 255),
                    })
                .PrimaryKey(t => t.MaLoaiSP);
            
            CreateTable(
                "dbo.HoaDon",
                c => new
                    {
                        MaHD = c.String(nullable: false, maxLength: 128),
                        MaKH = c.String(nullable: false, maxLength: 128),
                        PhuongThucThanhToan = c.Int(nullable: false),
                        TrangThai = c.Int(nullable: false),
                        NguoiTao = c.String(nullable: false),
                        NgayTao = c.DateTime(nullable: false),
                        NhanVien_MaNV = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.MaHD)
                .ForeignKey("dbo.KhachHang", t => t.MaKH, cascadeDelete: true)
                .ForeignKey("dbo.NhanVien", t => t.NhanVien_MaNV)
                .Index(t => t.MaKH)
                .Index(t => t.NhanVien_MaNV);
            
            CreateTable(
                "dbo.KhachHang",
                c => new
                    {
                        MaKH = c.String(nullable: false, maxLength: 128),
                        TenKH = c.String(nullable: false, maxLength: 255),
                        NgaySinh = c.DateTime(nullable: false),
                        SoDienThoai = c.String(nullable: false, maxLength: 20),
                        Email = c.String(nullable: false),
                        DiaChi = c.String(nullable: false, maxLength: 500),
                        MatKhau = c.String(nullable: false),
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
                    })
                .PrimaryKey(t => t.MaNV)
                .ForeignKey("dbo.ChucVu", t => t.MaCV, cascadeDelete: true)
                .Index(t => t.MaCV);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.HoaDon", "NhanVien_MaNV", "dbo.NhanVien");
            DropForeignKey("dbo.NhanVien", "MaCV", "dbo.ChucVu");
            DropForeignKey("dbo.HoaDon", "MaKH", "dbo.KhachHang");
            DropForeignKey("dbo.SanPham", "MaLoaiSP", "dbo.LoaiSanPham");
            DropForeignKey("dbo.SanPham", "MaHang", "dbo.Hang");
            DropIndex("dbo.NhanVien", new[] { "MaCV" });
            DropIndex("dbo.HoaDon", new[] { "NhanVien_MaNV" });
            DropIndex("dbo.HoaDon", new[] { "MaKH" });
            DropIndex("dbo.SanPham", new[] { "MaHang" });
            DropIndex("dbo.SanPham", new[] { "MaLoaiSP" });
            DropTable("dbo.NhanVien");
            DropTable("dbo.KhachHang");
            DropTable("dbo.HoaDon");
            DropTable("dbo.LoaiSanPham");
            DropTable("dbo.SanPham");
            DropTable("dbo.Hang");
            DropTable("dbo.ChucVu");
            DropTable("dbo.ChiTietHoaDon");
        }
    }
}
