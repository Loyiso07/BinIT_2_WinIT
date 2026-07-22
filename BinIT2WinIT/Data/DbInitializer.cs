using System;
using System.Linq;
using System.Threading.Tasks;
using BinIT2WinIT.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using BinIT2WinIT.Data;

namespace BinIT2WinIT.Data
{
    public static class DbInitializer
    {
        public static async Task Seed(ApplicationDbContext context)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            // Create Roles
            string[] roleNames = { "Administrator", "CollectionOfficer", "Resident" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create Admin User
            var adminEmail = "admin@recycle.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true,
                    IsActive = true
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser.Id, "Administrator");
                }
            }

            // Create Sample Officer User
            var officerEmail = "officer@recycle.com";
            var officerUser = await userManager.FindByEmailAsync(officerEmail);
            if (officerUser == null)
            {
                officerUser = new ApplicationUser
                {
                    UserName = officerEmail,
                    Email = officerEmail,
                    FullName = "Collection Officer",
                    EmailConfirmed = true,
                    IsActive = true
                };
                var result = await userManager.CreateAsync(officerUser, "Officer@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(officerUser.Id, "CollectionOfficer");
                }
            }

            // Create Sample Resident User
            var residentEmail = "resident@recycle.com";
            var residentUser = await userManager.FindByEmailAsync(residentEmail);
            if (residentUser == null)
            {
                residentUser = new ApplicationUser
                {
                    UserName = residentEmail,
                    Email = residentEmail,
                    FullName = "Resident User",
                    EmailConfirmed = true,
                    IsActive = true
                };
                var result = await userManager.CreateAsync(residentUser, "Resident@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(residentUser.Id, "Resident");
                }
            }

            // Seed Material Types
            if (!context.MaterialTypes.Any())
            {
                context.MaterialTypes.AddRange(new MaterialType[]
                {
                    new MaterialType { Name = "Glass", Description = "Glass bottles and jars", CreatedAt = DateTime.Now },
                    new MaterialType { Name = "Plastic", Description = "Plastic bottles and containers", CreatedAt = DateTime.Now },
                    new MaterialType { Name = "Paper", Description = "Paper and cardboard", CreatedAt = DateTime.Now },
                    new MaterialType { Name = "Metal", Description = "Metal cans and containers", CreatedAt = DateTime.Now },
                    new MaterialType { Name = "E-Waste", Description = "Electronic waste", CreatedAt = DateTime.Now }
                });
                await context.SaveChangesAsync();
            }

            // Seed Points Rates
            if (!context.PointsRates.Any())
            {
                var materials = context.MaterialTypes.ToList();
                context.PointsRates.AddRange(new PointsRate[]
                {
                    new PointsRate { MaterialTypeId = materials.First(m => m.Name == "Glass").MaterialTypeId, PointsPerKg = 5, IsActive = true },
                    new PointsRate { MaterialTypeId = materials.First(m => m.Name == "Plastic").MaterialTypeId, PointsPerKg = 4, IsActive = true },
                    new PointsRate { MaterialTypeId = materials.First(m => m.Name == "Paper").MaterialTypeId, PointsPerKg = 3, IsActive = true },
                    new PointsRate { MaterialTypeId = materials.First(m => m.Name == "Metal").MaterialTypeId, PointsPerKg = 6, IsActive = true },
                    new PointsRate { MaterialTypeId = materials.First(m => m.Name == "E-Waste").MaterialTypeId, PointsPerKg = 8, IsActive = true }
                });
                await context.SaveChangesAsync();
            }

            // Seed DropOff Points
            if (!context.DropOffPoints.Any())
            {
                context.DropOffPoints.AddRange(new DropOffPoint[]
                {
                    new DropOffPoint { Name = "Durban City Centre", Address = "123 Main Street", City = "Durban", Suburb = "CBD", ContactPerson = "John Doe", PhoneNumber = "031-555-0101", IsActive = true },
                    new DropOffPoint { Name = "Umhlanga Recycling Hub", Address = "45 Lighthouse Road", City = "Durban", Suburb = "Umhlanga", ContactPerson = "Jane Smith", PhoneNumber = "031-555-0102", IsActive = true },
                    new DropOffPoint { Name = "Pinetown Collection Point", Address = "789 Old Main Road", City = "Durban", Suburb = "Pinetown", ContactPerson = "Peter Mokoena", PhoneNumber = "031-555-0103", IsActive = true }
                });
                await context.SaveChangesAsync();
            }

            // Seed System Configurations
            if (!context.SystemConfigurations.Any())
            {
                context.SystemConfigurations.AddRange(new SystemConfiguration[]
                {
                    new SystemConfiguration { ConfigKey = "WelcomeBonusPoints", ConfigValue = "100", Description = "Points awarded to new residents upon registration" },
                    new SystemConfiguration { ConfigKey = "InfluencerPointsPerReferral", ConfigValue = "50", Description = "Influencer points earned per successful referral" }
                });
                await context.SaveChangesAsync();
            }
        }
    }
}