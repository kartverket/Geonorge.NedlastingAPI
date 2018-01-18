namespace Kartverket.Geonorge.Download.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMachineAccount : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MachineAccounts",
                c => new
                    {
                        Username = c.String(nullable: false, maxLength: 128),
                        Passsword = c.String(),
                        Company = c.String(),
                        ContactPerson = c.String(),
                        ContactEmail = c.String(),
                        Created = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Username);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.MachineAccounts");
        }
    }
}
