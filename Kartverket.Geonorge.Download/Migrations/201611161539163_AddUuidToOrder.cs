namespace Kartverket.Geonorge.Download.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUuidToOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.orderDownload", "Uuid", c => c.Guid(nullable: false));
            CreateIndex("dbo.orderDownload", "Uuid", name: "IDX_Uuid");
        }
        
        public override void Down()
        {
            DropIndex("dbo.orderDownload", "IDX_Uuid");
            DropColumn("dbo.orderDownload", "Uuid");
        }
    }
}
