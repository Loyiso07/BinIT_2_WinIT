using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using BinIT2WinIT.Data;
using BinIT2WinIT.Models;

namespace BinIT2WinIT.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        public ActionResult Index()
        {
            // Get some statistics for the home page
            var totalResidents = _context.Residents.Count();
            var totalSubmissions = _context.RecyclingSubmissions.Count();
            var totalPoints = _context.PointsTransactions.Sum(t => (int?)t.Amount) ?? 0;

            ViewBag.TotalResidents = totalResidents;
            ViewBag.TotalSubmissions = totalSubmissions;
            ViewBag.TotalPoints = totalPoints;

            // Check if user is logged in and get their role
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId();
                if (User.IsInRole("Resident"))
                {
                    var resident = _context.Residents.FirstOrDefault(r => r.UserId == userId);
                    if (resident != null)
                    {
                        ViewBag.UserPoints = resident.PointsBalance;
                        ViewBag.UserName = resident.FullName;
                    }
                }
            }

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Smart Recycling Rewards System - Empowering communities through sustainable innovation.";

            // Get some environmental impact stats
            var totalCO2Saved = _context.Residents.Sum(r => r.TotalCO2Saved);
            ViewBag.TotalCO2Saved = totalCO2Saved;
            ViewBag.TotalRecycled = _context.RecyclingSubmissions
                .Where(s => s.Status == "Confirmed")
                .Sum(s => s.Weight);

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contact us for more information about the Smart Recycling Rewards System.";
            return View();
        }

        //  Error page
        public ActionResult Error()
        {
            return View();
        }

        //  Access Denied page
        public ActionResult AccessDenied()
        {
            return View();
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