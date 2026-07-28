using Microsoft.Owin;
using Owin;
using BinIT2WinIT.App_Start;
using BinIT2WinIT.Data;
using BinIT2WinIT.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security.Cookies;
using BinIT2WinIT.Models;
using Microsoft.AspNet.Identity.Owin;
using System;

[assembly: OwinStartup(typeof(BinIT2WinIT.Startup))]

namespace BinIT2WinIT
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Register DbContext
            app.CreatePerOwinContext<ApplicationDbContext>(ApplicationDbContext.Create);

            // Register UserManager, RoleManager, SignInManager
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationRoleManager>(ApplicationRoleManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // ✅ Register Notification Service (now implements IDisposable)
            app.CreatePerOwinContext<NotificationService>(CreateNotificationService);

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity = SecurityStampValidator
                        .OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                            validateInterval: TimeSpan.FromMinutes(30),
                            regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });
        }

        private NotificationService CreateNotificationService()
        {
            var context = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            return new NotificationService(context, userManager);
        }
    }
}