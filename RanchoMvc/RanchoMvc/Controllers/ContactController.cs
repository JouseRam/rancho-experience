using System;
using System.Linq;
using System.Web.Mvc;
using RanchoMvc.Data;
using RanchoMvc.Models;
using RanchoMvc.Models.ViewModels;

namespace RanchoMvc.Controllers
{
    public class ContactController : BaseController
    {
        private readonly RanchoDbContext _db = new RanchoDbContext();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Send(ContactViewModel vm)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Datos inválidos." });

            var msg = new ContactMessage
            {
                Name = vm.Name,
                Email = vm.Email,
                Phone = vm.Phone,
                Company = vm.Company,
                Message = vm.Message,
                CreatedAt = DateTime.UtcNow
            };
            _db.ContactMessages.Add(msg);
            _db.SaveChanges();

            return Json(new { success = true, message = "¡Mensaje recibido! Te contactaremos pronto." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Quote(ReservationViewModel vm)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Por favor completa los campos requeridos." });

            var reservation = new Reservation
            {
                CompanyName = vm.CompanyName,
                ContactName = vm.ContactName,
                Email = vm.Email,
                Phone = vm.Phone,
                PlanId = vm.PlanId,
                PreferredDate = vm.PreferredDate,
                GuestCount = vm.GuestCount,
                Notes = vm.Notes,
                Status = "Pendiente",
                CreatedAt = DateTime.UtcNow
            };
            _db.Reservations.Add(reservation);
            _db.SaveChanges();

            return Json(new { success = true, message = "¡Cotización enviada! Nos pondremos en contacto en breve." });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
