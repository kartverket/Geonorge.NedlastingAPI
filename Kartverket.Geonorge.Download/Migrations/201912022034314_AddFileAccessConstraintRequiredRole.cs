namespace Kartverket.Geonorge.Download.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFileAccessConstraintRequiredRole : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.filliste", "AccessConstraintRequiredRole", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.filliste", "AccessConstraintRequiredRole");
        }
    }
}
