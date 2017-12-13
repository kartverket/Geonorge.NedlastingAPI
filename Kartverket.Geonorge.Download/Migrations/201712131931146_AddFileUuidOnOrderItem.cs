namespace Kartverket.Geonorge.Download.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddFileUuidOnOrderItem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.orderItem", "FileUuid", c => c.Guid());
            CreateIndex("dbo.orderItem", "FileUuid", name: "IDX_FileUuid");
        }
        
        public override void Down()
        {
            DropIndex("dbo.orderItem", "IDX_FileUuid");
            DropColumn("dbo.orderItem", "FileUuid");
        }
    }
}
