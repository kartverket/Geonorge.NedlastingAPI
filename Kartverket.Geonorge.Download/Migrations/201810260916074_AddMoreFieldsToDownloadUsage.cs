namespace Kartverket.Geonorge.Download.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMoreFieldsToDownloadUsage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DownloadUsage", "AreaCode", c => c.String());
            AddColumn("dbo.DownloadUsage", "AreaName", c => c.String());
            AddColumn("dbo.DownloadUsage", "Format", c => c.String());
            AddColumn("dbo.DownloadUsage", "Projection", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DownloadUsage", "Projection");
            DropColumn("dbo.DownloadUsage", "Format");
            DropColumn("dbo.DownloadUsage", "AreaName");
            DropColumn("dbo.DownloadUsage", "AreaCode");
        }
    }
}
