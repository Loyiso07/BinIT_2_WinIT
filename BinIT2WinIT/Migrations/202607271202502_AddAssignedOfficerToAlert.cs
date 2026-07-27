namespace BinIT2WinIT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAssignedOfficerToAlert : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BinAlerts", "AssignedOfficerId", c => c.Int());
            AddColumn("dbo.SmartBins", "TempAssignedOfficerId", c => c.Int());
            AddColumn("dbo.SmartBins", "TempAssignmentDate", c => c.DateTime());
            AddColumn("dbo.SmartBins", "TempAssignmentReason", c => c.String());
            CreateIndex("dbo.BinAlerts", "AssignedOfficerId");
            CreateIndex("dbo.SmartBins", "TempAssignedOfficerId");
            AddForeignKey("dbo.BinAlerts", "AssignedOfficerId", "dbo.CollectionOfficers", "OfficerId");
            AddForeignKey("dbo.SmartBins", "TempAssignedOfficerId", "dbo.CollectionOfficers", "OfficerId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SmartBins", "TempAssignedOfficerId", "dbo.CollectionOfficers");
            DropForeignKey("dbo.BinAlerts", "AssignedOfficerId", "dbo.CollectionOfficers");
            DropIndex("dbo.SmartBins", new[] { "TempAssignedOfficerId" });
            DropIndex("dbo.BinAlerts", new[] { "AssignedOfficerId" });
            DropColumn("dbo.SmartBins", "TempAssignmentReason");
            DropColumn("dbo.SmartBins", "TempAssignmentDate");
            DropColumn("dbo.SmartBins", "TempAssignedOfficerId");
            DropColumn("dbo.BinAlerts", "AssignedOfficerId");
        }
    }
}
