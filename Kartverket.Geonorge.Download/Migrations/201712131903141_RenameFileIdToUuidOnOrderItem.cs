namespace Kartverket.Geonorge.Download.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class RenameFileIdToUuidOnOrderItem : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.orderItem", "IDX_FileId");
            RenameColumn("dbo.orderItem", "FileId", "Uuid");
            CreateIndex("dbo.orderItem", "Uuid", name: "IDX_Uuid");
        }
        
        public override void Down()
        {
            DropIndex("dbo.orderItem", "IDX_Uuid");
            RenameColumn("dbo.orderItem", "Uuid", "FileId");
            CreateIndex("dbo.orderItem", "FileId", name: "IDX_FileId");
        }
    }
}
