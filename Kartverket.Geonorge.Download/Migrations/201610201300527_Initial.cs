namespace Kartverket.Geonorge.Download.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        //Uncomment up and down methods until deployed in environments that already have an existing database 
        public override void Up()
        {
            //CreateTable(
            //    "dbo.Dataset",
            //    c => new
            //        {
            //            ID = c.Int(nullable: false, identity: true),
            //            Tittel = c.String(maxLength: 255),
            //            metadataUuid = c.String(maxLength: 255),
            //            supportsAreaSelection = c.Boolean(),
            //            supportsFormatSelection = c.Boolean(),
            //            supportsPolygonSelection = c.Boolean(),
            //            supportsProjectionSelection = c.Boolean(),
            //            fmeklippeUrl = c.String(),
            //            mapSelectionLayer = c.String(),
            //        })
            //    .PrimaryKey(t => t.ID);
            
            //CreateTable(
            //    "dbo.filliste",
            //    c => new
            //        {
            //            filnavn = c.String(maxLength: 100),
            //            url = c.String(storeType: "ntext"),
            //            id = c.Guid(nullable: false),
            //            kategori = c.String(maxLength: 50),
            //            underkategori = c.String(maxLength: 50),
            //            inndeling = c.String(maxLength: 50),
            //            inndelingsverdi = c.String(maxLength: 100),
            //            projeksjon = c.String(maxLength: 100),
            //            format = c.String(maxLength: 100),
            //            dataset = c.Int(),
            //        })
            //    .PrimaryKey(t => t.id)
            //    .ForeignKey("dbo.Dataset", t => t.dataset)
            //    .Index(t => t.dataset);
            
            //CreateTable(
            //    "dbo.orderDownload",
            //    c => new
            //        {
            //            referenceNumber = c.Int(nullable: false, identity: true),
            //            email = c.String(maxLength: 50),
            //            orderDate = c.DateTime(),
            //        })
            //    .PrimaryKey(t => t.referenceNumber);
            
            //CreateTable(
            //    "dbo.orderItem",
            //    c => new
            //        {
            //            id = c.Int(nullable: false, identity: true),
            //            referenceNumber = c.Int(nullable: false),
            //            downloadUrl = c.String(),
            //            fileName = c.String(),
            //        })
            //    .PrimaryKey(t => t.id)
            //    .ForeignKey("dbo.orderDownload", t => t.referenceNumber, cascadeDelete: true)
            //    .Index(t => t.referenceNumber);
            
        }
        
        public override void Down()
        {
            //DropForeignKey("dbo.orderItem", "referenceNumber", "dbo.orderDownload");
            //DropForeignKey("dbo.filliste", "dataset", "dbo.Dataset");
            //DropIndex("dbo.orderItem", new[] { "referenceNumber" });
            //DropIndex("dbo.filliste", new[] { "dataset" });
            //DropTable("dbo.orderItem");
            //DropTable("dbo.orderDownload");
            //DropTable("dbo.filliste");
            //DropTable("dbo.Dataset");
        }
    }
}
