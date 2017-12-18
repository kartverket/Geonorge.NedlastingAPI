namespace Kartverket.Geonorge.Download.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDownloadBundleNotificationSent : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.orderDownload", "DownloadBundleNotificationSent", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.orderDownload", "DownloadBundleNotificationSent");
        }
    }
}
