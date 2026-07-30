namespace BinIT2WinIT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
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
                        NewAdminEmail = c.String(nullable: false, maxLength: 256),
                        NewAdminName = c.String(nullable: false, maxLength: 100),
                        CreatedByName = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.AuditId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.NewAdminUserId)
                .Index(t => t.NewAdminUserId)
                .Index(t => t.CreatedByUserId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FullName = c.String(maxLength: 100),
                        PhoneNumber = c.String(),
                        Address = c.String(),
                        Suburb = c.String(),
                        City = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Administrators",
                c => new
                    {
                        AdminId = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        FullName = c.String(nullable: false, maxLength: 100),
                        Email = c.String(nullable: false, maxLength: 100),
                        Department = c.String(maxLength: 50),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedByUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.AdminId)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedByUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.CreatedByUserId);
            
            CreateTable(
                "dbo.Announcements",
                c => new
                    {
                        AnnouncementId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 200),
                        Message = c.String(nullable: false),
                        RewardType = c.String(),
                        TargetAudience = c.String(maxLength: 20),
                        MinPointsRequired = c.Int(),
                        VoucherCode = c.String(),
                        CommunityReward = c.String(),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedBy = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.AnnouncementId);
            
            CreateTable(
                "dbo.BinAlerts",
                c => new
                    {
                        AlertId = c.Int(nullable: false, identity: true),
                        BinId = c.Int(nullable: false),
                        AlertType = c.String(nullable: false, maxLength: 50),
                        Message = c.String(),
                        IsResolved = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        ResolvedAt = c.DateTime(),
                        ResolvedBy = c.String(),
                        AssignedOfficerId = c.Int(),
                    })
                .PrimaryKey(t => t.AlertId)
                .ForeignKey("dbo.CollectionOfficers", t => t.AssignedOfficerId)
                .ForeignKey("dbo.SmartBins", t => t.BinId)
                .Index(t => t.BinId)
                .Index(t => t.AssignedOfficerId);
            
            CreateTable(
                "dbo.CollectionOfficers",
                c => new
                    {
                        OfficerId = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        FullName = c.String(nullable: false, maxLength: 100),
                        PhoneNumber = c.String(maxLength: 20),
                        DropOffPointId = c.Int(),
                        EmployeeNumber = c.String(nullable: false, maxLength: 20),
                        Department = c.String(maxLength: 50),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        DropOffPoint_DropOffPointId = c.Int(),
                    })
                .PrimaryKey(t => t.OfficerId)
                .ForeignKey("dbo.DropOffPoints", t => t.DropOffPoint_DropOffPointId)
                .ForeignKey("dbo.DropOffPoints", t => t.DropOffPointId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.DropOffPointId)
                .Index(t => t.DropOffPoint_DropOffPointId);
            
            CreateTable(
                "dbo.DropOffPoints",
                c => new
                    {
                        DropOffPointId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Address = c.String(nullable: false, maxLength: 200),
                        City = c.String(maxLength: 50),
                        Suburb = c.String(),
                        ContactPerson = c.String(),
                        PhoneNumber = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.DropOffPointId);
            
            CreateTable(
                "dbo.CollectionEvents",
                c => new
                    {
                        EventId = c.Int(nullable: false, identity: true),
                        DropOffPointId = c.Int(nullable: false),
                        EventDate = c.DateTime(nullable: false),
                        StartTime = c.Time(nullable: false, precision: 7),
                        EndTime = c.Time(nullable: false, precision: 7),
                        Description = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        DropOffPoint_DropOffPointId = c.Int(),
                    })
                .PrimaryKey(t => t.EventId)
                .ForeignKey("dbo.DropOffPoints", t => t.DropOffPointId)
                .ForeignKey("dbo.DropOffPoints", t => t.DropOffPoint_DropOffPointId)
                .Index(t => t.DropOffPointId)
                .Index(t => t.DropOffPoint_DropOffPointId);
            
            CreateTable(
                "dbo.RecyclingSubmissions",
                c => new
                    {
                        SubmissionId = c.Int(nullable: false, identity: true),
                        ResidentId = c.Int(nullable: false),
                        MaterialTypeId = c.Int(nullable: false),
                        DropOffPointId = c.Int(),
                        Weight = c.Double(nullable: false),
                        SubmissionDate = c.DateTime(nullable: false),
                        Status = c.String(nullable: false, maxLength: 20),
                        VerifiedDate = c.DateTime(),
                        VerifiedBy = c.Int(),
                        OfficerNotes = c.String(),
                        MaterialType_MaterialTypeId = c.Int(),
                        DropOffPoint_DropOffPointId = c.Int(),
                        CollectionOfficer_OfficerId = c.Int(),
                    })
                .PrimaryKey(t => t.SubmissionId)
                .ForeignKey("dbo.DropOffPoints", t => t.DropOffPointId)
                .ForeignKey("dbo.MaterialTypes", t => t.MaterialType_MaterialTypeId)
                .ForeignKey("dbo.MaterialTypes", t => t.MaterialTypeId)
                .ForeignKey("dbo.Residents", t => t.ResidentId)
                .ForeignKey("dbo.DropOffPoints", t => t.DropOffPoint_DropOffPointId)
                .ForeignKey("dbo.CollectionOfficers", t => t.CollectionOfficer_OfficerId)
                .Index(t => t.ResidentId)
                .Index(t => t.MaterialTypeId)
                .Index(t => t.DropOffPointId)
                .Index(t => t.MaterialType_MaterialTypeId)
                .Index(t => t.DropOffPoint_DropOffPointId)
                .Index(t => t.CollectionOfficer_OfficerId);
            
            CreateTable(
                "dbo.MaterialTypes",
                c => new
                    {
                        MaterialTypeId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(maxLength: 200),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.MaterialTypeId);
            
            CreateTable(
                "dbo.CO2Factor",
                c => new
                    {
                        CO2FactorId = c.Int(nullable: false, identity: true),
                        MaterialTypeId = c.Int(nullable: false),
                        CO2SavedPerKg = c.Double(nullable: false),
                        EffectiveDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                        MaterialType_MaterialTypeId = c.Int(),
                    })
                .PrimaryKey(t => t.CO2FactorId)
                .ForeignKey("dbo.MaterialTypes", t => t.MaterialTypeId)
                .ForeignKey("dbo.MaterialTypes", t => t.MaterialType_MaterialTypeId)
                .Index(t => t.MaterialTypeId)
                .Index(t => t.MaterialType_MaterialTypeId);
            
            CreateTable(
                "dbo.PointsRates",
                c => new
                    {
                        PointsRateId = c.Int(nullable: false, identity: true),
                        MaterialTypeId = c.Int(nullable: false),
                        PointsPerKg = c.Double(nullable: false),
                        EffectiveDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                        MaterialType_MaterialTypeId = c.Int(),
                    })
                .PrimaryKey(t => t.PointsRateId)
                .ForeignKey("dbo.MaterialTypes", t => t.MaterialTypeId)
                .ForeignKey("dbo.MaterialTypes", t => t.MaterialType_MaterialTypeId)
                .Index(t => t.MaterialTypeId)
                .Index(t => t.MaterialType_MaterialTypeId);
            
            CreateTable(
                "dbo.Residents",
                c => new
                    {
                        ResidentId = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        FullName = c.String(nullable: false, maxLength: 100),
                        PhoneNumber = c.String(nullable: false, maxLength: 20),
                        Address = c.String(maxLength: 200),
                        Suburb = c.String(maxLength: 50),
                        City = c.String(maxLength: 50),
                        Province = c.String(),
                        PostalCode = c.String(),
                        PointsBalance = c.Int(nullable: false),
                        InfluencerPoints = c.Int(nullable: false),
                        TotalCO2Saved = c.Double(nullable: false),
                        TotalReferrals = c.Int(nullable: false),
                        ReferralCode = c.String(maxLength: 50),
                        DropOffPointId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ResidentId)
                .ForeignKey("dbo.DropOffPoints", t => t.DropOffPointId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.ReferralCode, unique: true, name: "IX_UniqueReferralCode")
                .Index(t => t.DropOffPointId);
            
            CreateTable(
                "dbo.PointsTransactions",
                c => new
                    {
                        TransactionId = c.Int(nullable: false, identity: true),
                        ResidentId = c.Int(nullable: false),
                        TransactionDate = c.DateTime(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Description = c.String(nullable: false, maxLength: 500),
                        Type = c.String(nullable: false, maxLength: 20),
                        ReferenceId = c.Int(),
                        Reason = c.String(maxLength: 200),
                    })
                .PrimaryKey(t => t.TransactionId)
                .ForeignKey("dbo.Residents", t => t.ResidentId)
                .Index(t => t.ResidentId);
            
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
            
            CreateTable(
                "dbo.SmartBins",
                c => new
                    {
                        BinId = c.Int(nullable: false, identity: true),
                        BinName = c.String(nullable: false, maxLength: 50),
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
                        TempAssignedOfficerId = c.Int(),
                        TempAssignmentDate = c.DateTime(),
                        TempAssignmentReason = c.String(),
                    })
                .PrimaryKey(t => t.BinId)
                .ForeignKey("dbo.DropOffPoints", t => t.DropOffPointId)
                .ForeignKey("dbo.CollectionOfficers", t => t.TempAssignedOfficerId)
                .Index(t => t.DropOffPointId)
                .Index(t => t.TempAssignedOfficerId);
            
            CreateTable(
                "dbo.CommunityStatus",
                c => new
                    {
                        StatusId = c.Int(nullable: false, identity: true),
                        DropOffPointId = c.Int(nullable: false),
                        Status = c.String(nullable: false, maxLength: 50),
                        Notes = c.String(maxLength: 500),
                        UpdatedDate = c.DateTime(nullable: false),
                        UpdatedBy = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.StatusId)
                .ForeignKey("dbo.DropOffPoints", t => t.DropOffPointId)
                .Index(t => t.DropOffPointId);
            
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        NotificationId = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        Title = c.String(nullable: false, maxLength: 200),
                        Message = c.String(nullable: false),
                        Type = c.String(maxLength: 50),
                        IsRead = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        ReadAt = c.DateTime(),
                        Link = c.String(maxLength: 500),
                    })
                .PrimaryKey(t => t.NotificationId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.RedemptionOptions",
                c => new
                    {
                        OptionId = c.Int(nullable: false, identity: true),
                        UtilityType = c.String(nullable: false, maxLength: 20),
                        Description = c.String(nullable: false, maxLength: 100),
                        PointsRequired = c.Int(nullable: false),
                        DiscountAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Icon = c.String(maxLength: 50),
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
                        DiscountAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
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
                .ForeignKey("dbo.Residents", t => t.ResidentId)
                .ForeignKey("dbo.RedemptionOptions", t => t.OptionId)
                .Index(t => t.ResidentId)
                .Index(t => t.OptionId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.SystemConfigurations",
                c => new
                    {
                        ConfigId = c.Int(nullable: false, identity: true),
                        ConfigKey = c.String(nullable: false, maxLength: 100),
                        ConfigValue = c.String(nullable: false, maxLength: 500),
                        Description = c.String(maxLength: 500),
                        UpdatedDate = c.DateTime(nullable: false),
                        UpdatedBy = c.String(),
                        IsRedemptionEnabled = c.Boolean(nullable: false),
                        MinRedeemablePoints = c.Int(nullable: false),
                        MaxRedeemablePoints = c.Int(nullable: false),
                        WaterDiscountRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ElectricityDiscountRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ComboDiscountRate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        RedemptionProcessingDays = c.Int(nullable: false),
                        RedemptionTerms = c.String(),
                        DefaultOptionId = c.Int(),
                        AutoApproveRedemption = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ConfigId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.RedemptionRequests", "OptionId", "dbo.RedemptionOptions");
            DropForeignKey("dbo.RedemptionRequests", "ResidentId", "dbo.Residents");
            DropForeignKey("dbo.Notifications", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.CommunityStatus", "DropOffPointId", "dbo.DropOffPoints");
            DropForeignKey("dbo.BinAlerts", "BinId", "dbo.SmartBins");
            DropForeignKey("dbo.SmartBins", "TempAssignedOfficerId", "dbo.CollectionOfficers");
            DropForeignKey("dbo.SmartBins", "DropOffPointId", "dbo.DropOffPoints");
            DropForeignKey("dbo.BinAlerts", "AssignedOfficerId", "dbo.CollectionOfficers");
            DropForeignKey("dbo.RecyclingSubmissions", "CollectionOfficer_OfficerId", "dbo.CollectionOfficers");
            DropForeignKey("dbo.CollectionOfficers", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.CollectionOfficers", "DropOffPointId", "dbo.DropOffPoints");
            DropForeignKey("dbo.RecyclingSubmissions", "DropOffPoint_DropOffPointId", "dbo.DropOffPoints");
            DropForeignKey("dbo.RecyclingSubmissions", "ResidentId", "dbo.Residents");
            DropForeignKey("dbo.Residents", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ReferralTransactions", "ReferrerId", "dbo.Residents");
            DropForeignKey("dbo.ReferralTransactions", "NewResidentId", "dbo.Residents");
            DropForeignKey("dbo.PointsTransactions", "ResidentId", "dbo.Residents");
            DropForeignKey("dbo.Residents", "DropOffPointId", "dbo.DropOffPoints");
            DropForeignKey("dbo.RecyclingSubmissions", "MaterialTypeId", "dbo.MaterialTypes");
            DropForeignKey("dbo.RecyclingSubmissions", "MaterialType_MaterialTypeId", "dbo.MaterialTypes");
            DropForeignKey("dbo.PointsRates", "MaterialType_MaterialTypeId", "dbo.MaterialTypes");
            DropForeignKey("dbo.PointsRates", "MaterialTypeId", "dbo.MaterialTypes");
            DropForeignKey("dbo.CO2Factor", "MaterialType_MaterialTypeId", "dbo.MaterialTypes");
            DropForeignKey("dbo.CO2Factor", "MaterialTypeId", "dbo.MaterialTypes");
            DropForeignKey("dbo.RecyclingSubmissions", "DropOffPointId", "dbo.DropOffPoints");
            DropForeignKey("dbo.CollectionOfficers", "DropOffPoint_DropOffPointId", "dbo.DropOffPoints");
            DropForeignKey("dbo.CollectionEvents", "DropOffPoint_DropOffPointId", "dbo.DropOffPoints");
            DropForeignKey("dbo.CollectionEvents", "DropOffPointId", "dbo.DropOffPoints");
            DropForeignKey("dbo.Administrators", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Administrators", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AdminCreationAudits", "NewAdminUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AdminCreationAudits", "CreatedByUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.RedemptionRequests", new[] { "OptionId" });
            DropIndex("dbo.RedemptionRequests", new[] { "ResidentId" });
            DropIndex("dbo.Notifications", new[] { "UserId" });
            DropIndex("dbo.CommunityStatus", new[] { "DropOffPointId" });
            DropIndex("dbo.SmartBins", new[] { "TempAssignedOfficerId" });
            DropIndex("dbo.SmartBins", new[] { "DropOffPointId" });
            DropIndex("dbo.ReferralTransactions", new[] { "NewResidentId" });
            DropIndex("dbo.ReferralTransactions", new[] { "ReferrerId" });
            DropIndex("dbo.PointsTransactions", new[] { "ResidentId" });
            DropIndex("dbo.Residents", new[] { "DropOffPointId" });
            DropIndex("dbo.Residents", "IX_UniqueReferralCode");
            DropIndex("dbo.Residents", new[] { "UserId" });
            DropIndex("dbo.PointsRates", new[] { "MaterialType_MaterialTypeId" });
            DropIndex("dbo.PointsRates", new[] { "MaterialTypeId" });
            DropIndex("dbo.CO2Factor", new[] { "MaterialType_MaterialTypeId" });
            DropIndex("dbo.CO2Factor", new[] { "MaterialTypeId" });
            DropIndex("dbo.RecyclingSubmissions", new[] { "CollectionOfficer_OfficerId" });
            DropIndex("dbo.RecyclingSubmissions", new[] { "DropOffPoint_DropOffPointId" });
            DropIndex("dbo.RecyclingSubmissions", new[] { "MaterialType_MaterialTypeId" });
            DropIndex("dbo.RecyclingSubmissions", new[] { "DropOffPointId" });
            DropIndex("dbo.RecyclingSubmissions", new[] { "MaterialTypeId" });
            DropIndex("dbo.RecyclingSubmissions", new[] { "ResidentId" });
            DropIndex("dbo.CollectionEvents", new[] { "DropOffPoint_DropOffPointId" });
            DropIndex("dbo.CollectionEvents", new[] { "DropOffPointId" });
            DropIndex("dbo.CollectionOfficers", new[] { "DropOffPoint_DropOffPointId" });
            DropIndex("dbo.CollectionOfficers", new[] { "DropOffPointId" });
            DropIndex("dbo.CollectionOfficers", new[] { "UserId" });
            DropIndex("dbo.BinAlerts", new[] { "AssignedOfficerId" });
            DropIndex("dbo.BinAlerts", new[] { "BinId" });
            DropIndex("dbo.Administrators", new[] { "CreatedByUserId" });
            DropIndex("dbo.Administrators", new[] { "UserId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AdminCreationAudits", new[] { "CreatedByUserId" });
            DropIndex("dbo.AdminCreationAudits", new[] { "NewAdminUserId" });
            DropTable("dbo.SystemConfigurations");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.RedemptionRequests");
            DropTable("dbo.RedemptionOptions");
            DropTable("dbo.Notifications");
            DropTable("dbo.CommunityStatus");
            DropTable("dbo.SmartBins");
            DropTable("dbo.ReferralTransactions");
            DropTable("dbo.PointsTransactions");
            DropTable("dbo.Residents");
            DropTable("dbo.PointsRates");
            DropTable("dbo.CO2Factor");
            DropTable("dbo.MaterialTypes");
            DropTable("dbo.RecyclingSubmissions");
            DropTable("dbo.CollectionEvents");
            DropTable("dbo.DropOffPoints");
            DropTable("dbo.CollectionOfficers");
            DropTable("dbo.BinAlerts");
            DropTable("dbo.Announcements");
            DropTable("dbo.Administrators");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.AdminCreationAudits");
        }
    }
}
