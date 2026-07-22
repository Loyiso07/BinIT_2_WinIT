using BinIT2WinIT.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace BinIT2WinIT.Data  // Make sure namespace matches your project
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext() : base("DefaultConnection")
        {
        }

        // ✅ CUSTOM TABLES - These are fine
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

        // ❌ REMOVE THIS LINE:
        // public System.Data.Entity.DbSet<BinIT2WinIT.Models.ApplicationUser> ApplicationUsers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and constraints here
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}