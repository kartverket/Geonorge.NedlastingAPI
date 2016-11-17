namespace Kartverket.Geonorge.Download.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMaxAreaToDataset : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Dataset", "maxArea", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Dataset", "maxArea");
        }
    }
}
