using BinIT2WinIT.Data;
using BinIT2WinIT.Models;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BinIT2WinIT.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        // GET: Admin/Dashboard
        public async Task<ActionResult> Dashboard()
        {
            var totalResidents = await _context.Residents.CountAsync();
            var totalSubmissions = await _context.RecyclingSubmissions.CountAsync();
            var totalPoints = await _context.PointsTransactions.SumAsync(t => t.Amount);

            ViewBag.TotalResidents = totalResidents;
            ViewBag.TotalSubmissions = totalSubmissions;
            ViewBag.TotalPoints = totalPoints;

            return View();
        }

        // GET: Admin/PointsRates
        public async Task<ActionResult> PointsRates()
        {
            var rates = await _context.PointsRates
                .Include(p => p.MaterialType)
                .ToListAsync();
            return View(rates);
        }

        // GET: Admin/Users
        public async Task<ActionResult> Users()
        {
            // EF6 - Use FirstOrDefaultAsync instead of FindAsync
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // POST: Admin/DeactivateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeactivateUser(string userId)
        {
            // EF6 - Use FirstOrDefaultAsync instead of FindAsync
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                user.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "User deactivated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "User not found.";
            }

            return RedirectToAction("Users");
        }
    }
}