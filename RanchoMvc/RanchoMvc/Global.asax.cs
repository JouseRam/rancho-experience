using System;
using System.Data.Entity;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Routing;
using RanchoMvc.App_Start;
using RanchoMvc.Data;
using RanchoMvc.Data.Migrations;

namespace RanchoMvc
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AntiForgeryConfig.SuppressIdentityHeuristicChecks = true;

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            Database.SetInitializer(
                new MigrateDatabaseToLatestVersion<RanchoDbContext, Configuration>());

            using (var db = new RanchoDbContext())
            {
                db.Database.Initialize(force: false);
            }
        }
    }
}
