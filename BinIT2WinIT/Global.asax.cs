using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using BinIT2WinIT.Data;
using global::BinIT2WinIT.Data;
namespace BinIT2WinIT
{
        public class MvcApplication : System.Web.HttpApplication
        {
            protected void Application_Start()
            {
                AreaRegistration.RegisterAllAreas();
                RouteConfig.RegisterRoutes(RouteTable.Routes);
                BundleConfig.RegisterBundles(BundleTable.Bundles);

                // Seed Database
                using (var context = new ApplicationDbContext())
                {
                    DbInitializer.Seed(context);
                }
            }
        }
    }

