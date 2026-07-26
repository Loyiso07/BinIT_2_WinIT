namespace BinIT2WinIT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCommunityToResident : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Residents", "Province", c => c.String());
            AddColumn("dbo.Residents", "PostalCode", c => c.String());
            AddColumn("dbo.Residents", "DropOffPointId", c => c.Int());
            CreateIndex("dbo.Residents", "ReferralCode", unique: true, name: "IX_UniqueReferralCode");
            CreateIndex("dbo.Residents", "DropOffPointId");
            AddForeignKey("dbo.Residents", "DropOffPointId", "dbo.DropOffPoints", "DropOffPointId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Residents", "DropOffPointId", "dbo.DropOffPoints");
            DropIndex("dbo.Residents", new[] { "DropOffPointId" });
            DropIndex("dbo.Residents", "IX_UniqueReferralCode");
            DropColumn("dbo.Residents", "DropOffPointId");
            DropColumn("dbo.Residents", "PostalCode");
            DropColumn("dbo.Residents", "Province");
        }
    }
}
