namespace Kartverket.Geonorge.Download.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SupportsPolygonSelectionRequiredRole : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Dataset", "supportsPolygonSelectionRequiredRole", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Dataset", "supportsPolygonSelectionRequiredRole");
        }
    }
}
