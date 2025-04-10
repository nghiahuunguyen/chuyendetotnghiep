namespace chuyende.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDatabasesp : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SanPham", "BanChay", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SanPham", "BanChay");
        }
    }
}
