namespace chuyende.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDatabase : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LoaiSanPham", "Link", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.LoaiSanPham", "Link");
        }
    }
}
