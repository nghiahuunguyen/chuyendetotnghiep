namespace chuyende.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TenMigrationMoi : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.BaiViet", "MaLoaiBV", "dbo.LoaiBaiViet");
            DropIndex("dbo.BaiViet", new[] { "MaLoaiBV" });
            DropTable("dbo.BaiViet");
            DropTable("dbo.LoaiBaiViet");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.LoaiBaiViet",
                c => new
                    {
                        MaLoaiBV = c.String(nullable: false, maxLength: 128),
                        TenLoaiBV = c.String(nullable: false),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MaLoaiBV);
            
            CreateTable(
                "dbo.BaiViet",
                c => new
                    {
                        MaBV = c.String(nullable: false, maxLength: 128),
                        TenBV = c.String(nullable: false),
                        NoiDung = c.String(),
                        HinhAnh = c.String(),
                        Link = c.String(),
                        MaLoaiBV = c.String(maxLength: 128),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MaBV);
            
            CreateIndex("dbo.BaiViet", "MaLoaiBV");
            AddForeignKey("dbo.BaiViet", "MaLoaiBV", "dbo.LoaiBaiViet", "MaLoaiBV");
        }
    }
}
