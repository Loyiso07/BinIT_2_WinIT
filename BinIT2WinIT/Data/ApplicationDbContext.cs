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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ Specify column length for ReferralCode
            modelBuilder.Entity<Resident>()
                .Property(r => r.ReferralCode)
                .HasMaxLength(50);

            // ❌ REMOVED HasPrecision (not needed for double)
            // modelBuilder.Entity<RecyclingSubmission>()
            //     .Property(s => s.Weight)
            //     .HasPrecision(18, 2);

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
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}