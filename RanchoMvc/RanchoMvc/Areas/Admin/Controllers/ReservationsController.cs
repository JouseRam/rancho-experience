using System.Linq;
using System.Web.Mvc;
using RanchoMvc.Data;

namespace RanchoMvc.Areas.Admin.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly RanchoDbContext _db = new RanchoDbContext();

        public ActionResult Index(string status = null)
        {
            var query = _db.Reservations.Include("Plan").AsQueryable();
            if (!string.IsNullOrEmpty(status))
                query = query.Where(r => r.Status == status);
            ViewBag.CurrentStatus = status;
            ViewBag.Statuses = new[] { "Pendiente", "Confirmada", "Cancelada" };
            return View(query.OrderByDescending(r => r.CreatedAt).ToList());
        }

        public ActionResult Details(int id)
        {
            var r = _db.Reservations.Include("Plan").FirstOrDefault(x => x.Id == id);
            if (r == null) return HttpNotFound();
            ViewBag.Statuses = new[] { "Pendiente", "Confirmada", "Cancelada" };
            return View(r);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string status)
        {
            var r = _db.Reservations.Find(id);
            if (r == null) return HttpNotFound();
            r.Status = status;
            _db.SaveChanges();
            TempData["Success"] = "Estado actualizado.";
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var r = _db.Reservations.Find(id);
            if (r != null) { _db.Reservations.Remove(r); _db.SaveChanges(); }
            TempData["Success"] = "Cotización eliminada.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
