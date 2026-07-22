using BinIT2WinIT.Data;          // Required for ApplicationDbContext
using BinIT2WinIT.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BinIT2WinIT.Controllers
{
    [Authorize(Roles = "Resident")]
    public class ResidentController : Controller
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        // GET: Resident/Dashboard
        public async Task<ActionResult> Dashboard()
        {
            var userId = User.Identity.GetUserId();

            var resident = await _context.Residents
                .Include(r => r.Submissions)
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
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    CreatedAt = DateTime.Now,
                    ReferralCode = GenerateReferralCode()
                };
                _context.Residents.Add(resident);
                await _context.SaveChangesAsync();

                await AwardWelcomeBonus(resident.ResidentId);
            }

            return View(resident);
        }

        // GET: Resident/SubmitRecycling
        public async Task<ActionResult> SubmitRecycling()
        {
            var viewModel = new RecyclingSubmissionViewModel
            {
                MaterialTypes = await _context.MaterialTypes.Where(m => m.IsActive).ToListAsync(),
                DropOffPoints = await _context.DropOffPoints.Where(d => d.IsActive).ToListAsync()
            };
            return View(viewModel);
        }

        // POST: Resident/SubmitRecycling
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitRecycling(RecyclingSubmissionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
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

                TempData["SuccessMessage"] = $"Your recycling submission was successful! Estimated points: {estimatedPoints}";
                return RedirectToAction("Dashboard");
            }

            model.MaterialTypes = await _context.MaterialTypes.Where(m => m.IsActive).ToListAsync();
            model.DropOffPoints = await _context.DropOffPoints.Where(d => d.IsActive).ToListAsync();
            return View(model);
        }

        // GET: Resident/PointsHistory
        public async Task<ActionResult> PointsHistory()
        {
            var userId = User.Identity.GetUserId();
            var resident = await _context.Residents
                .Include(r => r.PointsTransactions)
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (resident == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(resident);
        }

        // GET: Resident/Leaderboard
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

        // GET: Resident/InfluencerPoints
        public async Task<ActionResult> InfluencerPoints()
        {
            var userId = User.Identity.GetUserId();
            var resident = await _context.Residents
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (resident == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(resident);
        }

        #region Helper Methods
        private string GenerateReferralCode()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var code = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
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
        #endregion
    }
}