using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using BinIT2WinIT.Data;
using BinIT2WinIT.Models;

namespace BinIT2WinIT.Controllers
{
    [Authorize(Roles = "CollectionOfficer")]
    public class OfficerController : Controller
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        // ============================================================
        // GET: Officer/Dashboard
        // ============================================================
        public async Task<ActionResult> Dashboard()
        {
            var userId = User.Identity.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // FIND OFFICER OR CREATE ONE
            var officer = await _context.CollectionOfficers
                .Include(o => o.AssignedDropOffPoint)
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (officer == null)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // ✅ CREATE OFFICER WITH ALL REQUIRED FIELDS
                officer = new CollectionOfficer
                {
                    UserId = userId,
                    FullName = user.FullName ?? user.UserName ?? "Unknown User",
                    PhoneNumber = user.PhoneNumber ?? "000-000-0000",
                    EmployeeNumber = GenerateEmployeeNumber(),
                    Department = "Waste Management",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _context.CollectionOfficers.Add(officer);

                try
                {
                    await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine("✅ Officer created successfully!");
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    // ✅ LOG DETAILED ERRORS
                    var errors = new List<string>();
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            errors.Add($"Property: '{validationError.PropertyName}' - Error: {validationError.ErrorMessage}");
                        }
                    }

                    System.Diagnostics.Debug.WriteLine("❌ Validation Errors:");
                    foreach (var error in errors)
                    {
                        System.Diagnostics.Debug.WriteLine(error);
                    }

                    TempData["ErrorMessage"] = "Failed to create officer profile: " + string.Join("; ", errors);
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error creating officer: {ex.Message}");
                    TempData["ErrorMessage"] = "An error occurred while creating your profile. Please contact administrator.";
                    return RedirectToAction("Index", "Home");
                }
            }

            // ✅ COUNT PENDING SUBMISSIONS
            var pendingCount = await _context.RecyclingSubmissions
                .Where(s => s.Status == "Pending")
                .CountAsync();

            var verifiedToday = await _context.RecyclingSubmissions
                .Where(s => s.Status == "Confirmed" && s.VerifiedDate != null && DbFunctions.TruncateTime(s.VerifiedDate) == DateTime.Today)
                .CountAsync();

            ViewBag.PendingCount = pendingCount;
            ViewBag.VerifiedToday = verifiedToday;

            return View(officer);
        }

        // ============================================================
        // GET: Officer/Pending
        // ============================================================
        // GET: Officer/Pending
        public async Task<ActionResult> Pending()
        {
            var submissions = await _context.RecyclingSubmissions
                .Include(s => s.Resident)          // ← IMPORTANT!
                .Include(s => s.MaterialType)      // ← IMPORTANT!
                .Include(s => s.DropOffPoint)      // ← IMPORTANT!
                .Where(s => s.Status == "Pending")
                .OrderBy(s => s.SubmissionDate)
                .ToListAsync();

            return View(submissions);
        }

        // ============================================================
        // POST: Officer/Confirm
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Confirm(int submissionId)
        {
            var userId = User.Identity.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Please log in first.";
                return RedirectToAction("Login", "Account");
            }

            // ✅ GET OFFICER
            var officer = await _context.CollectionOfficers
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (officer == null)
            {
                TempData["ErrorMessage"] = "Officer profile not found. Please contact administrator.";
                return RedirectToAction("Pending");
            }

            // ✅ GET SUBMISSION
            var submission = await _context.RecyclingSubmissions
                .Include(s => s.Resident)
                .Include(s => s.MaterialType)
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

            if (submission == null)
            {
                TempData["ErrorMessage"] = "Submission not found.";
                return RedirectToAction("Pending");
            }

            // ✅ CHECK IF ALREADY VERIFIED
            if (submission.Status != "Pending")
            {
                TempData["ErrorMessage"] = $"This submission has already been {submission.Status.ToLower()}.";
                return RedirectToAction("Pending");
            }

            // ✅ GET POINTS RATE
            var pointsRate = await _context.PointsRates
                .FirstOrDefaultAsync(p => p.MaterialTypeId == submission.MaterialTypeId && p.IsActive);

            var points = pointsRate != null ? (int)(submission.Weight * pointsRate.PointsPerKg) : 0;

            // ✅ UPDATE SUBMISSION
            submission.Status = "Confirmed";
            submission.VerifiedBy = officer.OfficerId;
            submission.VerifiedDate = DateTime.Now;

            // ✅ CREATE POINTS TRANSACTION
            var transaction = new PointsTransaction
            {
                ResidentId = submission.ResidentId,
                Amount = points,
                Description = $"Recycling verified: {submission.Weight}kg of {submission.MaterialType.Name}",
                Type = "Earn",
                ReferenceId = submissionId,
                TransactionDate = DateTime.Now
            };
            _context.PointsTransactions.Add(transaction);

            // ✅ UPDATE RESIDENT BALANCE
            var resident = await _context.Residents.FindAsync(submission.ResidentId);
            if (resident != null)
            {
                resident.PointsBalance += points;
                // TODO: Calculate CO2 based on material type
                resident.TotalCO2Saved += submission.Weight * 1.0;
            }

            // ✅ SAVE WITH ERROR HANDLING
            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"✅ Submission confirmed! {resident?.FullName ?? "Resident"} awarded {points} points.";
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var errors = new List<string>();
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        errors.Add($"Property: '{validationError.PropertyName}' - Error: {validationError.ErrorMessage}");
                    }
                }
                TempData["ErrorMessage"] = "Validation failed: " + string.Join("; ", errors);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }

            return RedirectToAction("Pending");
        }

        // ============================================================
        // POST: Officer/Reject
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Reject(int submissionId, string reason)
        {
            var userId = User.Identity.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Please log in first.";
                return RedirectToAction("Login", "Account");
            }

            // ✅ GET OFFICER
            var officer = await _context.CollectionOfficers
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (officer == null)
            {
                TempData["ErrorMessage"] = "Officer profile not found. Please contact administrator.";
                return RedirectToAction("Pending");
            }

            // ✅ GET SUBMISSION
            var submission = await _context.RecyclingSubmissions
                .FindAsync(submissionId);

            if (submission == null)
            {
                TempData["ErrorMessage"] = "Submission not found.";
                return RedirectToAction("Pending");
            }

            // ✅ CHECK IF ALREADY VERIFIED
            if (submission.Status != "Pending")
            {
                TempData["ErrorMessage"] = $"This submission has already been {submission.Status.ToLower()}.";
                return RedirectToAction("Pending");
            }

            // ✅ UPDATE SUBMISSION
            submission.Status = "Rejected";
            submission.VerifiedBy = officer.OfficerId;
            submission.VerifiedDate = DateTime.Now;
            submission.OfficerNotes = string.IsNullOrEmpty(reason) ? "No reason provided" : reason;

            // ✅ SAVE WITH ERROR HANDLING
            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "❌ Submission rejected successfully.";
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var errors = new List<string>();
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        errors.Add($"Property: '{validationError.PropertyName}' - Error: {validationError.ErrorMessage}");
                    }
                }
                TempData["ErrorMessage"] = "Validation failed: " + string.Join("; ", errors);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }

            return RedirectToAction("Pending");
        }

        // ============================================================
        // GET: Officer/Statistics
        // ============================================================
        public async Task<ActionResult> Statistics()
        {
            var userId = User.Identity.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var officer = await _context.CollectionOfficers
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (officer == null)
            {
                TempData["ErrorMessage"] = "Officer profile not found.";
                return RedirectToAction("Dashboard");
            }

            // ✅ GET STATISTICS
            var totalVerified = await _context.RecyclingSubmissions
                .Where(s => s.VerifiedBy == officer.OfficerId && s.Status == "Confirmed")
                .CountAsync();

            var totalRejected = await _context.RecyclingSubmissions
                .Where(s => s.VerifiedBy == officer.OfficerId && s.Status == "Rejected")
                .CountAsync();

            var totalWeightVerified = await _context.RecyclingSubmissions
                .Where(s => s.VerifiedBy == officer.OfficerId && s.Status == "Confirmed")
                .SumAsync(s => (double?)s.Weight) ?? 0;

            ViewBag.TotalVerified = totalVerified;
            ViewBag.TotalRejected = totalRejected;
            ViewBag.TotalWeightVerified = totalWeightVerified;

            return View(officer);
        }

        // ============================================================
        // GET: Officer/Leaderboard (Redirect to Resident Leaderboard)
        // ============================================================
        public ActionResult Leaderboard()
        {
            return RedirectToAction("Leaderboard", "Resident");
        }

        // ============================================================
        // Helper Methods
        // ============================================================
        private string GenerateEmployeeNumber()
        {
            var random = new Random();
            var number = random.Next(10000, 99999).ToString();
            return "EMP" + number;
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