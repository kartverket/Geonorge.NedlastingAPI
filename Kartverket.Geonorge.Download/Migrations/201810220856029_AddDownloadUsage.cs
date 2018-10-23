namespace Kartverket.Geonorge.Download.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDownloadUsage : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DownloadUsage",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Timestamp = c.DateTime(nullable: false),
                        Uuid = c.String(),
                        Group = c.String(),
                        Purpose = c.String(),
                        SoftwareClientVersion = c.String(),
                        SoftwareClient = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DownloadUsage");
        }
    }
}
