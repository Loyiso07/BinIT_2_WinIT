using BinIT2WinIT.App_Start;
using BinIT2WinIT.Data;
using BinIT2WinIT.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BinIT2WinIT.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        // ============================================================
        // CONSTRUCTORS
        // ============================================================
        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = roleManager;
        }

        // ============================================================
        // PROPERTIES
        // ============================================================
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        private IAuthenticationManager AuthenticationManager
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        // ============================================================
        // GET: /Account/Login
        // ============================================================
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // ============================================================
        // POST: /Account/Login
        // ============================================================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);

            switch (result)
            {
                case SignInStatus.Success:
                    var user = await UserManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        if (await UserManager.IsInRoleAsync(user.Id, "Administrator"))
                        {
                            return RedirectToAction("Dashboard", "Admin");
                        }
                        else if (await UserManager.IsInRoleAsync(user.Id, "CollectionOfficer"))
                        {
                            return RedirectToAction("Dashboard", "Officer");
                        }
                        else if (await UserManager.IsInRoleAsync(user.Id, "Resident"))
                        {
                            return RedirectToAction("Dashboard", "Resident");
                        }
                    }
                    return RedirectToLocal(returnUrl);

                case SignInStatus.LockedOut:
                    return View("Lockout");

                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });

                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        // ============================================================
        // GET: /Account/Register
        // ============================================================
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        // ============================================================
        // POST: /Account/Register
        // ============================================================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = new ApplicationUser
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        FullName = model.FullName,
                        PhoneNumber = model.PhoneNumber,
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    };

                    var result = await UserManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        // ✅ ENSURE ROLES EXIST
                        if (!await RoleManager.RoleExistsAsync("Administrator"))
                        {
                            await RoleManager.CreateAsync(new IdentityRole("Administrator"));
                        }
                        if (!await RoleManager.RoleExistsAsync("CollectionOfficer"))
                        {
                            await RoleManager.CreateAsync(new IdentityRole("CollectionOfficer"));
                        }
                        if (!await RoleManager.RoleExistsAsync("Resident"))
                        {
                            await RoleManager.CreateAsync(new IdentityRole("Resident"));
                        }

                        // Add user to Resident role
                        await UserManager.AddToRoleAsync(user.Id, "Resident");

                        // Create Resident profile
                        var resident = new Resident
                        {
                            UserId = user.Id,
                            FullName = model.FullName,
                            PhoneNumber = model.PhoneNumber ?? "",
                            PointsBalance = 100,
                            InfluencerPoints = 0,
                            TotalCO2Saved = 0,
                            TotalReferrals = 0,
                            ReferralCode = GenerateReferralCode(),
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        };

                        _context.Residents.Add(resident);
                        await _context.SaveChangesAsync();

                        // ✅ PROCESS REFERRAL CODE
                        if (!string.IsNullOrEmpty(model.ReferralCode))
                        {
                            var referrer = _context.Residents
                                .FirstOrDefault(r => r.ReferralCode == model.ReferralCode);

                            if (referrer != null && referrer.UserId != user.Id)
                            {
                                var welcomeBonusConfig = _context.SystemConfigurations
                                    .FirstOrDefault(c => c.ConfigKey == "WelcomeBonusPoints");
                                var influencerConfig = _context.SystemConfigurations
                                    .FirstOrDefault(c => c.ConfigKey == "InfluencerPointsPerReferral");

                                var welcomeBonus = welcomeBonusConfig != null ? int.Parse(welcomeBonusConfig.ConfigValue) : 100;
                                var influencerPoints = influencerConfig != null ? int.Parse(influencerConfig.ConfigValue) : 50;

                                var referral = new ReferralTransaction
                                {
                                    ReferrerId = referrer.ResidentId,
                                    NewResidentId = resident.ResidentId,
                                    PromoCodeUsed = model.ReferralCode,
                                    InfluencerPointsEarned = influencerPoints,
                                    WelcomeBonusAwarded = welcomeBonus,
                                    TransactionDate = DateTime.Now,
                                    Status = "Completed"
                                };
                                _context.ReferralTransactions.Add(referral);

                                referrer.InfluencerPoints += influencerPoints;
                                referrer.TotalReferrals += 1;
                                resident.PointsBalance += welcomeBonus;

                                await _context.SaveChangesAsync();

                                TempData["SuccessMessage"] = $"✅ Welcome! You earned {welcomeBonus} bonus points!";
                            }
                        }

                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                        return RedirectToAction("Dashboard", "Resident");
                    }

                    AddErrors(result);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Registration failed: " + ex.Message);
                }
            }

            return View(model);
        }

        // ============================================================
        // ✅ FIX: GET /Account/Logout (For direct link clicks)
        // ============================================================
        [HttpGet]
        public ActionResult Logout()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        // ============================================================
        // POST: /Account/Logout (For forms - more secure)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogoutPost()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        // ============================================================
        // GET: /Account/ForgotPassword
        // ============================================================
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        // ============================================================
        // POST: /Account/ForgotPassword
        // ============================================================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    return View("ForgotPasswordConfirmation");
                }
            }

            return View(model);
        }

        // ============================================================
        // GET: /Account/ForgotPasswordConfirmation
        // ============================================================
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // ============================================================
        // GET: /Account/ResetPassword
        // ============================================================
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        // ============================================================
        // POST: /Account/ResetPassword
        // ============================================================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }

            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }

            AddErrors(result);
            return View();
        }

        // ============================================================
        // GET: /Account/ResetPasswordConfirmation
        // ============================================================
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // ============================================================
        // GET: /Account/AccessDenied
        // ============================================================
        [AllowAnonymous]
        public ActionResult AccessDenied()
        {
            return View();
        }

        // ============================================================
        // HELPERS
        // ============================================================
        private string GenerateReferralCode()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var code = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            while (_context.Residents.Any(r => r.ReferralCode == code))
            {
                code = new string(Enumerable.Repeat(chars, 8)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            }
            return code;
        }

        private int GetConfigValue(string key, int defaultValue)
        {
            var config = _context.SystemConfigurations
                .FirstOrDefault(c => c.ConfigKey == key);

            return config != null ? int.Parse(config.ConfigValue) : defaultValue;
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
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