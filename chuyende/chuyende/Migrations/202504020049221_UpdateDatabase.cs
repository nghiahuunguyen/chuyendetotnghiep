namespace chuyende.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDatabase : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SanPham", "GiaNhap", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.SanPham", "GiaDau", c => c.Decimal(precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SanPham", "GiaDau", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.SanPham", "GiaNhap", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
