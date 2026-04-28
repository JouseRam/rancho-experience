using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RanchoMvc.Data;

namespace RanchoMvc.Controllers
{
    public abstract class BaseController : Controller
    {
        private RanchoDbContext _db;
        protected RanchoDbContext Db => _db ?? (_db = new RanchoDbContext());

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            var settings = HttpRuntime.Cache["SiteSettings"] as System.Collections.Generic.Dictionary<string, string>;
            if (settings == null)
            {
                settings = new System.Collections.Generic.Dictionary<string, string>();
                foreach (var s in Db.SiteSettings.ToList())
                    settings[s.Key] = s.Value;
                HttpRuntime.Cache.Insert("SiteSettings", settings, null,
                    DateTime.UtcNow.AddMinutes(10),
                    System.Web.Caching.Cache.NoSlidingExpiration);
            }
            ViewBag.SiteSettings = settings;
            string logo;
            ViewBag.LogoUrl = settings.TryGetValue("LogoUrl", out logo) ? logo : "";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db?.Dispose();
            base.Dispose(disposing);
        }
    }
}
