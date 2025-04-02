namespace chuyende.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDatabaseHD : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.HoaDon", "NgaySinh");
        }
        
        public override void Down()
        {
            AddColumn("dbo.HoaDon", "NgaySinh", c => c.DateTime(nullable: false));
        }
    }
}
