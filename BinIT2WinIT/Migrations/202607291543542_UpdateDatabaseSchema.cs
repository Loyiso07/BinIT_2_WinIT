namespace BinIT2WinIT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class UpdateDatabaseSchema : DbMigration
    {
        public override void Up()
        {
            // ============================================================
            // DROP EXISTING FOREIGN KEYS AND INDEXES
            // ============================================================
            DropForeignKey("dbo.AdminCreationAudits", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AdminCreationAudits", "NewAdminUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.RecyclingSubmissions", "DropOffPoint_DropOffPointId", "dbo.DropOffPoints");
            DropForeignKey("dbo.RecyclingSubmissions", "DropOffPointId", "dbo.DropOffPoints");
            DropForeignKey("dbo.RecyclingSubmissions", "MaterialTypeId", "dbo.MaterialTypes");
            DropForeignKey("dbo.RecyclingSubmissions", "ResidentId", "dbo.Residents");
            DropForeignKey("dbo.RecyclingSubmissions", "MaterialType_MaterialTypeId", "dbo.MaterialTypes");
            DropForeignKey("dbo.ReferralTransactions", "NewResidentId", "dbo.Residents");
            DropForeignKey("dbo.ReferralTransactions", "ReferrerId", "dbo.Residents");
            DropForeignKey("dbo.SmartBins", "DropOffPointId", "dbo.DropOffPoints");

            DropIndex("dbo.RecyclingSubmissions", new[] { "MaterialTypeId" });
            DropIndex("dbo.RecyclingSubmissions", new[] { "DropOffPointId" });
            DropIndex("dbo.RecyclingSubmissions", new[] { "MaterialType_MaterialTypeId" });
            DropIndex("dbo.RecyclingSubmissions", new[] { "DropOffPoint_DropOffPointId" });
            DropIndex("dbo.ReferralTransactions", new[] { "ReferrerId" });
            DropIndex("dbo.ReferralTransactions", new[] { "Resident_ResidentId" });

            // ============================================================
            // DROP OLD COLUMNS
            // ============================================================
            DropColumn("dbo.RecyclingSubmissions", "DropOffPointId");
            DropColumn("dbo.RecyclingSubmissions", "MaterialTypeId");
            DropColumn("dbo.ReferralTransactions", "ReferrerId");

            // ============================================================
            // RENAME COLUMNS
            // ============================================================
            RenameColumn(table: "dbo.RecyclingSubmissions", name: "DropOffPoint_DropOffPointId", newName: "DropOffPointId");
            RenameColumn(table: "dbo.RecyclingSubmissions", name: "MaterialType_MaterialTypeId", newName: "MaterialTypeId");
            RenameColumn(table: "dbo.ReferralTransactions", name: "Resident_ResidentId", newName: "ReferrerId");
            RenameColumn(table: "dbo.RedemptionRequests", name: "RedemptionOptionId", newName: "OptionId");
            RenameIndex(table: "dbo.RedemptionRequests", name: "IX_RedemptionOptionId", newName: "IX_OptionId");

            // ============================================================
            // MAKE COLUMNS NULLABLE FIRST (to handle existing data)
            // ============================================================
            AlterColumn("dbo.RecyclingSubmissions", "MaterialTypeId", c => c.Int());
            AlterColumn("dbo.RecyclingSubmissions", "DropOffPointId", c => c.Int());
            AlterColumn("dbo.ReferralTransactions", "ReferrerId", c => c.Int());

            // ============================================================
            // ✅ SET DEFAULT VALUES FOR NULL RECORDS
            // ============================================================

            // 1. Ensure a default MaterialType exists
            Sql(@"
                IF NOT EXISTS (SELECT 1 FROM dbo.MaterialTypes WHERE MaterialTypeId = 1)
                BEGIN
                    SET IDENTITY_INSERT dbo.MaterialTypes ON;
                    INSERT INTO dbo.MaterialTypes (MaterialTypeId, Name, Description, IsActive) 
                    VALUES (1, 'Plastic', 'Plastic bottles and containers', 1);
                    SET IDENTITY_INSERT dbo.MaterialTypes OFF;
                END
            ");

            // 2. Update NULL MaterialTypeId to 1
            Sql("UPDATE dbo.RecyclingSubmissions SET MaterialTypeId = 1 WHERE MaterialTypeId IS NULL");

            // 3. Update NULL DropOffPointId to NULL (if there's no default, keep as NULL)
            // Or set to an existing DropOffPointId if you have one
            // If you don't have any DropOffPoints, leave as NULL temporarily
            Sql("UPDATE dbo.RecyclingSubmissions SET DropOffPointId = NULL WHERE DropOffPointId IS NULL");

            // 4. Update NULL ReferrerId to a default value (if applicable)
            // If you don't have valid referrers, set to NULL or a default value
            Sql("UPDATE dbo.ReferralTransactions SET ReferrerId = 1 WHERE ReferrerId IS NULL");

            // ============================================================
            // NOW MAKE COLUMNS NON-NULLABLE (if needed)
            // ============================================================
            // Only make MaterialTypeId non-nullable if you're sure
            AlterColumn("dbo.RecyclingSubmissions", "MaterialTypeId", c => c.Int(nullable: false));

            // If DropOffPointId can be optional, keep it nullable
            // AlterColumn("dbo.RecyclingSubmissions", "DropOffPointId", c => c.Int());

            // If ReferrerId can be optional, keep it nullable
            // AlterColumn("dbo.ReferralTransactions", "ReferrerId", c => c.Int());

            // ============================================================
            // ADD REDEMPTION REQUESTS COLUMNS (OLD STRUCTURE)
            // ============================================================
            AddColumn("dbo.RedemptionRequests", "DiscountAmount", c => c.Decimal(nullable: false, precision: 10, scale: 2));
            AddColumn("dbo.RedemptionRequests", "UtilityType", c => c.String(nullable: false, maxLength: 20));
            AddColumn("dbo.RedemptionRequests", "RequestStatus", c => c.String(nullable: false, maxLength: 20));
            AddColumn("dbo.RedemptionRequests", "ApprovedDate", c => c.DateTime());
            AddColumn("dbo.RedemptionRequests", "AppliedDate", c => c.DateTime());
            AddColumn("dbo.RedemptionRequests", "ReferenceNumber", c => c.String(maxLength: 50));
            AddColumn("dbo.RedemptionRequests", "AdminNotes", c => c.String());
            AddColumn("dbo.RedemptionRequests", "ApprovedBy", c => c.String());
            AddColumn("dbo.RedemptionRequests", "UtilityAccountNumber", c => c.String(maxLength: 50));

            // ============================================================
            // ALTER REDEMPTION OPTIONS PRECISION
            // ============================================================
            AlterColumn("dbo.RedemptionOptions", "DiscountAmount", c => c.Decimal(nullable: false, precision: 10, scale: 2));

            // ============================================================
            // CREATE INDEXES
            // ============================================================
            CreateIndex("dbo.RecyclingSubmissions", "MaterialTypeId");
            CreateIndex("dbo.RecyclingSubmissions", "DropOffPointId");
            CreateIndex("dbo.ReferralTransactions", "ReferrerId");

            // ============================================================
            // ADD FOREIGN KEYS (WITHOUT CASCADE DELETE)
            // ============================================================
            AddForeignKey("dbo.AdminCreationAudits", "CreatedByUserId", "dbo.AspNetUsers", "Id", cascadeDelete: false);
            AddForeignKey("dbo.AdminCreationAudits", "NewAdminUserId", "dbo.AspNetUsers", "Id", cascadeDelete: false);
            AddForeignKey("dbo.RecyclingSubmissions", "DropOffPointId", "dbo.DropOffPoints", "DropOffPointId", cascadeDelete: false);
            AddForeignKey("dbo.RecyclingSubmissions", "MaterialTypeId", "dbo.MaterialTypes", "MaterialTypeId", cascadeDelete: false);
            AddForeignKey("dbo.RecyclingSubmissions", "ResidentId", "dbo.Residents", "ResidentId", cascadeDelete: false);
            AddForeignKey("dbo.ReferralTransactions", "NewResidentId", "dbo.Residents", "ResidentId", cascadeDelete: false);
            AddForeignKey("dbo.ReferralTransactions", "ReferrerId", "dbo.Residents", "ResidentId", cascadeDelete: false);
            AddForeignKey("dbo.SmartBins", "DropOffPointId", "dbo.DropOffPoints", "DropOffPointId", cascadeDelete: false);

            // ============================================================
            // DROP NEW COLUMNS THAT WERE ADDED IN ERROR
            // ============================================================
            DropColumn("dbo.RedemptionRequests", "AccountNumber");
            DropColumn("dbo.RedemptionRequests", "Status");
            DropColumn("dbo.RedemptionRequests", "ProcessedDate");
            DropColumn("dbo.RedemptionRequests", "ProcessedBy");
            DropColumn("dbo.RedemptionRequests", "Notes");
        }

        public override void Down()
        {
            // ============================================================
            // ADD BACK NEW COLUMNS
            // ============================================================
            AddColumn("dbo.RedemptionRequests", "Notes", c => c.String(maxLength: 500));
            AddColumn("dbo.RedemptionRequests", "ProcessedBy", c => c.String(maxLength: 128));
            AddColumn("dbo.RedemptionRequests", "ProcessedDate", c => c.DateTime());
            AddColumn("dbo.RedemptionRequests", "Status", c => c.String(nullable: false, maxLength: 20));
            AddColumn("dbo.RedemptionRequests", "AccountNumber", c => c.String(nullable: false, maxLength: 50));

            // ============================================================
            // DROP FOREIGN KEYS
            // ============================================================
            DropForeignKey("dbo.SmartBins", "DropOffPointId", "dbo.DropOffPoints");
            DropForeignKey("dbo.ReferralTransactions", "ReferrerId", "dbo.Residents");
            DropForeignKey("dbo.ReferralTransactions", "NewResidentId", "dbo.Residents");
            DropForeignKey("dbo.RecyclingSubmissions", "ResidentId", "dbo.Residents");
            DropForeignKey("dbo.RecyclingSubmissions", "MaterialTypeId", "dbo.MaterialTypes");
            DropForeignKey("dbo.RecyclingSubmissions", "DropOffPointId", "dbo.DropOffPoints");
            DropForeignKey("dbo.AdminCreationAudits", "NewAdminUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AdminCreationAudits", "CreatedByUserId", "dbo.AspNetUsers");

            // ============================================================
            // DROP INDEXES
            // ============================================================
            DropIndex("dbo.ReferralTransactions", new[] { "ReferrerId" });
            DropIndex("dbo.RecyclingSubmissions", new[] { "DropOffPointId" });
            DropIndex("dbo.RecyclingSubmissions", new[] { "MaterialTypeId" });

            // ============================================================
            // REVERT COLUMNS
            // ============================================================
            AlterColumn("dbo.RedemptionOptions", "DiscountAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.ReferralTransactions", "ReferrerId", c => c.Int());
            AlterColumn("dbo.RecyclingSubmissions", "DropOffPointId", c => c.Int());
            AlterColumn("dbo.RecyclingSubmissions", "MaterialTypeId", c => c.Int());

            // ============================================================
            // DROP REDEMPTION COLUMNS
            // ============================================================
            DropColumn("dbo.RedemptionRequests", "UtilityAccountNumber");
            DropColumn("dbo.RedemptionRequests", "ApprovedBy");
            DropColumn("dbo.RedemptionRequests", "AdminNotes");
            DropColumn("dbo.RedemptionRequests", "ReferenceNumber");
            DropColumn("dbo.RedemptionRequests", "AppliedDate");
            DropColumn("dbo.RedemptionRequests", "ApprovedDate");
            DropColumn("dbo.RedemptionRequests", "RequestStatus");
            DropColumn("dbo.RedemptionRequests", "UtilityType");
            DropColumn("dbo.RedemptionRequests", "DiscountAmount");

            // ============================================================
            // RENAME BACK
            // ============================================================
            RenameIndex(table: "dbo.RedemptionRequests", name: "IX_OptionId", newName: "IX_RedemptionOptionId");
            RenameColumn(table: "dbo.RedemptionRequests", name: "OptionId", newName: "RedemptionOptionId");
            RenameColumn(table: "dbo.ReferralTransactions", name: "ReferrerId", newName: "Resident_ResidentId");
            RenameColumn(table: "dbo.RecyclingSubmissions", name: "MaterialTypeId", newName: "MaterialType_MaterialTypeId");
            RenameColumn(table: "dbo.RecyclingSubmissions", name: "DropOffPointId", newName: "DropOffPoint_DropOffPointId");

            // ============================================================
            // ADD BACK OLD COLUMNS
            // ============================================================
            AddColumn("dbo.ReferralTransactions", "ReferrerId", c => c.Int(nullable: false));
            AddColumn("dbo.RecyclingSubmissions", "MaterialTypeId", c => c.Int(nullable: false));
            AddColumn("dbo.RecyclingSubmissions", "DropOffPointId", c => c.Int(nullable: false));

            // ============================================================
            // CREATE INDEXES
            // ============================================================
            CreateIndex("dbo.ReferralTransactions", "Resident_ResidentId");
            CreateIndex("dbo.ReferralTransactions", "ReferrerId");
            CreateIndex("dbo.RecyclingSubmissions", "DropOffPoint_DropOffPointId");
            CreateIndex("dbo.RecyclingSubmissions", "MaterialType_MaterialTypeId");
            CreateIndex("dbo.RecyclingSubmissions", "DropOffPointId");
            CreateIndex("dbo.RecyclingSubmissions", "MaterialTypeId");

            // ============================================================
            // ADD FOREIGN KEYS
            // ============================================================
            AddForeignKey("dbo.SmartBins", "DropOffPointId", "dbo.DropOffPoints", "DropOffPointId", cascadeDelete: true);
            AddForeignKey("dbo.ReferralTransactions", "ReferrerId", "dbo.Residents", "ResidentId", cascadeDelete: true);
            AddForeignKey("dbo.ReferralTransactions", "NewResidentId", "dbo.Residents", "ResidentId", cascadeDelete: true);
            AddForeignKey("dbo.RecyclingSubmissions", "MaterialType_MaterialTypeId", "dbo.MaterialTypes", "MaterialTypeId");
            AddForeignKey("dbo.RecyclingSubmissions", "ResidentId", "dbo.Residents", "ResidentId");
            AddForeignKey("dbo.RecyclingSubmissions", "MaterialTypeId", "dbo.MaterialTypes", "MaterialTypeId");
            AddForeignKey("dbo.RecyclingSubmissions", "DropOffPointId", "dbo.DropOffPoints", "DropOffPointId");
            AddForeignKey("dbo.RecyclingSubmissions", "DropOffPoint_DropOffPointId", "dbo.DropOffPoints", "DropOffPointId");
            AddForeignKey("dbo.AdminCreationAudits", "NewAdminUserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.AdminCreationAudits", "CreatedByUserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
    }
}