using BinIT2WinIT.Data;
using BinIT2WinIT.Models;
using BinIT2WinIT.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
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

        // ============================================================
        // GET NOTIFICATION SERVICE
        // ============================================================
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

            // Load announcements for resident
            var announcements = await _context.Announcements
                .Where(a => a.IsActive && (a.TargetAudience == "All" || a.TargetAudience == "Residents"))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            ViewBag.Announcements = announcements;

            // Load notifications for resident
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .ToListAsync();

            ViewBag.Notifications = notifications;
            ViewBag.UnreadCount = notifications.Count();

            // Get redemption stats
            var pendingRedemptions = await _context.RedemptionRequests
                .Where(r => r.ResidentId == resident.ResidentId && r.RequestStatus == "Pending")
                .CountAsync();

            var approvedRedemptions = await _context.RedemptionRequests
                .Where(r => r.ResidentId == resident.ResidentId && r.RequestStatus == "Approved")
                .CountAsync();

            ViewBag.PendingRedemptions = pendingRedemptions;
            ViewBag.ApprovedRedemptions = approvedRedemptions;

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
        // GET: Resident/RedeemPoints
        // ============================================================
        public async Task<ActionResult> RedeemPoints()
        {
            var userId = User.Identity.GetUserId();
            var resident = await _context.Residents
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (resident == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get redemption options
            var options = await _context.RedemptionOptions
                .Where(o => o.IsActive && (o.ExpiryDate == null || o.ExpiryDate > DateTime.Now))
                .ToListAsync();

            // Check if any options exist
            if (!options.Any())
            {
                ViewBag.NoOptions = true;
                ViewBag.Message = "No redemption options are currently available. Please check back later.";
            }

            var viewModel = new RedeemPointsViewModel
            {
                ResidentId = resident.ResidentId,
                ResidentName = resident.FullName,
                PointsBalance = resident.PointsBalance,
                RedemptionOptions = options
            };

            return View(viewModel);
        }

        // ============================================================
        // POST: Resident/RedeemPoints
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RedeemPoints(RedeemPointsViewModel model)
        {
            var userId = User.Identity.GetUserId();
            var resident = await _context.Residents
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (resident == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                model.PointsBalance = resident.PointsBalance;
                model.RedemptionOptions = await _context.RedemptionOptions
                    .Where(o => o.IsActive)
                    .ToListAsync();
                return View(model);
            }

            var selectedOption = await _context.RedemptionOptions
                .FirstOrDefaultAsync(o => o.OptionId == model.SelectedOptionId && o.IsActive);

            if (selectedOption == null)
            {
                ModelState.AddModelError("", "Invalid redemption option selected.");
                model.PointsBalance = resident.PointsBalance;
                model.RedemptionOptions = await _context.RedemptionOptions
                    .Where(o => o.IsActive)
                    .ToListAsync();
                return View(model);
            }

            // Check if resident has enough points
            if (resident.PointsBalance < selectedOption.PointsRequired)
            {
                ModelState.AddModelError("", $"You need {selectedOption.PointsRequired} points to redeem this option. You have {resident.PointsBalance} points.");
                model.PointsBalance = resident.PointsBalance;
                model.RedemptionOptions = await _context.RedemptionOptions
                    .Where(o => o.IsActive)
                    .ToListAsync();
                return View(model);
            }

            // Check if user has provided utility account number
            if (string.IsNullOrEmpty(model.UtilityAccountNumber))
            {
                ModelState.AddModelError("UtilityAccountNumber", "Please enter your municipal utility account number.");
                model.PointsBalance = resident.PointsBalance;
                model.RedemptionOptions = await _context.RedemptionOptions
                    .Where(o => o.IsActive)
                    .ToListAsync();
                return View(model);
            }

            // Create redemption request
            var request = new RedemptionRequest
            {
                ResidentId = resident.ResidentId,
                OptionId = selectedOption.OptionId,
                PointsUsed = selectedOption.PointsRequired,
                DiscountAmount = selectedOption.DiscountAmount,
                UtilityType = selectedOption.UtilityType,
                RequestStatus = "Pending",
                RequestDate = DateTime.Now,
                ReferenceNumber = GenerateReferenceNumber(),
                UtilityAccountNumber = model.UtilityAccountNumber
            };

            _context.RedemptionRequests.Add(request);

            // Deduct points immediately
            resident.PointsBalance -= selectedOption.PointsRequired;

            // Create points transaction for redemption
            var transaction = new PointsTransaction
            {
                ResidentId = resident.ResidentId,
                Amount = -selectedOption.PointsRequired,
                Description = $"Redeemed {selectedOption.PointsRequired} points for {selectedOption.UtilityType} discount (R{selectedOption.DiscountAmount})",
                Type = "Redeem",
                TransactionDate = DateTime.Now,
                ReferenceId = request.RequestId,
                Reason = "Utility discount redemption"
            };
            _context.PointsTransactions.Add(transaction);

            await _context.SaveChangesAsync();

            // ✅ FIXED: Safe notification service call with null checks and fallback
            try
            {
                var notificationService = NotificationService;
                if (notificationService != null && !string.IsNullOrEmpty(request.ReferenceNumber))
                {
                    await notificationService.SendNotification(
                        userId,
                        "Redemption Request Submitted",
                        $"Your request to redeem {selectedOption.PointsRequired} points for a R{selectedOption.DiscountAmount} {selectedOption.UtilityType} discount has been submitted. Reference: {request.ReferenceNumber}",
                        "Redemption",
                        request.ReferenceNumber
                    );
                }
                else
                {
                    // Log to debug output if service is missing (won't crash app)
                    System.Diagnostics.Debug.WriteLine($"NotificationService is null or ReferenceNumber is empty. Notification skipped for User: {userId}");
                }
            }
            catch (Exception ex)
            {
                // Log error but don't break the flow
                System.Diagnostics.Debug.WriteLine($"Notification failed: {ex.Message}");
            }

            TempData["SuccessMessage"] = $"✅ Successfully redeemed {selectedOption.PointsRequired} points for R{selectedOption.DiscountAmount} {selectedOption.UtilityType} discount! Reference: {request.ReferenceNumber}";
            TempData["ReferenceNumber"] = request.ReferenceNumber;

            return RedirectToAction("RedeemConfirmation", new { id = request.RequestId });
        }

        // ============================================================
        // GET: Resident/RedeemConfirmation
        // ============================================================
        public async Task<ActionResult> RedeemConfirmation(int id)
        {
            var userId = User.Identity.GetUserId();
            var resident = await _context.Residents
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (resident == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var request = await _context.RedemptionRequests
                .Include(r => r.RedemptionOption)
                .FirstOrDefaultAsync(r => r.RequestId == id && r.ResidentId == resident.ResidentId);

            if (request == null)
            {
                return HttpNotFound();
            }

            return View(request);
        }

        // ============================================================
        // GET: Resident/RedeemHistory
        // ============================================================
        public async Task<ActionResult> RedeemHistory()
        {
            var userId = User.Identity.GetUserId();
            var resident = await _context.Residents
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (resident == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var requests = await _context.RedemptionRequests
                .Include(r => r.RedemptionOption)
                .Where(r => r.ResidentId == resident.ResidentId)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            var viewModel = new RedemptionHistoryViewModel
            {
                Requests = requests,
                TotalPointsRedeemed = requests.Sum(r => r.PointsUsed),
                TotalDiscountsReceived = requests.Where(r => r.RequestStatus == "Approved" || r.RequestStatus == "Applied").Sum(r => r.DiscountAmount)
            };

            return View(viewModel);
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

        private string GenerateReferenceNumber()
        {
            var random = new Random();
            var prefix = "RED";
            var number = random.Next(10000, 99999).ToString();
            var suffix = DateTime.Now.ToString("yyMMdd");
            return $"{prefix}{number}{suffix}";
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