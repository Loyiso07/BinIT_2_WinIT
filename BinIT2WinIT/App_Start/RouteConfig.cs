using System.Web.Mvc;
using System.Web.Routing;

public class RouteConfig
{
    public static void RegisterRoutes(RouteCollection routes)
    {
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

        //  ENABLE ATTRIBUTE ROUTING
        routes.MapMvcAttributeRoutes();

        // ADMIN ROUTE
        routes.MapRoute(
            name: "Admin",
            url: "Admin",
            defaults: new { controller = "Admin", action = "Dashboard" }
        );

        // RESIDENT DASHBOARD ROUTE (optional)
        routes.MapRoute(
            name: "Resident",
            url: "Resident",
            defaults: new { controller = "Resident", action = "Dashboard" }
        );

        // OFFICER DASHBOARD ROUTE (optional)
        routes.MapRoute(
            name: "Officer",
            url: "Officer",
            defaults: new { controller = "Officer", action = "Dashboard" }
        );

        // DEFAULT ROUTE (MUST BE LAST)
        routes.MapRoute(
            name: "Default",
            url: "{controller}/{action}/{id}",
            defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
        );
    }
}