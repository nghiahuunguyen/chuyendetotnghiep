namespace chuyende.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreateLienHe : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LienHe",
                c => new
                    {
                        MaLH = c.Int(nullable: false, identity: true),
                        HoTen = c.String(nullable: false, maxLength: 100),
                        Email = c.String(nullable: false, maxLength: 100),
                        SoDienThoai = c.String(maxLength: 20),
                        TieuDe = c.String(nullable: false, maxLength: 200),
                        NoiDung = c.String(nullable: false),
                        NgayGui = c.DateTime(nullable: false),
                        TrangThai = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MaLH);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.LienHe");
        }
    }
}
