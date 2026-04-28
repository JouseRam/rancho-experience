using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.Identity;
using RanchoMvc.Data;

namespace RanchoMvc.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class ModuleAuthorizeAttribute : AuthorizeAttribute
    {
        // The module key (Plans, Reservations, Messages, Gallery, Settings, or empty for dashboard)
        public string Module { get; set; }

        // Force CanEdit check even on GET requests (e.g. Create/Edit form pages)
        public bool RequireEdit { get; set; }

        // Only SuperAdmins can access (used for Users controller)
        public bool SuperAdminOnly { get; set; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated) return false;

            var userId = httpContext.User.Identity.GetUserId();
            if (string.IsNullOrEmpty(userId)) return false;

            using (var db = new RanchoDbContext())
            {
                var user = db.Users.Find(userId);
                if (user == null) return false;

                if (SuperAdminOnly) return user.IsSuperAdmin;
                if (user.IsSuperAdmin) return true;
                if (string.IsNullOrEmpty(Module)) return true;

                var perm = db.UserModulePermissions
                    .FirstOrDefault(p => p.UserId == userId && p.Module == Module);
                if (perm == null) return false;

                bool isWrite = !string.Equals(httpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase)
                             && !string.Equals(httpContext.Request.HttpMethod, "HEAD", StringComparison.OrdinalIgnoreCase);

                if (isWrite || RequireEdit) return perm.CanEdit;
                return perm.CanView;
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // Logged in but no permission — back to dashboard with notice
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
                {
                    { "area",       "Admin" },
                    { "controller", "Dashboard" },
                    { "action",     "Index" },
                    { "permError",  "1" }
                });
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }
}
