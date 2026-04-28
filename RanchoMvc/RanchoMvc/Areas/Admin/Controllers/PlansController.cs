using System.Linq;
using System.Web.Mvc;
using RanchoMvc.Data;
using RanchoMvc.Filters;
using RanchoMvc.Models;

namespace RanchoMvc.Areas.Admin.Controllers
{
    [ModuleAuthorize(Module = AdminModule.Plans)]
    public class PlansController : BaseAdminController
    {
        private readonly RanchoDbContext _db = new RanchoDbContext();

        public ActionResult Index()
        {
            return View(_db.Plans.OrderBy(p => p.Order).ToList());
        }

        public ActionResult Create()
        {
            return View(new Plan { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Plan plan)
        {
            if (!ModelState.IsValid)
                return View(plan);
            _db.Plans.Add(plan);
            _db.SaveChanges();
            TempData["Success"] = "Plan creado correctamente.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var plan = _db.Plans.Find(id);
            if (plan == null) return HttpNotFound();
            return View(plan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Plan plan)
        {
            if (!ModelState.IsValid)
                return View(plan);
            _db.Entry(plan).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();
            TempData["Success"] = "Plan actualizado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var plan = _db.Plans.Find(id);
            if (plan != null)
            {
                _db.Plans.Remove(plan);
                _db.SaveChanges();
            }
            TempData["Success"] = "Plan eliminado.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ToggleActive(int id)
        {
            var plan = _db.Plans.Find(id);
            if (plan == null) return HttpNotFound();
            plan.IsActive = !plan.IsActive;
            _db.SaveChanges();
            return Json(new { isActive = plan.IsActive });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
