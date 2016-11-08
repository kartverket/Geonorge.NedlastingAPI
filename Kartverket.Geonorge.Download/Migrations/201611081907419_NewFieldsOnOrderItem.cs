namespace Kartverket.Geonorge.Download.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewFieldsOnOrderItem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.orderDownload", "username", c => c.String());
            AddColumn("dbo.orderItem", "FileId", c => c.Guid(nullable: false));
            AddColumn("dbo.orderItem", "Format", c => c.String());
            AddColumn("dbo.orderItem", "Area", c => c.String());
            AddColumn("dbo.orderItem", "Coordinates", c => c.String());
            AddColumn("dbo.orderItem", "CoordinateSystem", c => c.String());
            AddColumn("dbo.orderItem", "Projection", c => c.String());
            AddColumn("dbo.orderItem", "MetadataUuid", c => c.String());
            CreateIndex("dbo.orderItem", "FileId", name: "IDX_FileId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.orderItem", "IDX_FileId");
            DropColumn("dbo.orderItem", "MetadataUuid");
            DropColumn("dbo.orderItem", "Projection");
            DropColumn("dbo.orderItem", "CoordinateSystem");
            DropColumn("dbo.orderItem", "Coordinates");
            DropColumn("dbo.orderItem", "Area");
            DropColumn("dbo.orderItem", "Format");
            DropColumn("dbo.orderItem", "FileId");
            DropColumn("dbo.orderDownload", "username");
        }
    }
}
