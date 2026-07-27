namespace BinIT2WinIT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIoTModels : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BinAlerts",
                c => new
                    {
                        AlertId = c.Int(nullable: false, identity: true),
                        BinId = c.Int(nullable: false),
                        AlertType = c.String(),
                        Message = c.String(),
                        IsResolved = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        ResolvedAt = c.DateTime(),
                        ResolvedBy = c.String(),
                    })
                .PrimaryKey(t => t.AlertId)
                .ForeignKey("dbo.SmartBins", t => t.BinId, cascadeDelete: true)
                .Index(t => t.BinId);
            
            CreateTable(
                "dbo.SmartBins",
                c => new
                    {
                        BinId = c.Int(nullable: false, identity: true),
                        BinName = c.String(nullable: false),
                        Location = c.String(nullable: false),
                        DropOffPointId = c.Int(nullable: false),
                        FillLevel = c.Int(nullable: false),
                        CurrentWeight = c.Double(nullable: false),
                        Capacity = c.Double(nullable: false),
                        Status = c.String(),
                        BatteryLevel = c.Int(nullable: false),
                        LastUpdated = c.DateTime(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        IsFullAlertSent = c.Boolean(nullable: false),
                        IsMaintenanceAlertSent = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.BinId)
                .ForeignKey("dbo.DropOffPoints", t => t.DropOffPointId)
                .Index(t => t.DropOffPointId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BinAlerts", "BinId", "dbo.SmartBins");
            DropForeignKey("dbo.SmartBins", "DropOffPointId", "dbo.DropOffPoints");
            DropIndex("dbo.SmartBins", new[] { "DropOffPointId" });
            DropIndex("dbo.BinAlerts", new[] { "BinId" });
            DropTable("dbo.SmartBins");
            DropTable("dbo.BinAlerts");
        }
    }
}
