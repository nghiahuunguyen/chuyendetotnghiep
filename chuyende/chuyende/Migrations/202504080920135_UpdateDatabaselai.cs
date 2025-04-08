namespace chuyende.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDatabaselai : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ChiTietGioHangs", "SoLuong", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ChiTietGioHangs", "SoLuong");
        }
    }
}
