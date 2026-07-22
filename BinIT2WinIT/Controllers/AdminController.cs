using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using BinIT2WinIT.Data;
using BinIT2WinIT.Models;
using BinIT2WinIT.App_Start;
using System.Web;

namespace BinIT2WinIT.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        // ============================================================
        // GET: Admin/Dashboard
        // ============================================================
        public async Task<ActionResult> Dashboard()
        {
            var userId = User.Identity.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // ✅ FIXED: Use Find (synchronous) instead of FindAsync
            var user = _context.Users.Find(userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var admin = await _context.Administrators
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (admin == null)
            {
                admin = new Administrator
                {
                    UserId = userId,
                    FullName = user.FullName ?? user.UserName,
                    Email = user.Email,
                    Department = "System Administration",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _context.Administrators.Add(admin);
                await _context.SaveChangesAsync();
            }

            // Get statistics
            ViewBag.TotalResidents = await _context.Residents.CountAsync();
            ViewBag.TotalSubmissions = await _context.RecyclingSubmissions.CountAsync();
            ViewBag.TotalPoints = await _context.PointsTransactions.SumAsync(t => (int?)t.Amount) ?? 0;
            ViewBag.PendingSubmissions = await _context.RecyclingSubmissions
                .Where(s => s.Status == "Pending").CountAsync();

            return View(admin);
        }

        // ============================================================
        // GET: Admin/Users
        // ============================================================
        public async Task<ActionResult> Users()
        {
            var users = await _context.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            return View(users);
        }

        // ============================================================
        // POST: Admin/DeactivateUser
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeactivateUser(string userId)
        {
            // ✅ FIXED: Use Find (synchronous) instead of FindAsync
            var user = _context.Users.Find(userId);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Users");
            }

            user.IsActive = false;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"✅ User '{user.Email}' has been deactivated.";
            return RedirectToAction("Users");
        }

        // ============================================================
        // POST: Admin/ActivateUser
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ActivateUser(string userId)
        {
            // ✅ FIXED: Use Find (synchronous) instead of FindAsync
            var user = _context.Users.Find(userId);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Users");
            }

            user.IsActive = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"✅ User '{user.Email}' has been activated.";
            return RedirectToAction("Users");
        }

        // ============================================================
        // GET: Admin/Communities
        // ============================================================
        public async Task<ActionResult> Communities()
        {
            var communities = await _context.DropOffPoints
                .Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .ToListAsync();

            var officerCounts = new Dictionary<int, int>();
            var residentCounts = new Dictionary<int, int>();
            var submissionCounts = new Dictionary<int, int>();
            var pointsCounts = new Dictionary<int, int>();

            foreach (var community in communities)
            {
                officerCounts[community.DropOffPointId] = await _context.CollectionOfficers
                    .Where(o => o.DropOffPointId == community.DropOffPointId)
                    .CountAsync();

                residentCounts[community.DropOffPointId] = 0;

                submissionCounts[community.DropOffPointId] = await _context.RecyclingSubmissions
                    .Where(s => s.DropOffPointId == community.DropOffPointId && s.Status == "Confirmed")
                    .CountAsync();

                pointsCounts[community.DropOffPointId] = await _context.RecyclingSubmissions
                    .Where(s => s.DropOffPointId == community.DropOffPointId && s.Status == "Confirmed")
                    .SumAsync(s => (int?)s.Weight) ?? 0;
            }

            ViewBag.OfficerCounts = officerCounts;
            ViewBag.ResidentCounts = residentCounts;
            ViewBag.SubmissionCounts = submissionCounts;
            ViewBag.PointsCounts = pointsCounts;

            return View(communities);
        }

        // ============================================================
        // GET: Admin/OfficerDeployment
        // ============================================================
        public async Task<ActionResult> OfficerDeployment()
        {
            var regions = await _context.DropOffPoints
                .Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .ToListAsync();

            var officerCounts = new Dictionary<int, int>();
            foreach (var region in regions)
            {
                officerCounts[region.DropOffPointId] = await _context.CollectionOfficers
                    .Where(o => o.DropOffPointId == region.DropOffPointId)
                    .CountAsync();
            }

            ViewBag.OfficerCounts = officerCounts;
            ViewBag.TotalOfficers = await _context.CollectionOfficers.CountAsync();
            ViewBag.TotalRegions = regions.Count;
            ViewBag.UnassignedOfficers = await _context.CollectionOfficers
                .Where(o => o.DropOffPointId == null)
                .CountAsync();

            return View(regions);
        }

        // ============================================================
        // GET: Admin/CommunityLeaderboard
        // ============================================================
        public async Task<ActionResult> CommunityLeaderboard()
        {
            var communities = await _context.DropOffPoints
                .Where(d => d.IsActive)
                .ToListAsync();

            var totalWeight = new Dictionary<int, double>();
            var totalCO2 = new Dictionary<int, double>();
            var residentCounts = new Dictionary<int, int>();

            foreach (var community in communities)
            {
                var submissions = await _context.RecyclingSubmissions
                    .Where(s => s.DropOffPointId == community.DropOffPointId && s.Status == "Confirmed")
                    .ToListAsync();

                totalWeight[community.DropOffPointId] = submissions.Sum(s => s.Weight);
                totalCO2[community.DropOffPointId] = submissions.Sum(s => s.Weight * 1.0);
                residentCounts[community.DropOffPointId] = 0;
            }

            ViewBag.TotalWeight = totalWeight;
            ViewBag.TotalCO2 = totalCO2;
            ViewBag.ResidentCounts = residentCounts;

            return View(communities);
        }

        // ============================================================
        // GET: Admin/CommunityDetails
        // ============================================================
        public async Task<ActionResult> CommunityDetails(int id)
        {
            var community = await _context.DropOffPoints
                .FirstOrDefaultAsync(d => d.DropOffPointId == id);

            if (community == null)
            {
                TempData["ErrorMessage"] = "Community not found.";
                return RedirectToAction("Communities");
            }

            var officers = await _context.CollectionOfficers
                .Where(o => o.DropOffPointId == id)
                .ToListAsync();

            var submissions = await _context.RecyclingSubmissions
                .Where(s => s.DropOffPointId == id && s.Status == "Confirmed")
                .ToListAsync();

            ViewBag.Officers = officers;
            ViewBag.Submissions = submissions;
            ViewBag.TotalWeight = submissions.Sum(s => s.Weight);
            ViewBag.TotalCO2 = submissions.Sum(s => s.Weight * 1.0);
            ViewBag.OfficerCount = officers.Count;
            ViewBag.SubmissionCount = submissions.Count;

            return View(community);
        }

        // ============================================================
        // GET: Admin/Officers
        // ============================================================
        public async Task<ActionResult> Officers()
        {
            var officers = await _context.CollectionOfficers
                .Include(o => o.User)
                .Include(o => o.AssignedDropOffPoint)
                .ToListAsync();

            return View(officers);
        }

        // ============================================================
        // GET: Admin/AssignOfficer
        // ============================================================
        public async Task<ActionResult> AssignOfficer(int id)
        {
            var officer = await _context.CollectionOfficers
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OfficerId == id);

            if (officer == null)
            {
                TempData["ErrorMessage"] = "Officer not found.";
                return RedirectToAction("Officers");
            }

            var viewModel = new AssignOfficerViewModel
            {
                OfficerId = officer.OfficerId,
                OfficerName = officer.FullName,
                DropOffPointId = officer.DropOffPointId,
                DropOffPoints = new SelectList(
                    await _context.DropOffPoints.Where(d => d.IsActive).ToListAsync(),
                    "DropOffPointId",
                    "Name",
                    officer.DropOffPointId
                )
            };

            return View(viewModel);
        }

        // ============================================================
        // POST: Admin/AssignOfficer
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AssignOfficer(AssignOfficerViewModel model)
        {
            if (ModelState.IsValid)
            {
                var officer = await _context.CollectionOfficers
                    .FindAsync(model.OfficerId);

                if (officer == null)
                {
                    TempData["ErrorMessage"] = "Officer not found.";
                    return RedirectToAction("Officers");
                }

                officer.DropOffPointId = model.DropOffPointId;
                await _context.SaveChangesAsync();

                var pointName = model.DropOffPointId.HasValue
                    ? (await _context.DropOffPoints.FindAsync(model.DropOffPointId.Value))?.Name
                    : "No Region Assigned";

                TempData["SuccessMessage"] = $"✅ Officer '{officer.FullName}' assigned to '{pointName}' successfully!";
                return RedirectToAction("Officers");
            }

            model.DropOffPoints = new SelectList(
                await _context.DropOffPoints.Where(d => d.IsActive).ToListAsync(),
                "DropOffPointId",
                "Name",
                model.DropOffPointId
            );

            return View(model);
        }

        // ============================================================
        // GET: Admin/DropOffPoints
        // ============================================================
        public async Task<ActionResult> DropOffPoints()
        {
            var points = await _context.DropOffPoints
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(points);
        }

        // ============================================================
        // POST: Admin/CreateDropOffPoint
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateDropOffPoint(DropOffPoint model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedAt = DateTime.Now;
                model.IsActive = true;

                _context.DropOffPoints.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"✅ Drop-Off Point '{model.Name}' created successfully!";
                return RedirectToAction("DropOffPoints");
            }

            return View(model);
        }

        // ============================================================
        // POST: Admin/DeleteDropOffPoint
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteDropOffPoint(int id)
        {
            // ✅ FIXED: Use Find (synchronous) instead of FindAsync
            var point = _context.DropOffPoints.Find(id);

            if (point == null)
            {
                TempData["ErrorMessage"] = "Drop-Off Point not found.";
                return RedirectToAction("DropOffPoints");
            }

            point.IsActive = false;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"✅ Drop-Off Point '{point.Name}' deactivated successfully!";
            return RedirectToAction("DropOffPoints");
        }

        // ============================================================
        // GET: Admin/CreateOfficer
        // ============================================================
        public async Task<ActionResult> CreateOfficer()
        {
            ViewBag.DropOffPoints = new SelectList(
                await _context.DropOffPoints.Where(d => d.IsActive).ToListAsync(),
                "DropOffPointId",
                "Name"
            );

            return View();
        }

        // ============================================================
        // POST: Admin/CreateOfficer
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateOfficer(CreateOfficerViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var existingUser = await userManager.FindByEmailAsync(model.Email);

                if (existingUser != null)
                {
                    ModelState.AddModelError("", "A user with this email already exists.");
                    ViewBag.DropOffPoints = new SelectList(
                        await _context.DropOffPoints.Where(d => d.IsActive).ToListAsync(),
                        "DropOffPointId",
                        "Name",
                        model.DropOffPointId
                    );
                    return View(model);
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user.Id, "CollectionOfficer");

                    var officer = new CollectionOfficer
                    {
                        UserId = user.Id,
                        FullName = model.FullName,
                        PhoneNumber = model.PhoneNumber ?? "",
                        EmployeeNumber = GenerateEmployeeNumber(),
                        Department = model.Department ?? "Waste Management",
                        DropOffPointId = model.DropOffPointId,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };

                    _context.CollectionOfficers.Add(officer);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"✅ Officer '{model.FullName}' created successfully!";
                    return RedirectToAction("Officers");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
            }

            ViewBag.DropOffPoints = new SelectList(
                await _context.DropOffPoints.Where(d => d.IsActive).ToListAsync(),
                "DropOffPointId",
                "Name",
                model?.DropOffPointId
            );

            return View(model);
        }

        // ============================================================
        // Helper: Generate Employee Number
        // ============================================================
        private string GenerateEmployeeNumber()
        {
            var random = new Random();
            return "EMP" + random.Next(10000, 99999).ToString();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        // ============================================================
        // GET: Admin/PointsRates
        // ============================================================
        public async Task<ActionResult> PointsRates()
        {
            var rates = await _context.PointsRates
                .Include(p => p.MaterialType)
                .OrderByDescending(p => p.IsActive)
                .ToListAsync();

            return View(rates);
        }

        // ============================================================
        // GET: Admin/CreatePointsRate
        // ============================================================
        public async Task<ActionResult> CreatePointsRate()
        {
            ViewBag.MaterialTypes = new SelectList(
                await _context.MaterialTypes.Where(m => m.IsActive).ToListAsync(),
                "MaterialTypeId",
                "Name"
            );

            return View();
        }

        // ============================================================
        // POST: Admin/CreatePointsRate
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreatePointsRate(PointsRate model)
        {
            if (ModelState.IsValid)
            {
                // Deactivate any existing active rate for this material
                var existingActive = await _context.PointsRates
                    .FirstOrDefaultAsync(p => p.MaterialTypeId == model.MaterialTypeId && p.IsActive);

                if (existingActive != null)
                {
                    existingActive.IsActive = false;
                    existingActive.EndDate = DateTime.Now;
                }

                model.IsActive = true;
                model.EffectiveDate = model.EffectiveDate == DateTime.MinValue ? DateTime.Now : model.EffectiveDate;

                _context.PointsRates.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "✅ Points rate created successfully!";
                return RedirectToAction("PointsRates");
            }

            ViewBag.MaterialTypes = new SelectList(
                await _context.MaterialTypes.Where(m => m.IsActive).ToListAsync(),
                "MaterialTypeId",
                "Name"
            );

            return View(model);
        }

        // ============================================================
        // GET: Admin/DeactivateRate (Matches your view)
        // ============================================================
        public async Task<ActionResult> DeactivateRate(int id)
        {
            var rate = await _context.PointsRates.FindAsync(id);

            if (rate == null)
            {
                TempData["ErrorMessage"] = "Points rate not found.";
                return RedirectToAction("PointsRates");
            }

            rate.IsActive = false;
            rate.EndDate = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "✅ Points rate deactivated successfully!";
            return RedirectToAction("PointsRates");
        }
    }
}