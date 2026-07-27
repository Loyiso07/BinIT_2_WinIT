using BinIT2WinIT.Data;
using BinIT2WinIT.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace BinIT2WinIT.Services
{
    public class IoTSimulationService : IIoTSimulationService
    {
        private readonly ApplicationDbContext _context;
        private readonly Random _random = new Random();

        public IoTSimulationService()
        {
            _context = new ApplicationDbContext();
        }

        public async Task SimulateBinData()
        {
            var bins = await _context.SmartBins
                .Include(b => b.DropOffPoint)
                .ToListAsync();

            foreach (var bin in bins)
            {
                if (!bin.IsActive) continue;

                // Simulate fill level change
                var change = _random.Next(-3, 8);
                bin.FillLevel = Math.Min(100, Math.Max(0, bin.FillLevel + change));
                bin.CurrentWeight = Math.Min(bin.Capacity, Math.Max(0, bin.CurrentWeight + change * 0.5));
                bin.BatteryLevel = Math.Max(10, bin.BatteryLevel - _random.Next(0, 2));

                if (bin.BatteryLevel < 15)
                {
                    bin.Status = "Maintenance";
                }
                else if (bin.FillLevel >= 90)
                {
                    bin.Status = "Full";
                }
                else
                {
                    bin.Status = "Online";
                }

                bin.LastUpdated = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            await CheckAndSendAlerts();
        }

        public async Task CheckAndSendAlerts()
        {
            var bins = await _context.SmartBins
                .Include(b => b.DropOffPoint)
                .ToListAsync();

            foreach (var bin in bins)
            {
                // Full Bin Alert
                if (bin.FillLevel >= 90 && !bin.IsFullAlertSent)
                {
                    // Find officer for this bin's drop-off point
                    var officer = await _context.CollectionOfficers
                        .FirstOrDefaultAsync(o => o.DropOffPointId == bin.DropOffPointId);

                    var alert = new BinAlert
                    {
                        BinId = bin.BinId,
                        AlertType = "Full",
                        Message = $"🚨 Bin '{bin.BinName}' at {bin.Location} is {bin.FillLevel}% full. Please collect.",
                        CreatedAt = DateTime.Now,
                        IsResolved = false,
                        AssignedOfficerId = officer?.OfficerId
                    };
                    _context.BinAlerts.Add(alert);
                    bin.IsFullAlertSent = true;
                }

                if (bin.FillLevel < 70 && bin.IsFullAlertSent)
                {
                    bin.IsFullAlertSent = false;
                }

                // Maintenance Alert
                if ((bin.BatteryLevel < 15 || bin.Status == "Offline") && !bin.IsMaintenanceAlertSent)
                {
                    var officer = await _context.CollectionOfficers
                        .FirstOrDefaultAsync(o => o.DropOffPointId == bin.DropOffPointId);

                    var alert = new BinAlert
                    {
                        BinId = bin.BinId,
                        AlertType = "Maintenance",
                        Message = $"⚠️ Bin '{bin.BinName}' at {bin.Location} requires maintenance. Battery: {bin.BatteryLevel}%",
                        CreatedAt = DateTime.Now,
                        IsResolved = false,
                        AssignedOfficerId = officer?.OfficerId
                    };
                    _context.BinAlerts.Add(alert);
                    bin.IsMaintenanceAlertSent = true;
                }

                if (bin.BatteryLevel > 30 && bin.Status == "Online" && bin.IsMaintenanceAlertSent)
                {
                    bin.IsMaintenanceAlertSent = false;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}