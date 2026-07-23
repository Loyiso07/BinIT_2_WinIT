namespace BinIT2WinIT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddReferralTransaction : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ReferralTransactions",
                c => new
                    {
                        ReferralId = c.Int(nullable: false, identity: true),
                        ReferrerId = c.Int(nullable: false),
                        NewResidentId = c.Int(nullable: false),
                        PromoCodeUsed = c.String(),
                        InfluencerPointsEarned = c.Int(nullable: false),
                        WelcomeBonusAwarded = c.Int(nullable: false),
                        TransactionDate = c.DateTime(nullable: false),
                        Status = c.String(),
                    })
                .PrimaryKey(t => t.ReferralId)
                .ForeignKey("dbo.Residents", t => t.NewResidentId)
                .ForeignKey("dbo.Residents", t => t.ReferrerId)
                .Index(t => t.ReferrerId)
                .Index(t => t.NewResidentId);
            
            AlterColumn("dbo.Residents", "ReferralCode", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ReferralTransactions", "ReferrerId", "dbo.Residents");
            DropForeignKey("dbo.ReferralTransactions", "NewResidentId", "dbo.Residents");
            DropIndex("dbo.ReferralTransactions", new[] { "NewResidentId" });
            DropIndex("dbo.ReferralTransactions", new[] { "ReferrerId" });
            AlterColumn("dbo.Residents", "ReferralCode", c => c.String());
            DropTable("dbo.ReferralTransactions");
        }
    }
}
