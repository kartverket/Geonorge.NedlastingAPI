namespace Kartverket.Geonorge.Download.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAreaAndProjectionNameToOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.orderItem", "AreaName", c => c.String());
            AddColumn("dbo.orderItem", "ProjectionName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.orderItem", "ProjectionName");
            DropColumn("dbo.orderItem", "AreaName");
        }
    }
}
