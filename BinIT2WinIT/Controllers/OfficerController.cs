using System;
using System.Linq;
using BinIT2WinIT.Models;
using global::SmartRecycling.Data;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
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

                var pendingSubmissions = await _context.RecyclingSubmissions
                    .Where(s => s.Status == "Pending")
                    .ToListAsync();

                var verifiedToday = await _context.RecyclingSubmissions
                    .Where(s => s.VerifiedDate != null && DbFunctions.TruncateTime(s.VerifiedDate) == DateTime.Today)
                    .ToListAsync();

                ViewBag.PendingCount = pendingSubmissions.Count;
                ViewBag.VerifiedToday = verifiedToday.Count;

                return View(officer);
            }

            // GET: Officer/Pending
            public async Task<ActionResult> Pending()
            {
                var submissions = await _context.RecyclingSubmissions
                    .Include(s => s.Resident)
                    .Include(s => s.MaterialType)
                    .Include(s => s.DropOffPoint)
                    .Where(s => s.Status == "Pending")
                    .OrderBy(s => s.SubmissionDate)
                    .ToListAsync();

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

                var submission = await _context.RecyclingSubmissions
                    .Include(s => s.Resident)
                    .Include(s => s.MaterialType)
                    .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

                if (submission == null)
                {
                    return HttpNotFound();
                }

                // Get points rate
                var pointsRate = await _context.PointsRates
                    .FirstOrDefaultAsync(p => p.MaterialTypeId == submission.MaterialTypeId && p.IsActive);

                var points = pointsRate != null ? (int)(submission.Weight * pointsRate.PointsPerKg) : 0;

                // Update submission
                submission.Status = "Confirmed";
                submission.VerifiedBy = officer.OfficerId;
                submission.VerifiedDate = DateTime.Now;

                // Create points transaction
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

                // Update resident balance
                var resident = await _context.Residents.FindAsync(submission.ResidentId);
                resident.PointsBalance += points;
                resident.TotalCO2Saved += submission.Weight * 1.0; // Placeholder for CO2 factor

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Submission confirmed! {resident.FullName} awarded {points} points.";
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

                var submission = await _context.RecyclingSubmissions
                    .FindAsync(submissionId);

                if (submission == null)
                {
                    return HttpNotFound();
                }

                submission.Status = "Rejected";
                submission.VerifiedBy = officer.OfficerId;
                submission.VerifiedDate = DateTime.Now;
                submission.OfficerNotes = reason;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Submission rejected.";
                return RedirectToAction("Pending");
            }
        }
    }