using BinIT2WinIT.Data;
using BinIT2WinIT.Models;
using BinIT2WinIT.Services;  // ✅ ADD THIS
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;  // ✅ ADD THIS
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BinIT2WinIT.Controllers
{
    [Authorize(Roles = "Resident")]
    public class ResidentController : Controller
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        // ✅ GET NOTIFICATION SERVICE
        private INotificationService NotificationService
        {
            get
            {
                return HttpContext.GetOwinContext().Get<INotificationService>();
            }
        }

        // ============================================================
        // GET: Resident/Dashboard
        // ============================================================
        public async Task<ActionResult> Dashboard()
        {
            var userId = User.Identity.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var resident = await _context.Residents
                .Include(r => r.Submissions)
                .Include(r => r.Submissions.Select(s => s.MaterialType))
                .Include(r => r.PointsTransactions)
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (resident == null)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                resident = new Resident
                {
                    UserId = userId,
                    FullName = user.FullName ?? user.UserName,
                    PhoneNumber = user.PhoneNumber ?? "",
                    CreatedAt = DateTime.Now,
                    IsActive = true,
                    ReferralCode = GenerateReferralCode()
                };
                _context.Residents.Add(resident);
                await _context.SaveChangesAsync();

                await AwardWelcomeBonus(resident.ResidentId);
            }

            // ✅ LOAD ANNOUNCEMENTS FOR RESIDENT
            var announcements = await _context.Announcements
                .Where(a => a.IsActive && (a.TargetAudience == "All" || a.TargetAudience == "Residents"))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            ViewBag.Announcements = announcements;

            // ✅ LOAD NOTIFICATIONS FOR RESIDENT
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .ToListAsync();

            ViewBag.Notifications = notifications;
            ViewBag.UnreadCount = notifications.Count();

            return View(resident);
        }

        // ============================================================
        // GET: Resident/SubmitRecycling
        // ============================================================
        public async Task<ActionResult> SubmitRecycling()
        {
            var viewModel = new RecyclingSubmissionViewModel
            {
                MaterialTypes = await _context.MaterialTypes.Where(m => m.IsActive).ToListAsync(),
                DropOffPoints = await _context.DropOffPoints.Where(d => d.IsActive).ToListAsync()
            };
            return View(viewModel);
        }

        // ============================================================
        // POST: Resident/SubmitRecycling
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitRecycling(RecyclingSubmissionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();

                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var resident = await _context.Residents.FirstOrDefaultAsync(r => r.UserId == userId);

                if (resident == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var submission = new RecyclingSubmission
                {
                    ResidentId = resident.ResidentId,
                    MaterialTypeId = model.MaterialTypeId,
                    DropOffPointId = model.DropOffPointId,
                    Weight = model.Weight,
                    SubmissionDate = DateTime.Now,
                    Status = "Pending"
                };

                _context.RecyclingSubmissions.Add(submission);
                await _context.SaveChangesAsync();

                var pointsRate = await _context.PointsRates
                    .FirstOrDefaultAsync(p => p.MaterialTypeId == model.MaterialTypeId && p.IsActive);

                var estimatedPoints = pointsRate != null ? (int)(model.Weight * pointsRate.PointsPerKg) : 0;

                TempData["SuccessMessage"] = $"✅ Your recycling submission was successful! Estimated points: {estimatedPoints}";
                return RedirectToAction("Dashboard");
            }

            model.MaterialTypes = await _context.MaterialTypes.Where(m => m.IsActive).ToListAsync();
            model.DropOffPoints = await _context.DropOffPoints.Where(d => d.IsActive).ToListAsync();
            return View(model);
        }

        // ============================================================
        // GET: Resident/PointsHistory
        // ============================================================
        public async Task<ActionResult> PointsHistory()
        {
            var userId = User.Identity.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var resident = await _context.Residents
                .Include(r => r.PointsTransactions)
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (resident == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(resident);
        }

        // ============================================================
        // GET: Resident/Leaderboard
        // ============================================================
        [Authorize(Roles = "Resident, CollectionOfficer")]
        public async Task<ActionResult> Leaderboard()
        {
            var topResidents = await _context.Residents
                .OrderByDescending(r => r.PointsBalance)
                .Take(10)
                .ToListAsync();

            var userId = User.Identity.GetUserId();
            var currentResident = await _context.Residents
                .FirstOrDefaultAsync(r => r.UserId == userId);

            ViewBag.CurrentUserId = userId;
            ViewBag.CurrentResident = currentResident;

            return View(topResidents);
        }

        // ============================================================
        // GET: Resident/InfluencerPoints
        // ============================================================
        public async Task<ActionResult> InfluencerPoints()
        {
            var userId = User.Identity.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var resident = await _context.Residents
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (resident == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(resident);
        }

        // ============================================================
        // GET: Resident/ViewProfile
        // ============================================================
        public async Task<ActionResult> ViewProfile()
        {
            var userId = User.Identity.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var resident = await _context.Residents
                .Include(r => r.User)
                .Include(r => r.Submissions)
                .Include(r => r.Submissions.Select(s => s.MaterialType))
                .Include(r => r.PointsTransactions)
                .Include(r => r.Community)
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (resident == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Calculate additional stats
            var totalSubmissions = resident.Submissions?.Count ?? 0;
            var verifiedSubmissions = resident.Submissions?.Count(s => s.Status == "Confirmed") ?? 0;
            var pendingSubmissions = resident.Submissions?.Count(s => s.Status == "Pending") ?? 0;
            var rejectedSubmissions = resident.Submissions?.Count(s => s.Status == "Rejected") ?? 0;
            var totalWeight = resident.Submissions?.Where(s => s.Status == "Confirmed").Sum(s => s.Weight) ?? 0;

            ViewBag.TotalSubmissions = totalSubmissions;
            ViewBag.VerifiedSubmissions = verifiedSubmissions;
            ViewBag.PendingSubmissions = pendingSubmissions;
            ViewBag.RejectedSubmissions = rejectedSubmissions;
            ViewBag.TotalWeight = totalWeight;

            // Calculate rank
            var rank = await _context.Residents
                .Where(r => r.PointsBalance > resident.PointsBalance)
                .CountAsync() + 1;

            ViewBag.Rank = rank;

            return View(resident);
        }

        // ============================================================
        // GET: Resident/EditProfile
        // ============================================================
        public async Task<ActionResult> EditProfile()
        {
            var userId = User.Identity.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var resident = await _context.Residents
                .Include(r => r.Community)
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (resident == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Communities = new SelectList(
                _context.DropOffPoints.Where(d => d.IsActive).ToList(),
                "DropOffPointId",
                "Name",
                resident.DropOffPointId
            );

            return View(resident);
        }

        // ============================================================
        // POST: Resident/EditProfile
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditProfile(Resident model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                var resident = await _context.Residents
                    .FirstOrDefaultAsync(r => r.UserId == userId);

                if (resident == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                resident.FullName = model.FullName;
                resident.PhoneNumber = model.PhoneNumber;
                resident.Address = model.Address;
                resident.Suburb = model.Suburb;
                resident.City = model.City;
                resident.Province = model.Province;
                resident.PostalCode = model.PostalCode;
                resident.DropOffPointId = model.DropOffPointId;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "✅ Profile updated successfully!";
                return RedirectToAction("ViewProfile");
            }

            ViewBag.Communities = new SelectList(
                _context.DropOffPoints.Where(d => d.IsActive).ToList(),
                "DropOffPointId",
                "Name",
                model.DropOffPointId
            );

            return View(model);
        }

        // ============================================================
        // POST: Resident/MarkNotificationRead
        // ============================================================
        [HttpPost]
        public async Task<ActionResult> MarkNotificationRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // ============================================================
        // POST: Resident/MarkAllRead
        // ============================================================
        [HttpPost]
        public async Task<ActionResult> MarkAllRead()
        {
            var userId = User.Identity.GetUserId();
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in notifications)
            {
                n.IsRead = true;
                n.ReadAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // ============================================================
        // GET: Resident/Notifications
        // ============================================================
        public async Task<ActionResult> Notifications()
        {
            var userId = User.Identity.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notifications);
        }

        // ============================================================
        // Helper Methods
        // ============================================================
        private string GenerateReferralCode()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            string code;

            do
            {
                code = new string(Enumerable.Repeat(chars, 8)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            }
            while (_context.Residents.Any(r => r.ReferralCode == code));

            return code;
        }

        private async Task AwardWelcomeBonus(int residentId)
        {
            var config = await _context.SystemConfigurations
                .FirstOrDefaultAsync(c => c.ConfigKey == "WelcomeBonusPoints");

            var bonusPoints = config != null ? int.Parse(config.ConfigValue) : 100;

            var transaction = new PointsTransaction
            {
                ResidentId = residentId,
                Amount = bonusPoints,
                Description = "Welcome Bonus - Thank you for joining!",
                Type = "WelcomeBonus",
                TransactionDate = DateTime.Now
            };

            _context.PointsTransactions.Add(transaction);

            var resident = await _context.Residents.FirstOrDefaultAsync(r => r.ResidentId == residentId);

            if (resident != null)
            {
                resident.PointsBalance += bonusPoints;
            }

            await _context.SaveChangesAsync();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}