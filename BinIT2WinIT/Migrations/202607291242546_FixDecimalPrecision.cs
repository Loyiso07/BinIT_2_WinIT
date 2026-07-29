namespace BinIT2WinIT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixDecimalPrecision : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RedemptionOptions",
                c => new
                    {
                        OptionId = c.Int(nullable: false, identity: true),
                        UtilityType = c.String(nullable: false, maxLength: 20),
                        Description = c.String(nullable: false, maxLength: 100),
                        PointsRequired = c.Int(nullable: false),
                        DiscountAmount = c.Decimal(nullable: false, precision: 10, scale: 2),
                        Icon = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        ExpiryDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.OptionId);
            
            CreateTable(
                "dbo.RedemptionRequests",
                c => new
                    {
                        RequestId = c.Int(nullable: false, identity: true),
                        ResidentId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        PointsUsed = c.Int(nullable: false),
                        DiscountAmount = c.Decimal(nullable: false, precision: 10, scale: 2),
                        UtilityType = c.String(nullable: false, maxLength: 20),
                        RequestStatus = c.String(nullable: false, maxLength: 20),
                        RequestDate = c.DateTime(nullable: false),
                        ApprovedDate = c.DateTime(),
                        AppliedDate = c.DateTime(),
                        ReferenceNumber = c.String(maxLength: 50),
                        AdminNotes = c.String(),
                        ApprovedBy = c.String(),
                        UtilityAccountNumber = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.RequestId)
                .ForeignKey("dbo.RedemptionOptions", t => t.OptionId, cascadeDelete: true)
                .ForeignKey("dbo.Residents", t => t.ResidentId, cascadeDelete: true)
                .Index(t => t.ResidentId)
                .Index(t => t.OptionId);
            
            AddColumn("dbo.SystemConfigurations", "IsRedemptionEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.SystemConfigurations", "MinRedeemablePoints", c => c.Int(nullable: false));
            AddColumn("dbo.SystemConfigurations", "MaxRedeemablePoints", c => c.Int(nullable: false));
            AddColumn("dbo.SystemConfigurations", "WaterDiscountRate", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.SystemConfigurations", "ElectricityDiscountRate", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.SystemConfigurations", "ComboDiscountRate", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.SystemConfigurations", "RedemptionProcessingDays", c => c.Int(nullable: false));
            AddColumn("dbo.SystemConfigurations", "RedemptionTerms", c => c.String());
            AddColumn("dbo.SystemConfigurations", "DefaultOptionId", c => c.Int());
            AddColumn("dbo.SystemConfigurations", "AutoApproveRedemption", c => c.Boolean(nullable: false));
            AlterColumn("dbo.PointsTransactions", "Amount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RedemptionRequests", "ResidentId", "dbo.Residents");
            DropForeignKey("dbo.RedemptionRequests", "OptionId", "dbo.RedemptionOptions");
            DropIndex("dbo.RedemptionRequests", new[] { "OptionId" });
            DropIndex("dbo.RedemptionRequests", new[] { "ResidentId" });
            AlterColumn("dbo.PointsTransactions", "Amount", c => c.Int(nullable: false));
            DropColumn("dbo.SystemConfigurations", "AutoApproveRedemption");
            DropColumn("dbo.SystemConfigurations", "DefaultOptionId");
            DropColumn("dbo.SystemConfigurations", "RedemptionTerms");
            DropColumn("dbo.SystemConfigurations", "RedemptionProcessingDays");
            DropColumn("dbo.SystemConfigurations", "ComboDiscountRate");
            DropColumn("dbo.SystemConfigurations", "ElectricityDiscountRate");
            DropColumn("dbo.SystemConfigurations", "WaterDiscountRate");
            DropColumn("dbo.SystemConfigurations", "MaxRedeemablePoints");
            DropColumn("dbo.SystemConfigurations", "MinRedeemablePoints");
            DropColumn("dbo.SystemConfigurations", "IsRedemptionEnabled");
            DropTable("dbo.RedemptionRequests");
            DropTable("dbo.RedemptionOptions");
        }
    }
}
