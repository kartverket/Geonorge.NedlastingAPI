namespace Kartverket.Geonorge.Download.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOrderBundleFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.orderDownload", "DownloadAsBundle", c => c.Boolean(nullable: false));
            AddColumn("dbo.orderDownload", "DownloadBundleUrl", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.orderDownload", "DownloadBundleUrl");
            DropColumn("dbo.orderDownload", "DownloadAsBundle");
        }
    }
}
