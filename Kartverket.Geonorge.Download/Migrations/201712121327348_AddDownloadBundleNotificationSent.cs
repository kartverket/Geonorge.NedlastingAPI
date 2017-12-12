namespace Kartverket.Geonorge.Download.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddDownloadBundleNotificationSent : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.orderDownload", "DownloadBundleNotificationSent");
        }
        
        public override void Down()
        {
            AddColumn("dbo.orderDownload", "DownloadBundleNotificationSent", c => c.DateTime());
        }
    }
}
