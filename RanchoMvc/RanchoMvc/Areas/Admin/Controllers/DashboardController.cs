using System.Linq;
using System.Web.Mvc;
using RanchoMvc.Data;
using RanchoMvc.Filters;

namespace RanchoMvc.Areas.Admin.Controllers
{
    [ModuleAuthorize]
    public class DashboardController : BaseAdminController
    {
        private readonly RanchoDbContext _db = new RanchoDbContext();

        public ActionResult Index(string permError = null)
        {
            if (!string.IsNullOrEmpty(permError))
                TempData["Error"] = "No tienes permiso para acceder a esa sección.";

            ViewBag.TotalPlans = _db.Plans.Count(p => p.IsActive);
            ViewBag.PendingReservations = _db.Reservations.Count(r => r.Status == "Pendiente");
            ViewBag.UnreadMessages = _db.ContactMessages.Count(m => !m.IsRead);
            ViewBag.TotalGallery = _db.GalleryImages.Count(g => g.IsActive);
            ViewBag.RecentReservations = _db.Reservations
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToList();
            ViewBag.RecentMessages = _db.ContactMessages
                .OrderByDescending(m => m.CreatedAt)
                .Take(5)
                .ToList();
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
