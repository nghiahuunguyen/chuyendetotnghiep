namespace chuyende.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDatabasehd : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.HoaDon", "NguoiTao", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.HoaDon", "NguoiTao", c => c.String(nullable: false));
        }
    }
}
