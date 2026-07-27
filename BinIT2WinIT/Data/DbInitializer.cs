using System;
using System.Collections.Generic;
using System.Linq;
using BinIT2WinIT.Models;
using global::BinIT2WinIT.Data;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace BinIT2WinIT.Data
{
    public static class DbInitializer
    {
        public static void Seed(ApplicationDbContext context)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            // ============================================================
            // 1. Create Roles
            // ============================================================
            string[] roleNames = { "Administrator", "CollectionOfficer", "Resident" };
            foreach (var roleName in roleNames)
            {
                if (!roleManager.RoleExists(roleName))
                {
                    roleManager.Create(new IdentityRole(roleName));
                }
            }

            // ============================================================
            // 2. Create Admin User
            // ============================================================
            var adminEmail = "admin@recycle.com";
            var adminUser = userManager.FindByEmail(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                var result = userManager.Create(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    userManager.AddToRole(adminUser.Id, "Administrator");
                }
            }

            // ============================================================
            // 3. Create Sample Officer User
            // ============================================================
            var officerEmail = "officer@recycle.com";
            var officerUser = userManager.FindByEmail(officerEmail);
            if (officerUser == null)
            {
                officerUser = new ApplicationUser
                {
                    UserName = officerEmail,
                    Email = officerEmail,
                    FullName = "Collection Officer",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                var result = userManager.Create(officerUser, "Officer@123");
                if (result.Succeeded)
                {
                    userManager.AddToRole(officerUser.Id, "CollectionOfficer");
                }
            }

            // ============================================================
            // 4. Create Sample Resident User
            // ============================================================
            var residentEmail = "resident@recycle.com";
            var residentUser = userManager.FindByEmail(residentEmail);
            if (residentUser == null)
            {
                residentUser = new ApplicationUser
                {
                    UserName = residentEmail,
                    Email = residentEmail,
                    FullName = "Resident User",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                var result = userManager.Create(residentUser, "Resident@123");
                if (result.Succeeded)
                {
                    userManager.AddToRole(residentUser.Id, "Resident");
                }
            }

            // ============================================================
            // 5. Seed Material Types
            // ============================================================
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
                context.SaveChanges();
            }

            // ============================================================
            // 6. Seed Points Rates
            // ============================================================
            if (!context.PointsRates.Any())
            {
                var materials = context.MaterialTypes.ToList();
                context.PointsRates.AddRange(new PointsRate[]
                {
                    new PointsRate { MaterialTypeId = materials.First(m => m.Name == "Glass").MaterialTypeId, PointsPerKg = 5, IsActive = true, EffectiveDate = DateTime.Now },
                    new PointsRate { MaterialTypeId = materials.First(m => m.Name == "Plastic").MaterialTypeId, PointsPerKg = 4, IsActive = true, EffectiveDate = DateTime.Now },
                    new PointsRate { MaterialTypeId = materials.First(m => m.Name == "Paper").MaterialTypeId, PointsPerKg = 3, IsActive = true, EffectiveDate = DateTime.Now },
                    new PointsRate { MaterialTypeId = materials.First(m => m.Name == "Metal").MaterialTypeId, PointsPerKg = 6, IsActive = true, EffectiveDate = DateTime.Now },
                    new PointsRate { MaterialTypeId = materials.First(m => m.Name == "E-Waste").MaterialTypeId, PointsPerKg = 8, IsActive = true, EffectiveDate = DateTime.Now }
                });
                context.SaveChanges();
            }

            // ============================================================
            // 7. Seed CO2 Factors
            // ============================================================
            if (!context.CO2Factors.Any())
            {
                var materials = context.MaterialTypes.ToList();
                context.CO2Factors.AddRange(new CO2Factor[]
                {
                    new CO2Factor { MaterialTypeId = materials.First(m => m.Name == "Glass").MaterialTypeId, CO2SavedPerKg = 0.5, IsActive = true, EffectiveDate = DateTime.Now },
                    new CO2Factor { MaterialTypeId = materials.First(m => m.Name == "Plastic").MaterialTypeId, CO2SavedPerKg = 1.5, IsActive = true, EffectiveDate = DateTime.Now },
                    new CO2Factor { MaterialTypeId = materials.First(m => m.Name == "Paper").MaterialTypeId, CO2SavedPerKg = 1.0, IsActive = true, EffectiveDate = DateTime.Now },
                    new CO2Factor { MaterialTypeId = materials.First(m => m.Name == "Metal").MaterialTypeId, CO2SavedPerKg = 2.0, IsActive = true, EffectiveDate = DateTime.Now },
                    new CO2Factor { MaterialTypeId = materials.First(m => m.Name == "E-Waste").MaterialTypeId, CO2SavedPerKg = 3.0, IsActive = true, EffectiveDate = DateTime.Now }
                });
                context.SaveChanges();
            }

            // ============================================================
            // 8. Seed Drop-Off Points (SAVE so IDs are generated)
            // ============================================================
            if (!context.DropOffPoints.Any())
            {
                context.DropOffPoints.AddRange(new DropOffPoint[]
                {
                    new DropOffPoint {
                        Name = "Durban City Centre",
                        Address = "123 Anton Lembede Street",
                        City = "Durban",
                        Suburb = "CBD",
                        ContactPerson = "John Doe",
                        PhoneNumber = "031-555-0101",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    },
                    new DropOffPoint {
                        Name = "Umhlanga Recycling Hub",
                        Address = "45 Lighthouse Road",
                        City = "Durban",
                        Suburb = "Umhlanga",
                        ContactPerson = "Jane Smith",
                        PhoneNumber = "031-555-0102",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    },
                    new DropOffPoint {
                        Name = "Pinetown Collection Point",
                        Address = "789 Old Main Road",
                        City = "Durban",
                        Suburb = "Pinetown",
                        ContactPerson = "Peter Mokoena",
                        PhoneNumber = "031-555-0103",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    },
                    new DropOffPoint {
                        Name = "Westville Recycling Centre",
                        Address = "45 Jan Smuts Highway",
                        City = "Durban",
                        Suburb = "Westville",
                        ContactPerson = "Sipho Ndlovu",
                        PhoneNumber = "031-555-0104",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    }
                });
                context.SaveChanges(); // ✅ Critical: Save so IDs are generated!
            }

            // ============================================================
            // 9. Seed System Configurations
            // ============================================================
            if (!context.SystemConfigurations.Any())
            {
                context.SystemConfigurations.AddRange(new SystemConfiguration[]
                {
                    new SystemConfiguration {
                        ConfigKey = "WelcomeBonusPoints",
                        ConfigValue = "100",
                        Description = "Points awarded to new residents upon registration",
                        UpdatedDate = DateTime.Now
                    },
                    new SystemConfiguration {
                        ConfigKey = "InfluencerPointsPerReferral",
                        ConfigValue = "50",
                        Description = "Influencer points earned per successful referral",
                        UpdatedDate = DateTime.Now
                    }
                });
                context.SaveChanges();
            }

            // ============================================================
            // 10. Seed Smart Bins (IoT) - AFTER DropOffPoints
            // ============================================================
            if (!context.SmartBins.Any())
            {
                // ✅ Get actual DropOffPoint IDs from database
                var durbanPoint = context.DropOffPoints.FirstOrDefault(d => d.Name == "Durban City Centre");
                var umhlangaPoint = context.DropOffPoints.FirstOrDefault(d => d.Name == "Umhlanga Recycling Hub");
                var pinetownPoint = context.DropOffPoints.FirstOrDefault(d => d.Name == "Pinetown Collection Point");
                var westvillePoint = context.DropOffPoints.FirstOrDefault(d => d.Name == "Westville Recycling Centre");

                var smartBins = new List<SmartBin>();

                if (durbanPoint != null)
                {
                    smartBins.Add(new SmartBin
                    {
                        BinName = "Bin 1 - City Centre",
                        Location = "123 Anton Lembede Street, Durban CBD",
                        DropOffPointId = durbanPoint.DropOffPointId,
                        Capacity = 50,
                        FillLevel = 45,
                        CurrentWeight = 22.5,
                        Status = "Online",
                        BatteryLevel = 92,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        LastUpdated = DateTime.Now,
                        IsFullAlertSent = false,
                        IsMaintenanceAlertSent = false
                    });
                }

                if (umhlangaPoint != null)
                {
                    smartBins.Add(new SmartBin
                    {
                        BinName = "Bin 2 - Umhlanga",
                        Location = "45 Lighthouse Road, Umhlanga",
                        DropOffPointId = umhlangaPoint.DropOffPointId,
                        Capacity = 60,
                        FillLevel = 78,
                        CurrentWeight = 46.8,
                        Status = "Warning",
                        BatteryLevel = 65,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        LastUpdated = DateTime.Now,
                        IsFullAlertSent = false,
                        IsMaintenanceAlertSent = false
                    });
                }

                if (pinetownPoint != null)
                {
                    smartBins.Add(new SmartBin
                    {
                        BinName = "Bin 3 - Pinetown",
                        Location = "789 Old Main Road, Pinetown",
                        DropOffPointId = pinetownPoint.DropOffPointId,
                        Capacity = 45,
                        FillLevel = 92,
                        CurrentWeight = 41.4,
                        Status = "Full",
                        BatteryLevel = 78,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        LastUpdated = DateTime.Now,
                        IsFullAlertSent = true,
                        IsMaintenanceAlertSent = false
                    });
                }

                if (westvillePoint != null)
                {
                    smartBins.Add(new SmartBin
                    {
                        BinName = "Bin 4 - Westville",
                        Location = "45 Jan Smuts Highway, Westville",
                        DropOffPointId = westvillePoint.DropOffPointId,
                        Capacity = 55,
                        FillLevel = 23,
                        CurrentWeight = 12.65,
                        Status = "Online",
                        BatteryLevel = 45,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        LastUpdated = DateTime.Now,
                        IsFullAlertSent = false,
                        IsMaintenanceAlertSent = false
                    });
                }

                if (smartBins.Any())
                {
                    context.SmartBins.AddRange(smartBins);
                    context.SaveChanges();
                }
            }

            // ============================================================
            // 11. Seed Initial Bin Alerts
            // ============================================================
            if (!context.BinAlerts.Any())
            {
                var fullBin = context.SmartBins.FirstOrDefault(b => b.FillLevel >= 90);
                if (fullBin != null)
                {
                    context.BinAlerts.Add(new BinAlert
                    {
                        BinId = fullBin.BinId,
                        AlertType = "Full",
                        Message = $"Bin '{fullBin.BinName}' at {fullBin.Location} is {fullBin.FillLevel}% full. Please collect.",
                        CreatedAt = DateTime.Now,
                        IsResolved = false
                    });
                    context.SaveChanges();
                }
            }
        }
    }
}