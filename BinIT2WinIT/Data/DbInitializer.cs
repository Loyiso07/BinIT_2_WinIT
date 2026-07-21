using System;
using System.Linq;
<<<<<<< HEAD
using BinIT2WinIT.Models;
using global::SmartRecycling.Data;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
=======
using System.Threading.Tasks;
using BinIT2WinIT.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using SmartRecycling.Data;
>>>>>>> master

namespace BinIT2WinIT.Data
{
    public static class DbInitializer
    {
<<<<<<< HEAD
        public static void Seed(ApplicationDbContext context)
=======
        public static async Task Seed(ApplicationDbContext context)
>>>>>>> master
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            // Create Roles
            string[] roleNames = { "Administrator", "CollectionOfficer", "Resident" };
            foreach (var roleName in roleNames)
            {
<<<<<<< HEAD
                if (!roleManager.RoleExists(roleName))
                {
                    roleManager.Create(new IdentityRole(roleName));
=======
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
>>>>>>> master
                }
            }

            // Create Admin User
            var adminEmail = "admin@recycle.com";
<<<<<<< HEAD
            var adminUser = userManager.FindByEmail(adminEmail);
=======
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
>>>>>>> master
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
<<<<<<< HEAD
                var result = userManager.Create(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    userManager.AddToRole(adminUser.Id, "Administrator");
=======
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser.Id, "Administrator");
>>>>>>> master
                }
            }

            // Create Sample Officer User
            var officerEmail = "officer@recycle.com";
<<<<<<< HEAD
            var officerUser = userManager.FindByEmail(officerEmail);
=======
            var officerUser = await userManager.FindByEmailAsync(officerEmail);
>>>>>>> master
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
<<<<<<< HEAD
                var result = userManager.Create(officerUser, "Officer@123");
                if (result.Succeeded)
                {
                    userManager.AddToRole(officerUser.Id, "CollectionOfficer");
=======
                var result = await userManager.CreateAsync(officerUser, "Officer@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(officerUser.Id, "CollectionOfficer");
>>>>>>> master
                }
            }

            // Create Sample Resident User
            var residentEmail = "resident@recycle.com";
<<<<<<< HEAD
            var residentUser = userManager.FindByEmail(residentEmail);
=======
            var residentUser = await userManager.FindByEmailAsync(residentEmail);
>>>>>>> master
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
<<<<<<< HEAD
                var result = userManager.Create(residentUser, "Resident@123");
                if (result.Succeeded)
                {
                    userManager.AddToRole(residentUser.Id, "Resident");
                }
            }

            // Seed Material Types - ✅ FIXED
=======
                var result = await userManager.CreateAsync(residentUser, "Resident@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(residentUser.Id, "Resident");
                }
            }

            // Seed Material Types
>>>>>>> master
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
<<<<<<< HEAD
                context.SaveChanges();
            }

            // Seed Points Rates - ✅ FIXED
=======
                await context.SaveChangesAsync();
            }

            // Seed Points Rates
>>>>>>> master
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
<<<<<<< HEAD
                context.SaveChanges();
            }

            // Seed DropOff Points - ✅ FIXED
=======
                await context.SaveChangesAsync();
            }

            // Seed DropOff Points
>>>>>>> master
            if (!context.DropOffPoints.Any())
            {
                context.DropOffPoints.AddRange(new DropOffPoint[]
                {
                    new DropOffPoint { Name = "Durban City Centre", Address = "123 Main Street", City = "Durban", Suburb = "CBD", ContactPerson = "John Doe", PhoneNumber = "031-555-0101", IsActive = true },
                    new DropOffPoint { Name = "Umhlanga Recycling Hub", Address = "45 Lighthouse Road", City = "Durban", Suburb = "Umhlanga", ContactPerson = "Jane Smith", PhoneNumber = "031-555-0102", IsActive = true },
                    new DropOffPoint { Name = "Pinetown Collection Point", Address = "789 Old Main Road", City = "Durban", Suburb = "Pinetown", ContactPerson = "Peter Mokoena", PhoneNumber = "031-555-0103", IsActive = true }
                });
<<<<<<< HEAD
                context.SaveChanges();
            }

            // Seed System Configurations - ✅ FIXED
=======
                await context.SaveChangesAsync();
            }

            // Seed System Configurations
>>>>>>> master
            if (!context.SystemConfigurations.Any())
            {
                context.SystemConfigurations.AddRange(new SystemConfiguration[]
                {
                    new SystemConfiguration { ConfigKey = "WelcomeBonusPoints", ConfigValue = "100", Description = "Points awarded to new residents upon registration" },
                    new SystemConfiguration { ConfigKey = "InfluencerPointsPerReferral", ConfigValue = "50", Description = "Influencer points earned per successful referral" }
                });
<<<<<<< HEAD
                context.SaveChanges();
=======
                await context.SaveChangesAsync();
>>>>>>> master
            }
        }
    }
}