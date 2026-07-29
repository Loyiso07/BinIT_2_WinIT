namespace BinIT2WinIT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class FixDropOffPointRelationship : DbMigration
    {
        public override void Up()
        {
            // Drop existing foreign keys
            DropForeignKey("dbo.AdminCreationAudits", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AdminCreationAudits", "NewAdminUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.RecyclingSubmissions", "DropOffPointId", "dbo.DropOffPoints");
            DropForeignKey("dbo.RecyclingSubmissions", "MaterialTypeId", "dbo.MaterialTypes");
            DropForeignKey("dbo.RecyclingSubmissions", "ResidentId", "dbo.Residents");
            DropForeignKey("dbo.ReferralTransactions", "ReferrerId", "dbo.Residents");
            DropForeignKey("dbo.ReferralTransactions", "NewResidentId", "dbo.Residents");
            DropForeignKey("dbo.SmartBins", "DropOffPointId", "dbo.DropOffPoints");

            // Rename columns
            RenameColumn(table: "dbo.RedemptionRequests", name: "OptionId", newName: "RedemptionOptionId");
            RenameIndex(table: "dbo.RedemptionRequests", name: "IX_OptionId", newName: "IX_RedemptionOptionId");

            // Add new columns
            AddColumn("dbo.RecyclingSubmissions", "MaterialType_MaterialTypeId", c => c.Int());
            AddColumn("dbo.RecyclingSubmissions", "DropOffPoint_DropOffPointId", c => c.Int());
            AddColumn("dbo.ReferralTransactions", "Resident_ResidentId", c => c.Int());
            AddColumn("dbo.RedemptionRequests", "AccountNumber", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.RedemptionRequests", "Status", c => c.String(nullable: false, maxLength: 20));
            AddColumn("dbo.RedemptionRequests", "ProcessedDate", c => c.DateTime());
            AddColumn("dbo.RedemptionRequests", "ProcessedBy", c => c.String(maxLength: 128));
            AddColumn("dbo.RedemptionRequests", "Notes", c => c.String(maxLength: 500));

            // Alter column precision
            AlterColumn("dbo.RedemptionOptions", "DiscountAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));

            // Create indexes
            CreateIndex("dbo.RecyclingSubmissions", "MaterialType_MaterialTypeId");
            CreateIndex("dbo.RecyclingSubmissions", "DropOffPoint_DropOffPointId");
            CreateIndex("dbo.ReferralTransactions", "Resident_ResidentId");

            // ✅ FIXED: All foreign keys with cascadeDelete: false to prevent multiple cascade paths
            AddForeignKey("dbo.AdminCreationAudits", "CreatedByUserId", "dbo.AspNetUsers", "Id", cascadeDelete: false);
            AddForeignKey("dbo.AdminCreationAudits", "NewAdminUserId", "dbo.AspNetUsers", "Id", cascadeDelete: false);

            // ✅ FIXED: RecyclingSubmissions foreign keys with cascadeDelete: false
            AddForeignKey("dbo.RecyclingSubmissions", "DropOffPoint_DropOffPointId", "dbo.DropOffPoints", "DropOffPointId", cascadeDelete: false);
            AddForeignKey("dbo.RecyclingSubmissions", "DropOffPointId", "dbo.DropOffPoints", "DropOffPointId", cascadeDelete: false);
            AddForeignKey("dbo.RecyclingSubmissions", "MaterialTypeId", "dbo.MaterialTypes", "MaterialTypeId", cascadeDelete: false);
            AddForeignKey("dbo.RecyclingSubmissions", "ResidentId", "dbo.Residents", "ResidentId", cascadeDelete: false);
            AddForeignKey("dbo.RecyclingSubmissions", "MaterialType_MaterialTypeId", "dbo.MaterialTypes", "MaterialTypeId", cascadeDelete: false);

            // ✅ FIXED: ReferralTransactions foreign keys with cascadeDelete: false
            AddForeignKey("dbo.ReferralTransactions", "Resident_ResidentId", "dbo.Residents", "ResidentId", cascadeDelete: false);
            AddForeignKey("dbo.ReferralTransactions", "NewResidentId", "dbo.Residents", "ResidentId", cascadeDelete: false);
            AddForeignKey("dbo.ReferralTransactions", "ReferrerId", "dbo.Residents", "ResidentId", cascadeDelete: false);

            // ✅ FIXED: SmartBins foreign key with cascadeDelete: false
            AddForeignKey("dbo.SmartBins", "DropOffPointId", "dbo.DropOffPoints", "DropOffPointId", cascadeDelete: false);

            // Drop old columns
            DropColumn("dbo.RedemptionRequests", "DiscountAmount");
            DropColumn("dbo.RedemptionRequests", "UtilityType");
            DropColumn("dbo.RedemptionRequests", "RequestStatus");
            DropColumn("dbo.RedemptionRequests", "ApprovedDate");
            DropColumn("dbo.RedemptionRequests", "AppliedDate");
            DropColumn("dbo.RedemptionRequests", "ReferenceNumber");
            DropColumn("dbo.RedemptionRequests", "AdminNotes");
            DropColumn("dbo.RedemptionRequests", "ApprovedBy");
            DropColumn("dbo.RedemptionRequests", "UtilityAccountNumber");
        }

        public override void Down()
        {
            // Add back old columns
            AddColumn("dbo.RedemptionRequests", "UtilityAccountNumber", c => c.String(maxLength: 50));
            AddColumn("dbo.RedemptionRequests", "ApprovedBy", c => c.String());
            AddColumn("dbo.RedemptionRequests", "AdminNotes", c => c.String());
            AddColumn("dbo.RedemptionRequests", "ReferenceNumber", c => c.String(maxLength: 50));
            AddColumn("dbo.RedemptionRequests", "AppliedDate", c => c.DateTime());
            AddColumn("dbo.RedemptionRequests", "ApprovedDate", c => c.DateTime());
            AddColumn("dbo.RedemptionRequests", "RequestStatus", c => c.String(nullable: false, maxLength: 20));
            AddColumn("dbo.RedemptionRequests", "UtilityType", c => c.String(nullable: false, maxLength: 20));
            AddColumn("dbo.RedemptionRequests", "DiscountAmount", c => c.Decimal(nullable: false, precision: 10, scale: 2));

            // Drop foreign keys (with correct names)
            DropForeignKey("dbo.SmartBins", "DropOffPointId", "dbo.DropOffPoints");
            DropForeignKey("dbo.ReferralTransactions", "ReferrerId", "dbo.Residents");
            DropForeignKey("dbo.ReferralTransactions", "NewResidentId", "dbo.Residents");
            DropForeignKey("dbo.ReferralTransactions", "Resident_ResidentId", "dbo.Residents");
            DropForeignKey("dbo.RecyclingSubmissions", "MaterialType_MaterialTypeId", "dbo.MaterialTypes");
            DropForeignKey("dbo.RecyclingSubmissions", "ResidentId", "dbo.Residents");
            DropForeignKey("dbo.RecyclingSubmissions", "MaterialTypeId", "dbo.MaterialTypes");
            DropForeignKey("dbo.RecyclingSubmissions", "DropOffPointId", "dbo.DropOffPoints");
            DropForeignKey("dbo.RecyclingSubmissions", "DropOffPoint_DropOffPointId", "dbo.DropOffPoints");
            DropForeignKey("dbo.AdminCreationAudits", "NewAdminUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AdminCreationAudits", "CreatedByUserId", "dbo.AspNetUsers");

            // Drop indexes
            DropIndex("dbo.ReferralTransactions", new[] { "Resident_ResidentId" });
            DropIndex("dbo.RecyclingSubmissions", new[] { "DropOffPoint_DropOffPointId" });
            DropIndex("dbo.RecyclingSubmissions", new[] { "MaterialType_MaterialTypeId" });

            // Revert column precision
            AlterColumn("dbo.RedemptionOptions", "DiscountAmount", c => c.Decimal(nullable: false, precision: 10, scale: 2));

            // Drop new columns
            DropColumn("dbo.RedemptionRequests", "Notes");
            DropColumn("dbo.RedemptionRequests", "ProcessedBy");
            DropColumn("dbo.RedemptionRequests", "ProcessedDate");
            DropColumn("dbo.RedemptionRequests", "Status");
            DropColumn("dbo.RedemptionRequests", "AccountNumber");
            DropColumn("dbo.ReferralTransactions", "Resident_ResidentId");
            DropColumn("dbo.RecyclingSubmissions", "DropOffPoint_DropOffPointId");
            DropColumn("dbo.RecyclingSubmissions", "MaterialType_MaterialTypeId");

            // Rename columns back
            RenameIndex(table: "dbo.RedemptionRequests", name: "IX_RedemptionOptionId", newName: "IX_OptionId");
            RenameColumn(table: "dbo.RedemptionRequests", name: "RedemptionOptionId", newName: "OptionId");

            // Re-add old foreign keys (with cascadeDelete: false to avoid issues)
            AddForeignKey("dbo.SmartBins", "DropOffPointId", "dbo.DropOffPoints", "DropOffPointId", cascadeDelete: false);
            AddForeignKey("dbo.ReferralTransactions", "NewResidentId", "dbo.Residents", "ResidentId", cascadeDelete: false);
            AddForeignKey("dbo.ReferralTransactions", "ReferrerId", "dbo.Residents", "ResidentId", cascadeDelete: false);
            AddForeignKey("dbo.RecyclingSubmissions", "ResidentId", "dbo.Residents", "ResidentId", cascadeDelete: false);
            AddForeignKey("dbo.RecyclingSubmissions", "MaterialTypeId", "dbo.MaterialTypes", "MaterialTypeId", cascadeDelete: false);
            AddForeignKey("dbo.RecyclingSubmissions", "DropOffPointId", "dbo.DropOffPoints", "DropOffPointId", cascadeDelete: false);
            AddForeignKey("dbo.AdminCreationAudits", "NewAdminUserId", "dbo.AspNetUsers", "Id", cascadeDelete: false);
            AddForeignKey("dbo.AdminCreationAudits", "CreatedByUserId", "dbo.AspNetUsers", "Id", cascadeDelete: false);
        }
    }
}