namespace Kartverket.Geonorge.Download.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RolesForMachineAccount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MachineAccounts", "Roles", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.MachineAccounts", "Roles");
        }
    }
}
