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
        // GET: /Account/Login (General - Fallback)
        // ============================================================
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.LoginTitle = "Login";
            ViewBag.LoginSubtitle = "Log in to your account";
            ViewBag.Role = "General";
            ViewBag.ExpectedRole = "General";
            return View("Login");
        }

        // ============================================================
        // GET: /Account/UserLogin (Resident)
        // ============================================================
        [AllowAnonymous]
        public ActionResult UserLogin(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.LoginTitle = "Resident Login";
            ViewBag.LoginSubtitle = "Log in to your resident account";
            ViewBag.Role = "Resident";
            ViewBag.ExpectedRole = "Resident";
            return View("Login");
        }

        // ============================================================
        // GET: /Account/OfficerLogin (Collection Officer)
        // ============================================================
        [AllowAnonymous]
        public ActionResult OfficerLogin(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.LoginTitle = "Officer Login";
            ViewBag.LoginSubtitle = "Log in to your officer dashboard";
            ViewBag.Role = "CollectionOfficer";
            ViewBag.ExpectedRole = "CollectionOfficer";
            return View("Login");
        }

        // ============================================================
        // GET: /Account/AdminLogin (Administrator)
        // ============================================================
        [AllowAnonymous]
        public ActionResult AdminLogin(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl ?? "/Admin/Dashboard";
            ViewBag.LoginTitle = "Admin Login";
            ViewBag.LoginSubtitle = "Secure administrator access";
            ViewBag.Role = "Administrator";
            ViewBag.ExpectedRole = "Administrator";
            return View("Login");
        }

        // ============================================================
        // POST: /Account/Login (Handles all role logins with validation)
        // ============================================================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl, string expectedRole)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check for empty string AND null - use the first non-empty value
            string role = "General";

            if (!string.IsNullOrEmpty(expectedRole))
            {
                role = expectedRole;
            }
            else if (!string.IsNullOrEmpty(model.ExpectedRole))
            {
                role = model.ExpectedRole;
            }
            else if (!string.IsNullOrEmpty(ViewBag.ExpectedRole as string))
            {
                role = ViewBag.ExpectedRole as string;
            }

            System.Diagnostics.Debug.WriteLine($"=== LOGIN DEBUG ===");
            System.Diagnostics.Debug.WriteLine($"Email: {model.Email}");
            System.Diagnostics.Debug.WriteLine($"ExpectedRole from parameter: '{expectedRole ?? "NULL"}'");
            System.Diagnostics.Debug.WriteLine($"ExpectedRole from model: '{model.ExpectedRole ?? "NULL"}'");
            System.Diagnostics.Debug.WriteLine($"Using ExpectedRole: '{role}'");
            System.Diagnostics.Debug.WriteLine($"==================");

            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);

            switch (result)
            {
                case SignInStatus.Success:
                    var user = await UserManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        bool isValidRole = false;

                        switch (role)
                        {
                            case "Administrator":
                                isValidRole = await UserManager.IsInRoleAsync(user.Id, "Administrator");
                                break;
                            case "CollectionOfficer":
                                isValidRole = await UserManager.IsInRoleAsync(user.Id, "CollectionOfficer");
                                break;
                            case "Resident":
                                isValidRole = await UserManager.IsInRoleAsync(user.Id, "Resident");
                                break;
                            default:
                                isValidRole = true;
                                break;
                        }

                        System.Diagnostics.Debug.WriteLine($"IsValidRole: {isValidRole}");

                        if (!isValidRole)
                        {
                            ModelState.AddModelError("", "❌ Invalid login for this portal. Please use the correct login page for your role.");
                            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                            return View(model);
                        }

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
                        return RedirectToLocal(returnUrl);
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
            var communities = _context.DropOffPoints
                .Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem
                {
                    Value = d.DropOffPointId.ToString(),
                    Text = d.Name
                })
                .ToList();

            communities.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "-- Select Your Community --"
            });

            ViewBag.Communities = new SelectList(communities, "Value", "Text");

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
                        // Ensure roles exist
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

                        await UserManager.AddToRoleAsync(user.Id, "Resident");

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
                            CreatedAt = DateTime.Now,
                            Address = model.Address,
                            Suburb = model.Suburb,
                            City = model.City,
                            Province = model.Province,
                            PostalCode = model.PostalCode,
                            DropOffPointId = model.DropOffPointId
                        };

                        _context.Residents.Add(resident);
                        await _context.SaveChangesAsync();

                        // Process referral code
                        if (!string.IsNullOrEmpty(model.ReferralCode))
                        {
                            var referrer = _context.Residents
                                .FirstOrDefault(r => r.ReferralCode == model.ReferralCode);

                            if (referrer != null && referrer.UserId != user.Id)
                            {
                                var welcomeBonus = GetConfigValue("WelcomeBonusPoints", 100);
                                var influencerPoints = GetConfigValue("InfluencerPointsPerReferral", 50);

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

            // Reload communities if registration fails
            var communities = _context.DropOffPoints
                .Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem
                {
                    Value = d.DropOffPointId.ToString(),
                    Text = d.Name
                })
                .ToList();

            communities.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "-- Select Your Community --"
            });

            ViewBag.Communities = new SelectList(communities, "Value", "Text");

            return View(model);
        }

        // ============================================================
        // GET: /Account/Logout
        // ============================================================
        [HttpGet]
        public ActionResult Logout()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        // ============================================================
        // POST: /Account/Logout
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogoutPost()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        // ============================================================
        // ✅ GET: /Account/ForgotPassword (MISSING - ADDED!)
        // ============================================================
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        // ============================================================
        // ✅ POST: /Account/ForgotPassword (Shows reset link)
        // ============================================================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    // Generate password reset token
                    var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);

                    // Build the reset link
                    var callbackUrl = Url.Action("ResetPassword", "Account",
                        new { userId = user.Id, code = code },
                        protocol: Request.Url.Scheme);

                    // ✅ FOR DEMO: Store the link in TempData to display on confirmation page
                    TempData["ResetLink"] = callbackUrl;
                }

                // Always return the confirmation view (don't reveal if user exists or not)
                return View("ForgotPasswordConfirmation");
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