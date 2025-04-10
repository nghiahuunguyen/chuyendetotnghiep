namespace chuyende.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDatabaseloaibv : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.BaiViet", name: "LoaiBaiViet_MaLoaiBV", newName: "MaLoaiBV");
            RenameIndex(table: "dbo.BaiViet", name: "IX_LoaiBaiViet_MaLoaiBV", newName: "IX_MaLoaiBV");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.BaiViet", name: "IX_MaLoaiBV", newName: "IX_LoaiBaiViet_MaLoaiBV");
            RenameColumn(table: "dbo.BaiViet", name: "MaLoaiBV", newName: "LoaiBaiViet_MaLoaiBV");
        }
    }
}
