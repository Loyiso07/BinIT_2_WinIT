namespace BinIT2WinIT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CommunityStatus : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CommunityStatus",
                c => new
                    {
                        StatusId = c.Int(nullable: false, identity: true),
                        DropOffPointId = c.Int(nullable: false),
                        Status = c.String(nullable: false),
                        Notes = c.String(),
                        UpdatedDate = c.DateTime(nullable: false),
                        UpdatedBy = c.String(),
                    })
                .PrimaryKey(t => t.StatusId)
                .ForeignKey("dbo.DropOffPoints", t => t.DropOffPointId, cascadeDelete: true)
                .Index(t => t.DropOffPointId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CommunityStatus", "DropOffPointId", "dbo.DropOffPoints");
            DropIndex("dbo.CommunityStatus", new[] { "DropOffPointId" });
            DropTable("dbo.CommunityStatus");
        }
    }
}
