namespace chuyende.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDatabase1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SanPham", "Link", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SanPham", "Link");
        }
    }
}
