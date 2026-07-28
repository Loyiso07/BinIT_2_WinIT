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

        //  Admin Creation Audit
        public DbSet<AdminCreationAudit> AdminCreationAudits { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //  Specify column length for ReferralCode
            modelBuilder.Entity<Resident>()
                .Property(r => r.ReferralCode)
                .HasMaxLength(50);

            // Configure ReferralTransaction with NO CASCADE DELETE
            modelBuilder.Entity<ReferralTransaction>()
                .HasRequired(r => r.Referrer)
                .WithMany(r => r.ReferralsMade)
                .HasForeignKey(r => r.ReferrerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ReferralTransaction>()
                .HasRequired(r => r.NewResident)
                .WithMany()
                .HasForeignKey(r => r.NewResidentId)
                .WillCascadeOnDelete(false);

            //  Configure AdminCreationAudit with NO CASCADE DELETE
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
            
            // Configure SmartBin relationships
            modelBuilder.Entity<SmartBin>()
                .HasRequired(b => b.DropOffPoint)
                .WithMany()
                .HasForeignKey(b => b.DropOffPointId)
                .WillCascadeOnDelete(false);
        }



        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}