namespace chuyende.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDatabaseHang : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Hang", "Link", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Hang", "Link");
        }
    }
}
