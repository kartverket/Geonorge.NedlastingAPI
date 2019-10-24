namespace Kartverket.Geonorge.Download.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AccessConstraintRequiredRole : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Dataset", "AccessConstraintRequiredRole", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Dataset", "AccessConstraintRequiredRole");
        }
    }
}
