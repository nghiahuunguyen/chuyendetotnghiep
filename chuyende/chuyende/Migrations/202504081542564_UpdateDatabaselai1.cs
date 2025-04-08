namespace chuyende.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDatabaselai1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.HoaDon", "TenKH", c => c.String());
            AlterColumn("dbo.HoaDon", "SoDienThoai", c => c.String());
            AlterColumn("dbo.HoaDon", "Email", c => c.String());
            AlterColumn("dbo.HoaDon", "DiaChi", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.HoaDon", "DiaChi", c => c.String(nullable: false, maxLength: 500));
            AlterColumn("dbo.HoaDon", "Email", c => c.String(nullable: false));
            AlterColumn("dbo.HoaDon", "SoDienThoai", c => c.String(nullable: false, maxLength: 20));
            AlterColumn("dbo.HoaDon", "TenKH", c => c.String(nullable: false, maxLength: 255));
        }
    }
}
