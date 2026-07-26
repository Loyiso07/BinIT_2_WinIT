namespace BinIT2WinIT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAnnouncements : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Announcements",
                c => new
                    {
                        AnnouncementId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false),
                        Message = c.String(nullable: false),
                        RewardType = c.String(),
                        TargetAudience = c.String(),
                        MinPointsRequired = c.Int(),
                        VoucherCode = c.String(),
                        CommunityReward = c.String(),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                    })
                .PrimaryKey(t => t.AnnouncementId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Announcements");
        }
    }
}
