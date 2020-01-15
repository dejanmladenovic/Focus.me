using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Focus_me
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapMvcAttributeRoutes();

            routes.MapRoute(
                "Homepage", // Route name
                "",        // URL with parameters
                new { controller = "Post", action = "Index" }  // Parameter defaults
            );

            routes.MapRoute(
                "users",
                "user/{user_name}",
                new { controller = "Users", action = "ViewUser", user_name = UrlParameter.Optional }
            );

            routes.MapRoute(
                "register",
                "register",
                new { controller = "Users", action = "Register" }
            );

            routes.MapRoute(
                "login",
                "login",
                new { controller = "Users", action = "Login" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            
        }
    }
}
