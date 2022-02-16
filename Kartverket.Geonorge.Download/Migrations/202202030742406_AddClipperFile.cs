namespace Kartverket.Geonorge.Download.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddClipperFile : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ClipperFiles",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        DateUploaded = c.DateTime(nullable: false),
                        File = c.String(),
                        Valid = c.Boolean(nullable: false),
                        Message = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.orderItem", "ClipperFile", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.orderItem", "ClipperFile");
            DropTable("dbo.ClipperFiles");
        }
    }
}
