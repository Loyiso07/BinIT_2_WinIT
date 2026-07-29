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
using BinIT2WinIT.Services;
using System.Web;

namespace BinIT2WinIT.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        // GET NOTIFICATION SERVICE
        private INotificationService NotificationService
        {
            get
            {
                return HttpContext.GetOwinContext().Get<INotificationService>();
            }
        }

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

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

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
                    CreatedAt = DateTime.Now,
                    CreatedByUserId = userId
                };

                _context.Administrators.Add(admin);
                await _context.SaveChangesAsync();
            }

            ViewBag.TotalResidents = await _context.Residents.CountAsync();
            ViewBag.TotalSubmissions = await _context.RecyclingSubmissions.CountAsync();
            ViewBag.TotalPoints = await _context.PointsTransactions.SumAsync(t => (int?)t.Amount) ?? 0;
            ViewBag.PendingSubmissions = await _context.RecyclingSubmissions
                .Where(s => s.Status == "Pending").CountAsync();

            return View(admin);
        }

        // ============================================================
        // GET: Admin/ManageAdmins
        // ============================================================
        public async Task<ActionResult> ManageAdmins()
        {
            var admins = await _context.Administrators
                .Include(a => a.User)
                .Include(a => a.CreatedByUser)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(admins);
        }

        // ============================================================
        // GET: Admin/AddAdmin
        // ============================================================
        public ActionResult AddAdmin()
        {
            return View();
        }

        // ============================================================
        // POST: Admin/AddAdmin
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddAdmin(AddAdminViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUserId = User.Identity.GetUserId();
                var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

                var existingUser = await userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    if (await userManager.IsInRoleAsync(existingUser.Id, "Administrator"))
                    {
                        ModelState.AddModelError("", "This user is already an administrator.");
                        return View(model);
                    }

                    await userManager.AddToRoleAsync(existingUser.Id, "Administrator");

                    var admin = new Administrator
                    {
                        UserId = existingUser.Id,
                        FullName = existingUser.FullName ?? model.FullName,
                        Email = model.Email,
                        Department = model.Department,
                        CreatedAt = DateTime.Now,
                        IsActive = true,
                        CreatedByUserId = currentUserId
                    };

                    _context.Administrators.Add(admin);

                    var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
                    var audit = new AdminCreationAudit
                    {
                        NewAdminUserId = existingUser.Id,
                        CreatedByUserId = currentUserId,
                        CreatedAt = DateTime.Now,
                        NewAdminEmail = model.Email,
                        NewAdminName = existingUser.FullName ?? model.FullName,
                        CreatedByName = currentUser?.FullName ?? "Unknown"
                    };
                    _context.AdminCreationAudits.Add(audit);

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"✅ {model.FullName} added as an administrator successfully!";
                    return RedirectToAction("ManageAdmins");
                }

                var newUser = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber ?? "",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                var result = await userManager.CreateAsync(newUser, model.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser.Id, "Administrator");

                    var admin = new Administrator
                    {
                        UserId = newUser.Id,
                        FullName = model.FullName,
                        Email = model.Email,
                        Department = model.Department,
                        CreatedAt = DateTime.Now,
                        IsActive = true,
                        CreatedByUserId = currentUserId
                    };

                    _context.Administrators.Add(admin);

                    var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
                    var audit = new AdminCreationAudit
                    {
                        NewAdminUserId = newUser.Id,
                        CreatedByUserId = currentUserId,
                        CreatedAt = DateTime.Now,
                        NewAdminEmail = model.Email,
                        NewAdminName = model.FullName,
                        CreatedByName = currentUser?.FullName ?? "Unknown"
                    };
                    _context.AdminCreationAudits.Add(audit);

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"✅ {model.FullName} added as an administrator successfully!";
                    return RedirectToAction("ManageAdmins");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
            }

            return View(model);
        }

        // ============================================================
        // GET: Admin/AdminAudit
        // ============================================================
        public async Task<ActionResult> AdminAudit()
        {
            var audits = await _context.AdminCreationAudits
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(audits);
        }

        // ============================================================
        // POST: Admin/RemoveAdmin
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveAdmin(string userId)
        {
            var currentUserId = User.Identity.GetUserId();

            if (userId == currentUserId)
            {
                TempData["ErrorMessage"] = "❌ You cannot remove yourself as an administrator.";
                return RedirectToAction("ManageAdmins");
            }

            var adminCount = await _context.Administrators.CountAsync();
            if (adminCount <= 1)
            {
                TempData["ErrorMessage"] = "❌ Cannot remove the last administrator.";
                return RedirectToAction("ManageAdmins");
            }

            var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var user = await userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await userManager.RemoveFromRoleAsync(userId, "Administrator");

                var admin = await _context.Administrators.FirstOrDefaultAsync(a => a.UserId == userId);
                if (admin != null)
                {
                    _context.Administrators.Remove(admin);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = $"✅ {user.FullName ?? user.Email} removed as an administrator.";
            }

            return RedirectToAction("ManageAdmins");
        }

        // ============================================================
        // GET: Admin/Users (with search and filter)
        // ============================================================
        public async Task<ActionResult> Users(string searchTerm = null, string roleFilter = "All")
        {
            var users = await _context.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var userRoles = new Dictionary<string, string>();
            var userRoleList = new Dictionary<string, List<string>>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user.Id);
                var roleString = roles.Count > 0 ? string.Join(", ", roles) : "No Role";
                userRoles[user.Id] = roleString;
                userRoleList[user.Id] = roles.ToList();
            }

            if (!string.IsNullOrEmpty(roleFilter) && roleFilter != "All")
            {
                var filteredUserIds = userRoleList
                    .Where(kvp => kvp.Value.Contains(roleFilter))
                    .Select(kvp => kvp.Key)
                    .ToList();

                users = users.Where(u => filteredUserIds.Contains(u.Id)).ToList();
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                users = users.Where(u =>
                    u.Email.ToLower().Contains(searchTerm) ||
                    (u.FullName != null && u.FullName.ToLower().Contains(searchTerm))
                ).ToList();
            }

            ViewBag.TotalAdmins = userRoleList.Count(kvp => kvp.Value.Contains("Administrator"));
            ViewBag.TotalOfficers = userRoleList.Count(kvp => kvp.Value.Contains("CollectionOfficer"));
            ViewBag.TotalResidents = userRoleList.Count(kvp => kvp.Value.Contains("Resident"));
            ViewBag.TotalUsers = users.Count;
            ViewBag.UserRoles = userRoles;
            ViewBag.SelectedRole = roleFilter ?? "All";
            ViewBag.SearchTerm = searchTerm ?? "";

            ViewBag.RoleCounts = new Dictionary<string, int>
            {
                { "All", users.Count },
                { "Administrator", ViewBag.TotalAdmins },
                { "CollectionOfficer", ViewBag.TotalOfficers },
                { "Resident", ViewBag.TotalResidents }
            };

            return View(users);
        }

        // ============================================================
        // POST: Admin/DeactivateUser
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeactivateUser(string userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

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
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

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

                residentCounts[community.DropOffPointId] = await _context.Residents
                    .Where(r => r.DropOffPointId == community.DropOffPointId && r.IsActive)
                    .CountAsync();

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

            var statuses = await _context.CommunityStatuses
                .OrderByDescending(c => c.UpdatedDate)
                .ToListAsync();

            ViewBag.CommunityStatuses = statuses;

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

            var latestStatus = await _context.CommunityStatuses
                .Where(c => c.DropOffPointId == id)
                .OrderByDescending(c => c.UpdatedDate)
                .FirstOrDefaultAsync();

            ViewBag.Officers = officers;
            ViewBag.Submissions = submissions;
            ViewBag.TotalWeight = submissions.Sum(s => s.Weight);
            ViewBag.TotalCO2 = submissions.Sum(s => s.Weight * 1.0);
            ViewBag.OfficerCount = officers.Count;
            ViewBag.SubmissionCount = submissions.Count;
            ViewBag.LatestStatus = latestStatus;

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
        // GET: Admin/AssignOfficer (Shows the assignment form)
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
        // POST: Admin/AssignOfficer (with notification)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AssignOfficer(AssignOfficerViewModel model)
        {
            if (ModelState.IsValid)
            {
                var officer = await _context.CollectionOfficers
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.OfficerId == model.OfficerId);

                if (officer == null)
                {
                    TempData["ErrorMessage"] = "Officer not found.";
                    return RedirectToAction("Officers");
                }

                var oldRegion = officer.AssignedDropOffPoint?.Name ?? "None";
                officer.DropOffPointId = model.DropOffPointId;
                await _context.SaveChangesAsync();

                var pointName = model.DropOffPointId.HasValue
                    ? (await _context.DropOffPoints.FirstOrDefaultAsync(d => d.DropOffPointId == model.DropOffPointId.Value))?.Name
                    : "No Region Assigned";

                // SEND NOTIFICATION TO OFFICER
                try
                {
                    var notificationService = System.Web.HttpContext.Current.GetOwinContext().Get<NotificationService>();
                    if (notificationService != null && officer.UserId != null)
                    {
                        await notificationService.NotifyOfficerAssignment(officer.OfficerId, pointName);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Notification failed: {ex.Message}");
                }

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
            var point = await _context.DropOffPoints.FirstOrDefaultAsync(d => d.DropOffPointId == id);

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
                var existingActive = _context.PointsRates
                    .FirstOrDefault(p => p.MaterialTypeId == model.MaterialTypeId && p.IsActive);

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
        // GET: Admin/DeactivateRate
        // ============================================================
        public async Task<ActionResult> DeactivateRate(int id)
        {
            var rate = _context.PointsRates.Find(id);

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
        // GET: Admin/CommunityStatuses
        // ============================================================
        public async Task<ActionResult> CommunityStatuses()
        {
            var statuses = await _context.CommunityStatuses
                .Include(c => c.DropOffPoint)
                .OrderByDescending(c => c.UpdatedDate)
                .ToListAsync();

            return View(statuses);
        }

        // ============================================================
        // GET: Admin/Announcements
        // ============================================================
        public async Task<ActionResult> Announcements()
        {
            var announcements = await _context.Announcements
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(announcements);
        }

        // ============================================================
        // GET: Admin/CreateAnnouncement
        // ============================================================
        public ActionResult CreateAnnouncement()
        {
            return View();
        }

        // ============================================================
        // POST: Admin/CreateAnnouncement
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAnnouncement(Announcement model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedAt = DateTime.Now;
                model.IsActive = true;
                model.CreatedBy = User.Identity.Name;

                _context.Announcements.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "✅ Announcement created successfully!";
                return RedirectToAction("Announcements");
            }

            return View(model);
        }

        // ============================================================
        // POST: Admin/DeleteAnnouncement
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteAnnouncement(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);

            if (announcement == null)
            {
                TempData["ErrorMessage"] = "Announcement not found.";
                return RedirectToAction("Announcements");
            }

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "✅ Announcement deleted successfully!";
            return RedirectToAction("Announcements");
        }

        // ============================================================
        // GET: Admin/EditOfficer
        // ============================================================
        public async Task<ActionResult> EditOfficer(int id)
        {
            var officer = await _context.CollectionOfficers
                .Include(o => o.User)
                .Include(o => o.AssignedDropOffPoint)
                .FirstOrDefaultAsync(o => o.OfficerId == id);

            if (officer == null)
            {
                TempData["ErrorMessage"] = "Officer not found.";
                return RedirectToAction("Officers");
            }

            ViewBag.DropOffPoints = new SelectList(
                await _context.DropOffPoints.Where(d => d.IsActive).ToListAsync(),
                "DropOffPointId",
                "Name",
                officer.DropOffPointId
            );

            return View(officer);
        }

        // ============================================================
        // POST: Admin/EditOfficer
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditOfficer(CollectionOfficer model)
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

                officer.FullName = model.FullName;
                officer.PhoneNumber = model.PhoneNumber;
                officer.Department = model.Department;
                officer.DropOffPointId = model.DropOffPointId;
                officer.IsActive = model.IsActive;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "✅ Officer updated successfully!";
                return RedirectToAction("Officers");
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
        // GET: Admin/DeleteOfficer (Confirmation)
        // ============================================================
        public async Task<ActionResult> DeleteOfficer(int id)
        {
            var officer = await _context.CollectionOfficers
                .Include(o => o.User)
                .Include(o => o.AssignedDropOffPoint)
                .FirstOrDefaultAsync(o => o.OfficerId == id);

            if (officer == null)
            {
                TempData["ErrorMessage"] = "Officer not found.";
                return RedirectToAction("Officers");
            }

            return View(officer);
        }

        // ============================================================
        // POST: Admin/DeleteOfficerConfirmed (Perform deletion)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteOfficerConfirmed(int id)
        {
            var officer = await _context.CollectionOfficers
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OfficerId == id);

            if (officer == null)
            {
                TempData["ErrorMessage"] = "Officer not found.";
                return RedirectToAction("Officers");
            }

            var userName = officer.FullName;
            _context.CollectionOfficers.Remove(officer);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"✅ Officer '{userName}' has been deleted successfully!";
            return RedirectToAction("Officers");
        }

        // ============================================================
        // GET: Admin/DeleteResident (Confirmation)
        // ============================================================
        public async Task<ActionResult> DeleteResident(int id)
        {
            var resident = await _context.Residents
                .Include(r => r.User)
                .Include(r => r.Community)
                .FirstOrDefaultAsync(r => r.ResidentId == id);

            if (resident == null)
            {
                TempData["ErrorMessage"] = "Resident not found.";
                return RedirectToAction("Residents");
            }

            return View(resident);
        }

        // ============================================================
        // POST: Admin/DeleteResidentConfirmed (Perform deletion)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteResidentConfirmed(int id)
        {
            var resident = await _context.Residents
                .Include(r => r.Submissions)
                .Include(r => r.PointsTransactions)
                .FirstOrDefaultAsync(r => r.ResidentId == id);

            if (resident == null)
            {
                TempData["ErrorMessage"] = "Resident not found.";
                return RedirectToAction("Residents");
            }

            var userName = resident.FullName;

            if (resident.Submissions != null && resident.Submissions.Any())
            {
                _context.RecyclingSubmissions.RemoveRange(resident.Submissions);
            }

            if (resident.PointsTransactions != null && resident.PointsTransactions.Any())
            {
                _context.PointsTransactions.RemoveRange(resident.PointsTransactions);
            }

            _context.Residents.Remove(resident);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"✅ Resident '{userName}' has been deleted successfully!";
            return RedirectToAction("Residents");
        }

        // ============================================================
        // GET: Admin/Residents
        // ============================================================
        public async Task<ActionResult> Residents()
        {
            var residents = await _context.Residents
                .Include(r => r.User)
                .Include(r => r.Community)
                .OrderBy(r => r.FullName)
                .ToListAsync();

            return View(residents);
        }

        // ============================================================
        // GET: Admin/ManageRedemptionOptions
        // ============================================================
        public async Task<ActionResult> ManageRedemptionOptions()
        {
            var options = await _context.RedemptionOptions
                .OrderBy(o => o.UtilityType)
                .ToListAsync();

            return View(options);
        }

        // ============================================================
        // GET: Admin/CreateRedemptionOption
        // ============================================================
        public ActionResult CreateRedemptionOption()
        {
            return View();
        }

        // ============================================================
        // POST: Admin/CreateRedemptionOption
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateRedemptionOption(RedemptionOption model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedAt = DateTime.Now;
                _context.RedemptionOptions.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "✅ Redemption option created successfully!";
                return RedirectToAction("ManageRedemptionOptions");
            }

            return View(model);
        }

        // ============================================================
        // POST: Admin/ToggleRedemptionOption
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ToggleRedemptionOption(int id)
        {
            var option = await _context.RedemptionOptions.FindAsync(id);
            if (option != null)
            {
                option.IsActive = !option.IsActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"✅ Option {(option.IsActive ? "activated" : "deactivated")} successfully!";
            }

            return RedirectToAction("ManageRedemptionOptions");
        }

        // ============================================================
        // GET: Admin/ManageRedemptionRequests
        // ============================================================
        public async Task<ActionResult> ManageRedemptionRequests()
        {
            var pendingRequests = await _context.RedemptionRequests
                .Include(r => r.Resident)
                .Include(r => r.RedemptionOption)
                .Where(r => r.RequestStatus == "Pending")
                .OrderBy(r => r.RequestDate)
                .ToListAsync();

            var approvedRequests = await _context.RedemptionRequests
                .Include(r => r.Resident)
                .Include(r => r.RedemptionOption)
                .Where(r => r.RequestStatus == "Approved")
                .OrderByDescending(r => r.ApprovedDate)
                .ToListAsync();

            ViewBag.ApprovedRequests = approvedRequests;

            return View(pendingRequests);
        }

        // ============================================================
        // ✅ FIXED: POST: Admin/ApproveRedemption (Using FirstOrDefaultAsync)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ApproveRedemption(int id, string notes)
        {
            // ✅ FIX: Use FirstOrDefaultAsync instead of FindAsync
            var request = await _context.RedemptionRequests
                .Include(r => r.Resident)
                .Include(r => r.RedemptionOption)
                .FirstOrDefaultAsync(r => r.RequestId == id);

            if (request == null)
            {
                TempData["ErrorMessage"] = "Request not found.";
                return RedirectToAction("ManageRedemptionRequests");
            }

            request.RequestStatus = "Approved";
            request.ApprovedDate = DateTime.Now;
            request.ApprovedBy = User.Identity.GetUserName();
            request.AdminNotes = notes;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"✅ Redemption approved! {request.Resident.FullName} will receive R{request.DiscountAmount} off their {request.UtilityType} bill.";
            return RedirectToAction("ManageRedemptionRequests");
        }

        // ============================================================
        // ✅ FIXED: POST: Admin/RejectRedemption (Using FirstOrDefaultAsync)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RejectRedemption(int id, string reason)
        {
            // ✅ FIX: Use FirstOrDefaultAsync instead of FindAsync
            var request = await _context.RedemptionRequests
                .Include(r => r.Resident)
                .FirstOrDefaultAsync(r => r.RequestId == id);

            if (request == null)
            {
                TempData["ErrorMessage"] = "Request not found.";
                return RedirectToAction("ManageRedemptionRequests");
            }

            if (string.IsNullOrEmpty(reason))
            {
                TempData["ErrorMessage"] = "Please provide a reason for rejection.";
                return RedirectToAction("ManageRedemptionRequests");
            }

            request.RequestStatus = "Rejected";
            request.AdminNotes = reason;

            // Refund points to resident
            var resident = await _context.Residents
                .FirstOrDefaultAsync(r => r.ResidentId == request.ResidentId);

            if (resident != null)
            {
                resident.PointsBalance += request.PointsUsed;

                // Create refund transaction
                var transaction = new PointsTransaction
                {
                    ResidentId = request.ResidentId,
                    Amount = request.PointsUsed,
                    Description = $"Refund: Redemption rejected ({reason})",
                    Type = "Refund",
                    TransactionDate = DateTime.Now,
                    ReferenceId = request.RequestId,
                    Reason = reason
                };
                _context.PointsTransactions.Add(transaction);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "❌ Redemption rejected. Points have been refunded to the resident.";
            return RedirectToAction("ManageRedemptionRequests");
        }

        // ============================================================
        // GET: Admin/ExportRedemptions
        // ============================================================
        public async Task<ActionResult> ExportRedemptions()
        {
            var requests = await _context.RedemptionRequests
                .Include(r => r.Resident)
                .Include(r => r.RedemptionOption)
                .Where(r => r.RequestStatus == "Approved" || r.RequestStatus == "Applied")
                .OrderByDescending(r => r.ApprovedDate)
                .ToListAsync();

            return View(requests);
        }
    }
}