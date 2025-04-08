namespace chuyende.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDatabasegh : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ChiTietGioHangs",
                c => new
                    {
                        MaChiTiet = c.String(nullable: false, maxLength: 128),
                        MaGioHang = c.String(nullable: false, maxLength: 128),
                        MaSP = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.MaChiTiet)
                .ForeignKey("dbo.GioHangs", t => t.MaGioHang, cascadeDelete: true)
                .ForeignKey("dbo.SanPham", t => t.MaSP, cascadeDelete: true)
                .Index(t => t.MaGioHang)
                .Index(t => t.MaSP);
            
            CreateTable(
                "dbo.GioHangs",
                c => new
                    {
                        MaGioHang = c.String(nullable: false, maxLength: 128),
                        MaKH = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.MaGioHang)
                .ForeignKey("dbo.KhachHangs", t => t.MaKH, cascadeDelete: true)
                .Index(t => t.MaKH);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChiTietGioHangs", "MaSP", "dbo.SanPham");
            DropForeignKey("dbo.GioHangs", "MaKH", "dbo.KhachHangs");
            DropForeignKey("dbo.ChiTietGioHangs", "MaGioHang", "dbo.GioHangs");
            DropIndex("dbo.GioHangs", new[] { "MaKH" });
            DropIndex("dbo.ChiTietGioHangs", new[] { "MaSP" });
            DropIndex("dbo.ChiTietGioHangs", new[] { "MaGioHang" });
            DropTable("dbo.GioHangs");
            DropTable("dbo.ChiTietGioHangs");
        }
    }
}
