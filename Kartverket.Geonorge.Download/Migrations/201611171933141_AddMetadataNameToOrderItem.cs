namespace Kartverket.Geonorge.Download.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMetadataNameToOrderItem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.orderItem", "MetadataName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.orderItem", "MetadataName");
        }
    }
}
