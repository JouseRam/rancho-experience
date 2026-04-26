using System.Linq;
using System.Web.Mvc;
using RanchoMvc.Data;

namespace RanchoMvc.Areas.Admin.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly RanchoDbContext _db = new RanchoDbContext();

        public ActionResult Index()
        {
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
