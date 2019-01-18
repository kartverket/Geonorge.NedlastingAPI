namespace Kartverket.Geonorge.Download.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIndexes : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Dataset", "metadataUuid");
            CreateIndex("dbo.filliste", "inndeling");
            CreateIndex("dbo.filliste", "inndelingsverdi");
            CreateIndex("dbo.filliste", "projeksjon");
            CreateIndex("dbo.filliste", "format");
        }
        
        public override void Down()
        {
            DropIndex("dbo.filliste", new[] { "format" });
            DropIndex("dbo.filliste", new[] { "projeksjon" });
            DropIndex("dbo.filliste", new[] { "inndelingsverdi" });
            DropIndex("dbo.filliste", new[] { "inndeling" });
            DropIndex("dbo.Dataset", new[] { "metadataUuid" });
        }
    }
}
