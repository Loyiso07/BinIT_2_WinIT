using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using BinIT2WinIT.Models;

namespace BinIT2WinIT.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext() : base("DefaultConnection")
        {
        }

        // Existing DbSets
        public DbSet<Resident> Residents { get; set; }
        public DbSet<CollectionOfficer> CollectionOfficers { get; set; }
        public DbSet<Administrator> Administrators { get; set; }
        public DbSet<RecyclingSubmission> RecyclingSubmissions { get; set; }
        public DbSet<MaterialType> MaterialTypes { get; set; }
        public DbSet<DropOffPoint> DropOffPoints { get; set; }
        public DbSet<PointsRate> PointsRates { get; set; }
        public DbSet<CO2Factor> CO2Factors { get; set; }
        public DbSet<PointsTransaction> PointsTransactions { get; set; }
        public DbSet<CollectionEvent> CollectionEvents { get; set; }
        public DbSet<SystemConfiguration> SystemConfigurations { get; set; }
        public DbSet<ReferralTransaction> ReferralTransactions { get; set; }
        public DbSet<SmartBin> SmartBins { get; set; }
        public DbSet<BinAlert> BinAlerts { get; set; }
        public DbSet<CommunityStatus> CommunityStatuses { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<RedemptionOption> RedemptionOptions { get; set; }
        public DbSet<RedemptionRequest> RedemptionRequests { get; set; }
        public DbSet<AdminCreationAudit> AdminCreationAudits { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================================
            // RESIDENT CONFIGURATIONS
            // ============================================================
            modelBuilder.Entity<Resident>()
                .Property(r => r.ReferralCode)
                .HasMaxLength(50);

            modelBuilder.Entity<Resident>()
                .Property(r => r.FullName)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Resident>()
                .Property(r => r.Address)
                .HasMaxLength(200);

            modelBuilder.Entity<Resident>()
                .Property(r => r.Suburb)
                .HasMaxLength(50);

            modelBuilder.Entity<Resident>()
                .Property(r => r.City)
                .HasMaxLength(50);

            modelBuilder.Entity<Resident>()
                .Property(r => r.PhoneNumber)
                .HasMaxLength(20);

            // Resident relationships
            modelBuilder.Entity<Resident>()
                .HasOptional(r => r.Community)
                .WithMany()
                .HasForeignKey(r => r.DropOffPointId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Resident>()
                .HasMany(r => r.Submissions)
                .WithRequired(s => s.Resident)
                .HasForeignKey(s => s.ResidentId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Resident>()
                .HasMany(r => r.PointsTransactions)
                .WithRequired(t => t.Resident)
                .HasForeignKey(t => t.ResidentId)
                .WillCascadeOnDelete(false);

            // ============================================================
            // REDEMPTION OPTION CONFIGURATIONS
            // ============================================================
            modelBuilder.Entity<RedemptionOption>()
                .HasKey(o => o.OptionId);

            modelBuilder.Entity<RedemptionOption>()
                .Property(o => o.UtilityType)
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<RedemptionOption>()
                .Property(o => o.Description)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<RedemptionOption>()
                .Property(o => o.Icon)
                .HasMaxLength(50);

            // RedemptionOption relationships
            modelBuilder.Entity<RedemptionOption>()
                .HasMany(o => o.RedemptionRequests)
                .WithRequired(r => r.RedemptionOption)
                .HasForeignKey(r => r.OptionId)
                .WillCascadeOnDelete(false);

            // ============================================================
            // REDEMPTION REQUEST CONFIGURATIONS
            // ============================================================
            modelBuilder.Entity<RedemptionRequest>()
                .HasKey(r => r.RequestId);

            modelBuilder.Entity<RedemptionRequest>()
                .Property(r => r.UtilityAccountNumber)
                .HasMaxLength(50);

            modelBuilder.Entity<RedemptionRequest>()
                .Property(r => r.UtilityType)
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<RedemptionRequest>()
                .Property(r => r.RequestStatus)
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<RedemptionRequest>()
                .Property(r => r.ReferenceNumber)
                .HasMaxLength(50);

            // RedemptionRequest relationships
            modelBuilder.Entity<RedemptionRequest>()
                .HasRequired(r => r.Resident)
                .WithMany()
                .HasForeignKey(r => r.ResidentId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RedemptionRequest>()
                .HasRequired(r => r.RedemptionOption)
                .WithMany(o => o.RedemptionRequests)
                .HasForeignKey(r => r.OptionId)
                .WillCascadeOnDelete(false);

            // ============================================================
            // DROP OFF POINT CONFIGURATIONS
            // ============================================================
            modelBuilder.Entity<DropOffPoint>()
                .HasKey(d => d.DropOffPointId);

            modelBuilder.Entity<DropOffPoint>()
                .Property(d => d.Name)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<DropOffPoint>()
                .Property(d => d.Address)
                .HasMaxLength(200);

            modelBuilder.Entity<DropOffPoint>()
                .Property(d => d.City)
                .HasMaxLength(50);

            // ============================================================
            // MATERIAL TYPE CONFIGURATIONS
            // ============================================================
            modelBuilder.Entity<MaterialType>()
                .HasKey(m => m.MaterialTypeId);

            modelBuilder.Entity<MaterialType>()
                .Property(m => m.Name)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<MaterialType>()
                .Property(m => m.Description)
                .HasMaxLength(200);

            // ============================================================
            // POINTS RATE CONFIGURATIONS
            // ============================================================
            modelBuilder.Entity<PointsRate>()
                .HasKey(p => p.PointsRateId);

            modelBuilder.Entity<PointsRate>()
                .HasRequired(p => p.MaterialType)
                .WithMany()
                .HasForeignKey(p => p.MaterialTypeId)
                .WillCascadeOnDelete(false);

            // ============================================================
            // ✅ FIX: RECYCLING SUBMISSION - Use HasOptional for nullable DropOffPointId
            // ============================================================
            modelBuilder.Entity<RecyclingSubmission>()
                .HasKey(s => s.SubmissionId);

            modelBuilder.Entity<RecyclingSubmission>()
                .Property(s => s.Status)
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<RecyclingSubmission>()
                .HasRequired(s => s.Resident)
                .WithMany(r => r.Submissions)
                .HasForeignKey(s => s.ResidentId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RecyclingSubmission>()
                .HasRequired(s => s.MaterialType)
                .WithMany()
                .HasForeignKey(s => s.MaterialTypeId)
                .WillCascadeOnDelete(false);

            // ✅ CRITICAL: DropOffPoint is OPTIONAL (nullable)
            modelBuilder.Entity<RecyclingSubmission>()
                .HasOptional(s => s.DropOffPoint)  // HasOptional because DropOffPointId is nullable
                .WithMany()
                .HasForeignKey(s => s.DropOffPointId)
                .WillCascadeOnDelete(false);

            // ============================================================
            // POINTS TRANSACTION CONFIGURATIONS
            // ============================================================
            modelBuilder.Entity<PointsTransaction>()
                .HasKey(t => t.TransactionId);

            modelBuilder.Entity<PointsTransaction>()
                .Property(t => t.Description)
                .HasMaxLength(500);

            modelBuilder.Entity<PointsTransaction>()
                .Property(t => t.Type)
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<PointsTransaction>()
                .Property(t => t.Reason)
                .HasMaxLength(200);

            modelBuilder.Entity<PointsTransaction>()
                .HasRequired(t => t.Resident)
                .WithMany(r => r.PointsTransactions)
                .HasForeignKey(t => t.ResidentId)
                .WillCascadeOnDelete(false);

            // ============================================================
            // REFERRAL TRANSACTION CONFIGURATIONS
            // ============================================================
            modelBuilder.Entity<ReferralTransaction>()
                .HasRequired(t => t.Referrer)
                .WithMany(r => r.ReferralsMade)
                .HasForeignKey(t => t.ReferrerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ReferralTransaction>()
                .HasRequired(t => t.NewResident)
                .WithMany()
                .HasForeignKey(t => t.NewResidentId)
                .WillCascadeOnDelete(false);

            // ============================================================
            // ADMIN CREATION AUDIT CONFIGURATIONS
            // ============================================================
            modelBuilder.Entity<AdminCreationAudit>()
                .HasKey(a => a.AuditId);

            modelBuilder.Entity<AdminCreationAudit>()
                .Property(a => a.NewAdminEmail)
                .HasMaxLength(256)
                .IsRequired();

            modelBuilder.Entity<AdminCreationAudit>()
                .Property(a => a.NewAdminName)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<AdminCreationAudit>()
                .Property(a => a.CreatedByName)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<AdminCreationAudit>()
                .HasRequired(a => a.NewAdmin)
                .WithMany()
                .HasForeignKey(a => a.NewAdminUserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AdminCreationAudit>()
                .HasRequired(a => a.CreatedBy)
                .WithMany()
                .HasForeignKey(a => a.CreatedByUserId)
                .WillCascadeOnDelete(false);

            // ============================================================
            // COLLECTION OFFICER CONFIGURATIONS
            // ============================================================
            modelBuilder.Entity<CollectionOfficer>()
                .HasKey(o => o.OfficerId);

            modelBuilder.Entity<CollectionOfficer>()
                .Property(o => o.FullName)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<CollectionOfficer>()
                .Property(o => o.EmployeeNumber)
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<CollectionOfficer>()
                .Property(o => o.Department)
                .HasMaxLength(50);

            modelBuilder.Entity<CollectionOfficer>()
                .Property(o => o.PhoneNumber)
                .HasMaxLength(20);

            modelBuilder.Entity<CollectionOfficer>()
                .HasOptional(o => o.AssignedDropOffPoint)
                .WithMany()
                .HasForeignKey(o => o.DropOffPointId)
                .WillCascadeOnDelete(false);

            // ============================================================
            // ADMINISTRATOR CONFIGURATIONS
            // ============================================================
            modelBuilder.Entity<Administrator>()
                .HasKey(a => a.AdminId);

            modelBuilder.Entity<Administrator>()
                .Property(a => a.FullName)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Administrator>()
                .Property(a => a.Department)
                .HasMaxLength(50);

            modelBuilder.Entity<Administrator>()
                .Property(a => a.Email)
                .HasMaxLength(100)
                .IsRequired();

            // ============================================================
            // SMART BIN CONFIGURATIONS
            // ============================================================
            modelBuilder.Entity<SmartBin>()
                .HasKey(b => b.BinId);

            modelBuilder.Entity<SmartBin>()
                .Property(b => b.BinName)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<SmartBin>()
                .HasRequired(b => b.DropOffPoint)
                .WithMany()
                .HasForeignKey(b => b.DropOffPointId)
                .WillCascadeOnDelete(false);

            // ============================================================
            // BIN ALERT CONFIGURATIONS
            // ============================================================
            modelBuilder.Entity<BinAlert>()
                .HasKey(b => b.AlertId);

            modelBuilder.Entity<BinAlert>()
                .Property(b => b.AlertType)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<BinAlert>()
                .HasRequired(b => b.SmartBin)
                .WithMany()
                .HasForeignKey(b => b.BinId)
                .WillCascadeOnDelete(false);

            // ============================================================
            // COMMUNITY STATUS CONFIGURATIONS
            // ============================================================
            modelBuilder.Entity<CommunityStatus>()
                .Property(c => c.Status)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<CommunityStatus>()
                .Property(c => c.Notes)
                .HasMaxLength(500);

            modelBuilder.Entity<CommunityStatus>()
                .Property(c => c.UpdatedBy)
                .HasMaxLength(100);

            modelBuilder.Entity<CommunityStatus>()
                .HasRequired(c => c.DropOffPoint)
                .WithMany()
                .HasForeignKey(c => c.DropOffPointId)
                .WillCascadeOnDelete(false);

            // ============================================================
            // ANNOUNCEMENT CONFIGURATIONS
            // ============================================================
            modelBuilder.Entity<Announcement>()
                .HasKey(a => a.AnnouncementId);

            modelBuilder.Entity<Announcement>()
                .Property(a => a.Title)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<Announcement>()
                .Property(a => a.Message)
                .IsRequired();

            modelBuilder.Entity<Announcement>()
                .Property(a => a.TargetAudience)
                .HasMaxLength(20);

            modelBuilder.Entity<Announcement>()
                .Property(a => a.CreatedBy)
                .HasMaxLength(100);

            // ============================================================
            // NOTIFICATION CONFIGURATIONS
            // ============================================================
            modelBuilder.Entity<Notification>()
                .HasKey(n => n.NotificationId);

            modelBuilder.Entity<Notification>()
                .Property(n => n.Title)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<Notification>()
                .Property(n => n.Message)
                .IsRequired();

            modelBuilder.Entity<Notification>()
                .Property(n => n.Type)
                .HasMaxLength(50);

            modelBuilder.Entity<Notification>()
                .Property(n => n.Link)
                .HasMaxLength(500);

            modelBuilder.Entity<Notification>()
                .HasRequired(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .WillCascadeOnDelete(false);

            // ============================================================
            // APPLICATION USER CONFIGURATIONS
            // ============================================================
            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.FullName)
                .HasMaxLength(100);

            // ============================================================
            // CO2 FACTOR CONFIGURATIONS
            // ============================================================
            modelBuilder.Entity<CO2Factor>()
                .HasKey(c => c.CO2FactorId);

            modelBuilder.Entity<CO2Factor>()
                .HasRequired(c => c.MaterialType)
                .WithMany()
                .HasForeignKey(c => c.MaterialTypeId)
                .WillCascadeOnDelete(false);

            // ============================================================
            // COLLECTION EVENT CONFIGURATIONS
            // ============================================================
            modelBuilder.Entity<CollectionEvent>()
                .HasKey(e => e.EventId);

            modelBuilder.Entity<CollectionEvent>()
                .HasRequired(e => e.DropOffPoint)
                .WithMany()
                .HasForeignKey(e => e.DropOffPointId)
                .WillCascadeOnDelete(false);

            // ============================================================
            // SYSTEM CONFIGURATION CONFIGURATIONS
            // ============================================================
            modelBuilder.Entity<SystemConfiguration>()
                .HasKey(c => c.ConfigId);

            modelBuilder.Entity<SystemConfiguration>()
                .Property(c => c.ConfigKey)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<SystemConfiguration>()
                .Property(c => c.ConfigValue)
                .HasMaxLength(500)
                .IsRequired();

            modelBuilder.Entity<SystemConfiguration>()
                .Property(c => c.Description)
                .HasMaxLength(500);
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}