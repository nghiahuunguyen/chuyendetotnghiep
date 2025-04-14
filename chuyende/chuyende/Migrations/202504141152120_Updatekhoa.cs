namespace chuyende.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Updatekhoa : DbMigration
    {
        public override void Up()
        {
            // Cập nhật giá trị mặc định cho các cột cần thiết trong bảng Hang
            Sql("UPDATE dbo.Hang SET TenHang = 'Tên Hãng Mặc Định' WHERE TenHang IS NULL");
            Sql("UPDATE dbo.Hang SET Logo = 'Logo Mặc Định' WHERE Logo IS NULL");
            Sql("UPDATE dbo.Hang SET SoDienThoai = '000000000' WHERE SoDienThoai IS NULL");
            Sql("UPDATE dbo.Hang SET Email = 'example@example.com' WHERE Email IS NULL");
            Sql("UPDATE dbo.Hang SET DiaChi = 'Địa Chỉ Mặc Định' WHERE DiaChi IS NULL");
            Sql("UPDATE dbo.Hang SET TuKhoa = 'Từ Khóa Mặc Định' WHERE TuKhoa IS NULL");
            Sql("UPDATE dbo.Hang SET Link = 'http://default-link.com' WHERE Link IS NULL");

            AlterColumn("dbo.SanPham", "HinhAnh", c => c.String(nullable: false));
            AlterColumn("dbo.SanPham", "KhuyenMai", c => c.String(nullable: false));
            AlterColumn("dbo.SanPham", "TuKhoa", c => c.String(nullable: false));
            AlterColumn("dbo.SanPham", "GiaNhap", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.SanPham", "GiaDau", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.SanPham", "SoGiam", c => c.Int(nullable: false));
            AlterColumn("dbo.SanPham", "MoTa", c => c.String(nullable: false));
            AlterColumn("dbo.SanPham", "Link", c => c.String(nullable: false));
            AlterColumn("dbo.Hang", "TenHang", c => c.String(nullable: false));
            AlterColumn("dbo.Hang", "Logo", c => c.String(nullable: false));
            AlterColumn("dbo.Hang", "SoDienThoai", c => c.String(nullable: false));
            AlterColumn("dbo.Hang", "Email", c => c.String(nullable: false));
            AlterColumn("dbo.Hang", "DiaChi", c => c.String(nullable: false));
            AlterColumn("dbo.Hang", "TuKhoa", c => c.String(nullable: false));
            AlterColumn("dbo.Hang", "Link", c => c.String(nullable: false));
            AlterColumn("dbo.LoaiSanPham", "Link", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.LoaiSanPham", "Link", c => c.String());
            AlterColumn("dbo.Hang", "Link", c => c.String());
            AlterColumn("dbo.Hang", "TuKhoa", c => c.String());
            AlterColumn("dbo.Hang", "DiaChi", c => c.String());
            AlterColumn("dbo.Hang", "Email", c => c.String());
            AlterColumn("dbo.Hang", "SoDienThoai", c => c.String());
            AlterColumn("dbo.Hang", "Logo", c => c.String());
            AlterColumn("dbo.Hang", "TenHang", c => c.String());
            AlterColumn("dbo.SanPham", "Link", c => c.String());
            AlterColumn("dbo.SanPham", "MoTa", c => c.String());
            AlterColumn("dbo.SanPham", "SoGiam", c => c.Int());
            AlterColumn("dbo.SanPham", "GiaDau", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.SanPham", "GiaNhap", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.SanPham", "TuKhoa", c => c.String());
            AlterColumn("dbo.SanPham", "KhuyenMai", c => c.String());
            AlterColumn("dbo.SanPham", "HinhAnh", c => c.String());
        }
    }
}
