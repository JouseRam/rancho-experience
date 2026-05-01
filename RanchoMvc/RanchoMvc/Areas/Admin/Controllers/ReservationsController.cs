using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using RanchoMvc.Data;
using RanchoMvc.Filters;
using RanchoMvc.Models;
using RanchoMvc.Services;

namespace RanchoMvc.Areas.Admin.Controllers
{
    [ModuleAuthorize(Module = AdminModule.Reservations)]
    public class ReservationsController : BaseAdminController
    {
        private readonly RanchoDbContext _db = new RanchoDbContext();

        private static readonly string[] AllStatuses = { "Pendiente", "Confirmada", "Cancelada", "Pendiente de Pago" };

        public ActionResult Index(string status = null, int? planId = null,
            string paymentMethod = null, DateTime? from = null, DateTime? to = null,
            int page = 1, string search = null)
        {
            var query = _db.Reservations.Include("Plan").AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(r => r.ContactName.Contains(search)
                                      || r.Email.Contains(search)
                                      || r.CompanyName.Contains(search)
                                      || r.Phone.Contains(search));
            if (!string.IsNullOrEmpty(status))
                query = query.Where(r => r.Status == status);
            if (planId.HasValue)
                query = query.Where(r => r.PlanId == planId);
            if (!string.IsNullOrEmpty(paymentMethod))
                query = query.Where(r => r.PaymentMethod == paymentMethod);
            if (from.HasValue)
                query = query.Where(r => r.CreatedAt >= from.Value);
            if (to.HasValue)
            {
                var toEnd = to.Value.Date.AddDays(1);
                query = query.Where(r => r.CreatedAt < toEnd);
            }

            const int pageSize = 15;
            var totalCount = query.Count();
            var items = query.OrderByDescending(r => r.CreatedAt)
                             .Skip((page - 1) * pageSize)
                             .Take(pageSize)
                             .ToList();

            ViewBag.CurrentStatus    = status;
            ViewBag.CurrentPlanId    = planId;
            ViewBag.CurrentPayment   = paymentMethod;
            ViewBag.CurrentFrom      = from?.ToString("yyyy-MM-dd");
            ViewBag.CurrentTo        = to?.ToString("yyyy-MM-dd");
            ViewBag.CurrentSearch    = search;
            ViewBag.Statuses         = AllStatuses;
            ViewBag.Plans            = _db.Plans.Where(p => p.IsActive).OrderBy(p => p.Title).ToList();
            ViewBag.CurrentPage      = page;
            ViewBag.TotalPages       = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalCount       = totalCount;

            return View(items);
        }

        public ActionResult Details(int id)
        {
            var r = _db.Reservations.Include("Plan").FirstOrDefault(x => x.Id == id);
            if (r == null) return HttpNotFound();
            ViewBag.Statuses = AllStatuses;
            return View(r);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string status)
        {
            var r = _db.Reservations.Find(id);
            if (r == null) return HttpNotFound();
            var oldStatus = r.Status;
            r.Status = status;
            _db.SaveChanges();

            if (oldStatus != status && (status == "Confirmada" || status == "Cancelada"))
            {
                EmailService.SendReservationStatus(r.Email, r.ContactName,
                    string.IsNullOrEmpty(r.CompanyName) ? r.ContactName : r.CompanyName,
                    status, r.Id);
            }

            TempData["Success"] = "Estado actualizado.";
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveAdminNotes(int id, string adminNotes)
        {
            var r = _db.Reservations.Find(id);
            if (r == null) return HttpNotFound();
            r.AdminNotes = adminNotes;
            _db.SaveChanges();
            TempData["Success"] = "Notas guardadas.";
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

        [HttpGet]
        public ActionResult Export(string status = null, int? planId = null,
            string paymentMethod = null, DateTime? from = null, DateTime? to = null, string search = null)
        {
            var query = _db.Reservations.Include("Plan").AsQueryable();
            if (!string.IsNullOrEmpty(search))        query = query.Where(r => r.ContactName.Contains(search) || r.Email.Contains(search) || r.CompanyName.Contains(search));
            if (!string.IsNullOrEmpty(status))        query = query.Where(r => r.Status == status);
            if (planId.HasValue)                       query = query.Where(r => r.PlanId == planId);
            if (!string.IsNullOrEmpty(paymentMethod)) query = query.Where(r => r.PaymentMethod == paymentMethod);
            if (from.HasValue)                         query = query.Where(r => r.CreatedAt >= from.Value);
            if (to.HasValue)
            {
                var toEnd = to.Value.Date.AddDays(1);
                query = query.Where(r => r.CreatedAt < toEnd);
            }

            var data = query.OrderByDescending(r => r.CreatedAt).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Empresa/Grupo,Contacto,Email,Teléfono,Plan,Fecha preferida,Personas,Autos,Método de pago,Estado,Fecha solicitud");
            foreach (var r in data)
            {
                sb.AppendLine(string.Join(",",
                    CsvEscape(r.CompanyName),
                    CsvEscape(r.ContactName),
                    CsvEscape(r.Email),
                    CsvEscape(r.Phone),
                    CsvEscape(r.Plan != null ? r.Plan.Title : ""),
                    r.PreferredDate.HasValue ? r.PreferredDate.Value.ToString("dd/MM/yyyy") : "",
                    r.GuestCount.ToString(),
                    r.NumberOfCars.ToString(),
                    CsvEscape(r.PaymentMethod),
                    CsvEscape(r.Status),
                    r.CreatedAt.ToString("dd/MM/yyyy HH:mm")));
            }

            var bytes = new byte[] { 0xEF, 0xBB, 0xBF }.Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv", $"cotizaciones_{DateTime.Now:yyyyMMdd}.csv");
        }

        private static string CsvEscape(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            s = s.Replace("\"", "\"\"");
            return s.Contains(',') || s.Contains('"') || s.Contains('\n') ? $"\"{s}\"" : s;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
