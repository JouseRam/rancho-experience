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
            var logoUrl = HttpRuntime.Cache["SiteLogoUrl"] as string;
            if (logoUrl == null)
            {
                var s = Db.SiteSettings.FirstOrDefault(x => x.Key == "LogoUrl");
                logoUrl = s != null ? s.Value : "";
                HttpRuntime.Cache.Insert("SiteLogoUrl", logoUrl, null,
                    DateTime.UtcNow.AddMinutes(10),
                    System.Web.Caching.Cache.NoSlidingExpiration);
            }
            ViewBag.LogoUrl = logoUrl;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db?.Dispose();
            base.Dispose(disposing);
        }
    }
}
