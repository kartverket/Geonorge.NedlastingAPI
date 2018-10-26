namespace Kartverket.Geonorge.Download.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRequestIdToDownloadUsageEntry : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DownloadUsage", "RequestId", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DownloadUsage", "RequestId");
        }
    }
}
