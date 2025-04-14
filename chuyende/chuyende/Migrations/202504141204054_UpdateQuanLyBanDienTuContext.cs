namespace chuyende.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateQuanLyBanDienTuContext : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Hang", "TenHang", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Hang", "TenHang", c => c.String());
        }
    }
}
