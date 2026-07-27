using BinIT2WinIT.Data;
using BinIT2WinIT.Models;
using BinIT2WinIT.Services;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BinIT2WinIT.Controllers
{
    public class IoTController : Controller
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();
        private readonly IIoTSimulationService _iotService;

        public IoTController()
        {
            _iotService = new IoTSimulationService();
        }

        // ============================================================
        // GET: IoT/Dashboard (Admin)
        // ============================================================
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Dashboard()
        {
            var bins = await _context.SmartBins
                .Include(b => b.DropOffPoint)
                .ToListAsync();

            var alerts = await _context.BinAlerts
                .Include(a => a.AssignedOfficer)
                .Where(a => !a.IsResolved)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            ViewBag.Alerts = alerts;
            ViewBag.TotalBins = bins.Count;
            ViewBag.FullBins = bins.Count(b => b.FillLevel >= 90);
            ViewBag.MaintenanceBins = bins.Count(b => b.Status == "Maintenance" || b.Status == "Offline");
            ViewBag.ActiveAlerts = alerts.Count;

            return View(bins);
        }

        // ============================================================
        // GET: IoT/OfficerBins (Officer)
        // ============================================================
        [Authorize(Roles = "CollectionOfficer")]
        public async Task<ActionResult> OfficerBins()
        {
            var userId = User.Identity.GetUserId();
            var officer = await _context.CollectionOfficers
                .Include(o => o.AssignedDropOffPoint)
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (officer == null)
            {
                return RedirectToAction("Dashboard", "Officer");
            }

            // ✅ Get bins assigned to officer's location
            var bins = await _context.SmartBins
                .Include(b => b.DropOffPoint)
                .Where(b => b.DropOffPointId == officer.DropOffPointId && b.IsActive)
                .ToListAsync();

            ViewBag.OfficerName = officer.FullName;
            ViewBag.DropOffPointName = officer.AssignedDropOffPoint?.Name ?? "Not Assigned";

            return View(bins);
        }

        // ============================================================
        // POST: IoT/EmptyBin (Officer only)
        // ============================================================
        [HttpPost]
        [Authorize(Roles = "CollectionOfficer")]
        public async Task<ActionResult> EmptyBin(int binId)
        {
            var userId = User.Identity.GetUserId();
            var officer = await _context.CollectionOfficers
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (officer == null)
            {
                TempData["ErrorMessage"] = "Officer profile not found.";
                return RedirectToAction("OfficerBins");
            }

            var bin = await _context.SmartBins.FindAsync(binId);
            if (bin == null)
            {
                TempData["ErrorMessage"] = "Bin not found.";
                return RedirectToAction("OfficerBins");
            }

            // ✅ Verify officer is assigned to this bin's location
            if (bin.DropOffPointId != officer.DropOffPointId)
            {
                TempData["ErrorMessage"] = "You are not authorized to empty this bin.";
                return RedirectToAction("OfficerBins");
            }

            // Empty the bin
            bin.FillLevel = 0;
            bin.CurrentWeight = 0;
            bin.Status = "Online";
            bin.IsFullAlertSent = false;
            bin.LastUpdated = DateTime.Now;

            // Resolve any full alerts for this bin
            var alerts = await _context.BinAlerts
                .Where(a => a.BinId == binId && a.AlertType == "Full" && !a.IsResolved)
                .ToListAsync();

            foreach (var alert in alerts)
            {
                alert.IsResolved = true;
                alert.ResolvedAt = DateTime.Now;
                alert.ResolvedBy = User.Identity.GetUserName();
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"✅ Bin '{bin.BinName}' has been emptied!";

            return RedirectToAction("OfficerBins");
        }

        // ============================================================
        // POST: IoT/SendAlert (Admin only)
        // ============================================================
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> SendAlert(int binId, string message, int? officerId)
        {
            var bin = await _context.SmartBins.FindAsync(binId);
            if (bin == null)
            {
                TempData["ErrorMessage"] = "Bin not found.";
                return RedirectToAction("Dashboard");
            }

            var alert = new BinAlert
            {
                BinId = binId,
                AlertType = "Urgent",
                Message = message ?? $"🚨 Urgent: Bin '{bin.BinName}' at {bin.Location} requires immediate attention.",
                CreatedAt = DateTime.Now,
                IsResolved = false,
                AssignedOfficerId = officerId
            };

            _context.BinAlerts.Add(alert);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"✅ Alert sent successfully!";
            return RedirectToAction("Dashboard");
        }

        // ============================================================
        // POST: IoT/AssignTempOfficer (Admin only)
        // ============================================================
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> AssignTempOfficer(int binId, int tempOfficerId, string reason)
        {
            var bin = await _context.SmartBins.FindAsync(binId);
            if (bin == null)
            {
                TempData["ErrorMessage"] = "Bin not found.";
                return RedirectToAction("Dashboard");
            }

            bin.TempAssignedOfficerId = tempOfficerId;
            bin.TempAssignmentDate = DateTime.Now;
            bin.TempAssignmentReason = reason ?? "Temporary assignment";

            await _context.SaveChangesAsync();

            var officer = await _context.CollectionOfficers.FindAsync(tempOfficerId);
            TempData["SuccessMessage"] = $"✅ Officer '{officer?.FullName}' temporarily assigned to bin '{bin.BinName}'.";

            return RedirectToAction("Dashboard");
        }

        // ============================================================
        // POST: IoT/ResolveAlert (Admin only)
        // ============================================================
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> ResolveAlert(int alertId)
        {
            var alert = await _context.BinAlerts.FindAsync(alertId);
            if (alert != null)
            {
                alert.IsResolved = true;
                alert.ResolvedAt = DateTime.Now;
                alert.ResolvedBy = User.Identity.GetUserName();

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "✅ Alert resolved successfully!";
            }

            return RedirectToAction("Dashboard");
        }

        // ============================================================
        // POST: IoT/SimulateData (Admin only)
        // ============================================================
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> SimulateData()
        {
            await _iotService.SimulateBinData();
            TempData["SuccessMessage"] = "✅ IoT data simulated successfully!";
            return RedirectToAction("Dashboard");
        }

        // ============================================================
        // GET: IoT/RegisterBin (Admin only)
        // ============================================================
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> RegisterBin()
        {
            ViewBag.DropOffPoints = new SelectList(
                await _context.DropOffPoints.Where(d => d.IsActive).ToListAsync(),
                "DropOffPointId",
                "Name"
            );
            return View();
        }

        // ============================================================
        // POST: IoT/RegisterBin (Admin only)
        // ============================================================
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterBin(SmartBin model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedAt = DateTime.Now;
                model.IsActive = true;
                model.Status = "Online";
                model.FillLevel = 0;
                model.BatteryLevel = 100;

                _context.SmartBins.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"✅ Bin '{model.BinName}' registered successfully!";
                return RedirectToAction("Dashboard");
            }

            ViewBag.DropOffPoints = new SelectList(
                await _context.DropOffPoints.Where(d => d.IsActive).ToListAsync(),
                "DropOffPointId",
                "Name",
                model.DropOffPointId
            );
            return View(model);
        }

        // ============================================================
        // GET: IoT/ManageAlerts (Admin)
        // ============================================================
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> ManageAlerts()
        {
            var alerts = await _context.BinAlerts
                .Include(a => a.SmartBin)
                .Include(a => a.AssignedOfficer)
                .Where(a => !a.IsResolved)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            ViewBag.Officers = new SelectList(
                await _context.CollectionOfficers.ToListAsync(),
                "OfficerId",
                "FullName"
            );

            return View(alerts);
        }
    }
}