namespace Kartverket.Geonorge.Download.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedAccessConstraint : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Dataset", "AccessConstraint", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Dataset", "AccessConstraint");
        }
    }
}
