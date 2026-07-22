using BinIT2WinIT.Models;
using BinIT2WinIT.Data;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BinIT2WinIT.Controllers
{
    [Authorize(Roles = "CollectionOfficer")]
    public class OfficerController : Controller
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        // GET: Officer/Dashboard
        public async Task<ActionResult> Dashboard()
        {
            var userId = User.Identity.GetUserId();

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

                // ✅ Get first active drop-off point
                var defaultDropOffPoint = await _context.DropOffPoints
                    .FirstOrDefaultAsync(d => d.IsActive);

                officer = new CollectionOfficer
                {
                    UserId = userId,
                    FullName = user.FullName ?? "Collection Officer",
                    PhoneNumber = user.PhoneNumber ?? "000-000-0000",
                    EmployeeNumber = GenerateEmployeeNumber(),
                    Department = "Collection",
                    DropOffPointId = defaultDropOffPoint?.DropOffPointId ?? 1,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _context.CollectionOfficers.Add(officer);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    var errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => $"Property: {x.PropertyName}, Error: {x.ErrorMessage}");
                    var fullErrorMessage = string.Join("; ", errorMessages);
                    System.Diagnostics.Debug.WriteLine($"Validation Error: {fullErrorMessage}");
                    throw;
                }

                // ✅ Reload officer with included navigation property
                officer = await _context.CollectionOfficers
                    .Include(o => o.AssignedDropOffPoint)
                    .FirstOrDefaultAsync(o => o.UserId == userId);
            }

            // ✅ Get statistics
            var pendingCount = await _context.RecyclingSubmissions
                .CountAsync(s => s.Status == "Pending");

            var verifiedToday = await _context.RecyclingSubmissions
                .CountAsync(s => s.Status == "Confirmed" &&
                    DbFunctions.TruncateTime(s.VerifiedDate) == DateTime.Today);

            // ✅ Calculate total points awarded today
            var pointsAwardedToday = await _context.PointsTransactions
                .Where(t => t.Type == "Earn" &&
                    DbFunctions.TruncateTime(t.TransactionDate) == DateTime.Today)
                .SumAsync(t => (int?)t.Amount) ?? 0;

            // ✅ Pass data to view
            ViewBag.PendingCount = pendingCount;
            ViewBag.VerifiedToday = verifiedToday;
            ViewBag.PointsAwardedToday = pointsAwardedToday;

            return View(officer);
        }

        // GET: Officer/Pending
        public async Task<ActionResult> Pending()
        {
            var userId = User.Identity.GetUserId();
            var officer = await _context.CollectionOfficers
                .Include(o => o.AssignedDropOffPoint)
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (officer == null)
            {
                TempData["ErrorMessage"] = "Officer profile not found.";
                return RedirectToAction("Dashboard", "Officer");
            }

            var submissions = await _context.RecyclingSubmissions
                .Include(s => s.Resident)
                .Include(s => s.MaterialType)
                .Include(s => s.DropOffPoint)
                .Where(s => s.Status == "Pending" && s.DropOffPointId == officer.DropOffPointId)
                .OrderBy(s => s.SubmissionDate)
                .ToListAsync();

            // ✅ Pass officer data for display
            ViewBag.OfficerName = officer.FullName;
            ViewBag.DropOffPointName = officer.AssignedDropOffPoint?.Name ?? "Not Assigned";

            return View(submissions);
        }

        // POST: Officer/Confirm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Confirm(int submissionId)
        {
            var userId = User.Identity.GetUserId();
            var officer = await _context.CollectionOfficers
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (officer == null)
            {
                TempData["ErrorMessage"] = "Officer profile not found.";
                return RedirectToAction("Dashboard");
            }

            var submission = await _context.RecyclingSubmissions
                .Include(s => s.Resident)
                .Include(s => s.MaterialType)
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

            if (submission == null)
            {
                TempData["ErrorMessage"] = "Submission not found.";
                return RedirectToAction("Pending");
            }

            // ✅ Get points rate
            var pointsRate = await _context.PointsRates
                .FirstOrDefaultAsync(p => p.MaterialTypeId == submission.MaterialTypeId && p.IsActive);

            var points = pointsRate != null ? (int)(submission.Weight * pointsRate.PointsPerKg) : 0;

            // ✅ Get CO2 factor
            var co2Factor = await _context.CO2Factors
                .FirstOrDefaultAsync(c => c.MaterialTypeId == submission.MaterialTypeId && c.IsActive);

            var co2Saved = co2Factor != null ? submission.Weight * co2Factor.CO2SavedPerKg : 0;

            // ✅ Update submission
            submission.Status = "Confirmed";
            submission.VerifiedBy = officer.OfficerId;
            submission.VerifiedDate = DateTime.Now;

            // ✅ Create points transaction
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

            // ✅ Update resident balance
            var resident = await _context.Residents
                .FirstOrDefaultAsync(r => r.ResidentId == submission.ResidentId);

            if (resident != null)
            {
                resident.PointsBalance += points;
                resident.TotalCO2Saved += co2Saved;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Submission confirmed! {resident?.FullName ?? "Resident"} awarded {points} points.";

            return RedirectToAction("Pending");
        }

        // POST: Officer/Reject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Reject(int submissionId, string reason)
        {
            var userId = User.Identity.GetUserId();
            var officer = await _context.CollectionOfficers
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (officer == null)
            {
                TempData["ErrorMessage"] = "Officer profile not found.";
                return RedirectToAction("Dashboard");
            }

            var submission = await _context.RecyclingSubmissions
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

            if (submission == null)
            {
                TempData["ErrorMessage"] = "Submission not found.";
                return RedirectToAction("Pending");
            }

            submission.Status = "Rejected";
            submission.VerifiedBy = officer.OfficerId;
            submission.VerifiedDate = DateTime.Now;
            submission.OfficerNotes = reason;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Submission rejected successfully.";

            return RedirectToAction("Pending");
        }

        #region Helper Methods
        private string GenerateEmployeeNumber()
        {
            var random = new Random();
            var prefix = "EMP";
            var number = random.Next(10000, 99999).ToString();
            return $"{prefix}{number}";
        }
        #endregion
    }
}