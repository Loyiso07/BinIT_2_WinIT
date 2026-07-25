namespace BinIT2WinIT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAdminAudit : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AdminCreationAudits",
                c => new
                    {
                        AuditId = c.Int(nullable: false, identity: true),
                        NewAdminUserId = c.String(nullable: false, maxLength: 128),
                        CreatedByUserId = c.String(nullable: false, maxLength: 128),
                        CreatedAt = c.DateTime(nullable: false),
                        NewAdminEmail = c.String(),
                        NewAdminName = c.String(),
                        CreatedByName = c.String(),
                    })
                .PrimaryKey(t => t.AuditId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.NewAdminUserId)
                .Index(t => t.NewAdminUserId)
                .Index(t => t.CreatedByUserId);
            
            AddColumn("dbo.Administrators", "CreatedByUserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Administrators", "CreatedByUserId");
            AddForeignKey("dbo.Administrators", "CreatedByUserId", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Administrators", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AdminCreationAudits", "NewAdminUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AdminCreationAudits", "CreatedByUserId", "dbo.AspNetUsers");
            DropIndex("dbo.Administrators", new[] { "CreatedByUserId" });
            DropIndex("dbo.AdminCreationAudits", new[] { "CreatedByUserId" });
            DropIndex("dbo.AdminCreationAudits", new[] { "NewAdminUserId" });
            DropColumn("dbo.Administrators", "CreatedByUserId");
            DropTable("dbo.AdminCreationAudits");
        }
    }
}
