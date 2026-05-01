using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using RanchoMvc.Data;
using RanchoMvc.Models;

namespace RanchoMvc.Areas.Admin.Controllers
{
    public abstract class BaseAdminController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (!User.Identity.IsAuthenticated) return;

            var userId = User.Identity.GetUserId();
            using (var db = new RanchoDbContext())
            {
                var user = db.Users.Find(userId);
                bool isSA = user != null && user.IsSuperAdmin;
                ViewBag.IsSuperAdmin = isSA;

                if (!isSA)
                {
                    var perms = db.UserModulePermissions
                        .Where(p => p.UserId == userId)
                        .ToDictionary(p => p.Module);
                    ViewBag.UserPerms = perms;
                }
                else
                {
                    ViewBag.UserPerms = null;
                }

                ViewBag.PendingResCount = db.Reservations.Count(r => r.Status == "Pendiente");
                ViewBag.UnreadMsgCount  = db.ContactMessages.Count(m => !m.IsRead);
            }
        }
    }
}
