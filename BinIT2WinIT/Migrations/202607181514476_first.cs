namespace BinIT2WinIT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class first : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Administrators",
                c => new
                    {
                        AdminId = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        FullName = c.String(nullable: false),
                        Email = c.String(nullable: false),
                        Department = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.AdminId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FullName = c.String(),
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
                "dbo.CO2Factor",
                c => new
                    {
                        CO2FactorId = c.Int(nullable: false, identity: true),
                        MaterialTypeId = c.Int(nullable: false),
                        CO2SavedPerKg = c.Double(nullable: false),
                        EffectiveDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.CO2FactorId)
                .ForeignKey("dbo.MaterialTypes", t => t.MaterialTypeId, cascadeDelete: true)
                .Index(t => t.MaterialTypeId);
            
            CreateTable(
                "dbo.MaterialTypes",
                c => new
                    {
                        MaterialTypeId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Description = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.MaterialTypeId);
            
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
                    })
                .PrimaryKey(t => t.PointsRateId)
                .ForeignKey("dbo.MaterialTypes", t => t.MaterialTypeId, cascadeDelete: true)
                .Index(t => t.MaterialTypeId);
            
            CreateTable(
                "dbo.RecyclingSubmissions",
                c => new
                    {
                        SubmissionId = c.Int(nullable: false, identity: true),
                        ResidentId = c.Int(nullable: false),
                        MaterialTypeId = c.Int(nullable: false),
                        DropOffPointId = c.Int(nullable: false),
                        Weight = c.Double(nullable: false),
                        SubmissionDate = c.DateTime(nullable: false),
                        Status = c.String(nullable: false),
                        OfficerNotes = c.String(),
                        VerifiedBy = c.Int(),
                        VerifiedDate = c.DateTime(),
                        IsFlagged = c.Boolean(nullable: false),
                        FlagReason = c.String(),
                    })
                .PrimaryKey(t => t.SubmissionId)
                .ForeignKey("dbo.CollectionOfficers", t => t.VerifiedBy)
                .ForeignKey("dbo.DropOffPoints", t => t.DropOffPointId, cascadeDelete: true)
                .ForeignKey("dbo.MaterialTypes", t => t.MaterialTypeId, cascadeDelete: true)
                .ForeignKey("dbo.Residents", t => t.ResidentId, cascadeDelete: true)
                .Index(t => t.ResidentId)
                .Index(t => t.MaterialTypeId)
                .Index(t => t.DropOffPointId)
                .Index(t => t.VerifiedBy);
            
            CreateTable(
                "dbo.DropOffPoints",
                c => new
                    {
                        DropOffPointId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Address = c.String(nullable: false),
                        City = c.String(),
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
                    })
                .PrimaryKey(t => t.EventId)
                .ForeignKey("dbo.DropOffPoints", t => t.DropOffPointId, cascadeDelete: true)
                .Index(t => t.DropOffPointId);
            
            CreateTable(
                "dbo.CollectionOfficers",
                c => new
                    {
                        OfficerId = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        FullName = c.String(nullable: false),
                        PhoneNumber = c.String(nullable: false),
                        DropOffPointId = c.Int(),
                        EmployeeNumber = c.String(),
                        Department = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.OfficerId)
                .ForeignKey("dbo.DropOffPoints", t => t.DropOffPointId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.DropOffPointId);
            
            CreateTable(
                "dbo.Residents",
                c => new
                    {
                        ResidentId = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        FullName = c.String(nullable: false),
                        PhoneNumber = c.String(nullable: false),
                        Address = c.String(),
                        Suburb = c.String(),
                        City = c.String(),
                        PointsBalance = c.Int(nullable: false),
                        InfluencerPoints = c.Int(nullable: false),
                        TotalCO2Saved = c.Double(nullable: false),
                        TotalReferrals = c.Int(nullable: false),
                        ReferralCode = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ResidentId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.PointsTransactions",
                c => new
                    {
                        TransactionId = c.Int(nullable: false, identity: true),
                        ResidentId = c.Int(nullable: false),
                        TransactionDate = c.DateTime(nullable: false),
                        Amount = c.Int(nullable: false),
                        Description = c.String(nullable: false),
                        Type = c.String(nullable: false),
                        ReferenceId = c.Int(),
                        Reason = c.String(),
                    })
                .PrimaryKey(t => t.TransactionId)
                .ForeignKey("dbo.Residents", t => t.ResidentId, cascadeDelete: true)
                .Index(t => t.ResidentId);
            
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
                        ConfigKey = c.String(nullable: false),
                        ConfigValue = c.String(nullable: false),
                        Description = c.String(),
                        UpdatedDate = c.DateTime(nullable: false),
                        UpdatedBy = c.String(),
                    })
                .PrimaryKey(t => t.ConfigId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Residents", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.RecyclingSubmissions", "ResidentId", "dbo.Residents");
            DropForeignKey("dbo.PointsTransactions", "ResidentId", "dbo.Residents");
            DropForeignKey("dbo.RecyclingSubmissions", "MaterialTypeId", "dbo.MaterialTypes");
            DropForeignKey("dbo.RecyclingSubmissions", "DropOffPointId", "dbo.DropOffPoints");
            DropForeignKey("dbo.RecyclingSubmissions", "VerifiedBy", "dbo.CollectionOfficers");
            DropForeignKey("dbo.CollectionOfficers", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.CollectionOfficers", "DropOffPointId", "dbo.DropOffPoints");
            DropForeignKey("dbo.CollectionEvents", "DropOffPointId", "dbo.DropOffPoints");
            DropForeignKey("dbo.PointsRates", "MaterialTypeId", "dbo.MaterialTypes");
            DropForeignKey("dbo.CO2Factor", "MaterialTypeId", "dbo.MaterialTypes");
            DropForeignKey("dbo.Administrators", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.PointsTransactions", new[] { "ResidentId" });
            DropIndex("dbo.Residents", new[] { "UserId" });
            DropIndex("dbo.CollectionOfficers", new[] { "DropOffPointId" });
            DropIndex("dbo.CollectionOfficers", new[] { "UserId" });
            DropIndex("dbo.CollectionEvents", new[] { "DropOffPointId" });
            DropIndex("dbo.RecyclingSubmissions", new[] { "VerifiedBy" });
            DropIndex("dbo.RecyclingSubmissions", new[] { "DropOffPointId" });
            DropIndex("dbo.RecyclingSubmissions", new[] { "MaterialTypeId" });
            DropIndex("dbo.RecyclingSubmissions", new[] { "ResidentId" });
            DropIndex("dbo.PointsRates", new[] { "MaterialTypeId" });
            DropIndex("dbo.CO2Factor", new[] { "MaterialTypeId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Administrators", new[] { "UserId" });
            DropTable("dbo.SystemConfigurations");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.PointsTransactions");
            DropTable("dbo.Residents");
            DropTable("dbo.CollectionOfficers");
            DropTable("dbo.CollectionEvents");
            DropTable("dbo.DropOffPoints");
            DropTable("dbo.RecyclingSubmissions");
            DropTable("dbo.PointsRates");
            DropTable("dbo.MaterialTypes");
            DropTable("dbo.CO2Factor");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Administrators");
        }
    }
}
