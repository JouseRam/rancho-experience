using System.Linq;
using System.Web.Mvc;
using RanchoMvc.Data;
using RanchoMvc.Filters;
using RanchoMvc.Models;

namespace RanchoMvc.Areas.Admin.Controllers
{
    [ModuleAuthorize(Module = AdminModule.Messages)]
    public class MessagesController : BaseAdminController
    {
        private readonly RanchoDbContext _db = new RanchoDbContext();

        public ActionResult Index()
        {
            var messages = _db.ContactMessages
                .OrderByDescending(m => m.CreatedAt)
                .ToList();
            return View(messages);
        }

        public ActionResult Details(int id)
        {
            var msg = _db.ContactMessages.Find(id);
            if (msg == null) return HttpNotFound();
            if (!msg.IsRead)
            {
                msg.IsRead = true;
                _db.SaveChanges();
            }
            return View(msg);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var msg = _db.ContactMessages.Find(id);
            if (msg != null) { _db.ContactMessages.Remove(msg); _db.SaveChanges(); }
            TempData["Success"] = "Mensaje eliminado.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
