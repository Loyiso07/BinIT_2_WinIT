using BinIT2WinIT.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace SmartRecycling.Data
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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and constraints here
            // For indexes, use the [Index] attribute on your models
            // or add them via migrations
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public System.Data.Entity.DbSet<BinIT2WinIT.Models.ApplicationUser> ApplicationUsers { get; set; }
    }
}