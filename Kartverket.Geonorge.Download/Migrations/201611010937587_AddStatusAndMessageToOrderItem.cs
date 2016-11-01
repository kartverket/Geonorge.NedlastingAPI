namespace Kartverket.Geonorge.Download.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStatusAndMessageToOrderItem : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.orderItem", new[] { "referenceNumber" });
            AddColumn("dbo.orderItem", "Status", c => c.Int(nullable: false));
            AddColumn("dbo.orderItem", "Message", c => c.String());
            CreateIndex("dbo.orderItem", "ReferenceNumber");
        }
        
        public override void Down()
        {
            DropIndex("dbo.orderItem", new[] { "ReferenceNumber" });
            DropColumn("dbo.orderItem", "Message");
            DropColumn("dbo.orderItem", "Status");
            CreateIndex("dbo.orderItem", "referenceNumber");
        }
    }
}
