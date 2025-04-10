namespace chuyende.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDatabasenodunbv : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BaiViet", "NoiDung", c => c.String());
            DropColumn("dbo.BaiViet", "NôiDung");
        }
        
        public override void Down()
        {
            AddColumn("dbo.BaiViet", "NôiDung", c => c.String());
            DropColumn("dbo.BaiViet", "NoiDung");
        }
    }
}
