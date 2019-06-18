using System.Web.Mvc;
using System.Web.Routing;

namespace Kartverket.Geonorge.Download
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("OIDC-callback-signout", "signout-callback-oidc", new { controller = "Home", action = "SignOutCallback"});

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
