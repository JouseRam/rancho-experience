using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using Newtonsoft.Json;
using RanchoMvc.Data;
using RanchoMvc.Models;
using RanchoMvc.Models.ViewModels;
using RanchoMvc.Services;

namespace RanchoMvc.Controllers
{
    public class ContactController : BaseController
    {
        private readonly RanchoDbContext _db = new RanchoDbContext();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Send([Bind(Prefix = "ContactForm")] ContactViewModel vm)
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
        public ActionResult Quote([Bind(Prefix = "ReservationForm")] ReservationViewModel vm)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Por favor completa los campos requeridos." });

            var plan = vm.PlanId.HasValue ? _db.Plans.Find(vm.PlanId.Value) : null;
            var total = plan != null ? plan.Price * vm.NumberOfCars : 0m;

            var reservation = new Reservation
            {
                CompanyName = vm.CompanyName,
                ContactName = vm.ContactName,
                Email = vm.Email,
                Phone = vm.Phone,
                PlanId = vm.PlanId,
                PreferredDate = vm.PreferredDate,
                GuestCount = vm.GuestCount,
                NumberOfCars = vm.NumberOfCars,
                PaymentMethod = vm.PaymentMethod ?? "ventanilla",
                Notes = vm.Notes,
                CreatedAt = DateTime.UtcNow
            };

            if ((vm.PaymentMethod ?? "ventanilla") == "online")
            {
                if (total <= 0)
                    return Json(new { success = false, message = "Por favor selecciona una experiencia para procesar el pago." });

                reservation.Status = "Pendiente de Pago";
                _db.Reservations.Add(reservation);
                _db.SaveChanges();

                var checkoutUrl = CreateMpPreference(reservation, plan, total);
                if (checkoutUrl == null)
                {
                    reservation.Status = "Pendiente";
                    reservation.PaymentMethod = "ventanilla";
                    _db.SaveChanges();
                    return Json(new { success = false, message = "No se pudo conectar con el sistema de pago. Por favor elige pago en ventanilla o intenta de nuevo." });
                }

                _db.SaveChanges();
                return Json(new { success = true, redirectUrl = checkoutUrl });
            }

            // Pago en ventanilla
            reservation.Status = "Pendiente";
            _db.Reservations.Add(reservation);
            _db.SaveChanges();

            var reservationNumber = "RANCHO-" + reservation.Id.ToString("D4");
            SendConfirmationEmail(reservation, plan, total, reservationNumber);

            return Json(new
            {
                success = true,
                message = "¡Reservación recibida! Presenta tu número de reservación al llegar y paga en ventanilla.",
                reservationNumber,
                total,
                paymentMethod = "ventanilla"
            });
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult PaymentWebhook()
        {
            string paymentId = null;
            string notificationType = Request.QueryString["type"] ?? Request.QueryString["topic"];

            try
            {
                Request.InputStream.Seek(0, SeekOrigin.Begin);
                var body = new StreamReader(Request.InputStream).ReadToEnd();
                if (!string.IsNullOrWhiteSpace(body))
                {
                    var notification = JsonConvert.DeserializeObject<dynamic>(body);
                    if (notificationType == null && notification?.type != null)
                        notificationType = (string)notification.type;
                    if (notification?.data?.id != null)
                        paymentId = notification.data.id.ToString();
                }
            }
            catch { /* ignore body parse errors */ }

            if (paymentId == null)
                paymentId = Request.QueryString["data.id"] ?? Request.QueryString["id"];

            if (string.IsNullOrEmpty(paymentId))
                return new HttpStatusCodeResult(200);

            var accessToken = _db.SiteSettings.FirstOrDefault(s => s.Key == "MercadoPagoAccessToken")?.Value ?? "";
            if (string.IsNullOrEmpty(accessToken) || accessToken == "REEMPLAZAR")
                return new HttpStatusCodeResult(200);

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                using (var client = new WebClient())
                {
                    client.Headers["Authorization"] = "Bearer " + accessToken;
                    var response = client.DownloadString("https://api.mercadopago.com/v1/payments/" + paymentId);
                    var payment = JsonConvert.DeserializeObject<dynamic>(response);

                    string status = payment.status;
                    string externalRef = payment.external_reference;

                    if (status == "approved" && !string.IsNullOrEmpty(externalRef))
                    {
                        int reservationId;
                        if (int.TryParse(externalRef, out reservationId))
                        {
                            var reservation = _db.Reservations.Find(reservationId);
                            if (reservation != null && reservation.Status == "Pendiente de Pago")
                            {
                                reservation.Status = "Confirmada";
                                _db.SaveChanges();

                                var relatedPlan = reservation.PlanId.HasValue ? _db.Plans.Find(reservation.PlanId.Value) : null;
                                var relatedTotal = relatedPlan != null ? relatedPlan.Price * reservation.NumberOfCars : 0m;
                                var rNum = "RANCHO-" + reservation.Id.ToString("D4");
                                SendConfirmationEmail(reservation, relatedPlan, relatedTotal, rNum);
                            }
                        }
                    }
                }
            }
            catch { /* never break the MP webhook response */ }

            return new HttpStatusCodeResult(200);
        }

        public ActionResult PaymentSuccess()
        {
            var externalRef = Request.QueryString["external_reference"];
            Reservation reservation = null;
            if (!string.IsNullOrEmpty(externalRef))
            {
                int id;
                if (int.TryParse(externalRef, out id))
                    reservation = _db.Reservations.Find(id);
            }
            ViewBag.ReservationNumber = reservation != null ? "RANCHO-" + reservation.Id.ToString("D4") : "";
            ViewBag.ContactName = reservation != null ? reservation.ContactName : "";
            return View();
        }

        public ActionResult PaymentFailure()
        {
            return View();
        }

        public ActionResult PaymentPending()
        {
            var externalRef = Request.QueryString["external_reference"];
            Reservation reservation = null;
            if (!string.IsNullOrEmpty(externalRef))
            {
                int id;
                if (int.TryParse(externalRef, out id))
                    reservation = _db.Reservations.Find(id);
            }
            ViewBag.ReservationNumber = reservation != null ? "RANCHO-" + reservation.Id.ToString("D4") : "";
            return View();
        }

        private string CreateMpPreference(Reservation reservation, Plan plan, decimal total)
        {
            var accessToken = _db.SiteSettings.FirstOrDefault(s => s.Key == "MercadoPagoAccessToken")?.Value ?? "";
            if (string.IsNullOrEmpty(accessToken) || accessToken == "REEMPLAZAR") return null;

            var baseUrl = (_db.SiteSettings.FirstOrDefault(s => s.Key == "SiteBaseUrl")?.Value ?? "https://ranchoelpato.somee.com").TrimEnd('/');

            var payload = new
            {
                items = new[] {
                    new {
                        title = plan != null ? "Rancho El Pato — " + plan.Title : "Reservación Rancho El Pato",
                        quantity = 1,
                        unit_price = (double)total,
                        currency_id = "MXN"
                    }
                },
                payer = new
                {
                    name = reservation.ContactName,
                    email = reservation.Email
                },
                back_urls = new
                {
                    success = baseUrl + "/Contact/PaymentSuccess",
                    failure = baseUrl + "/Contact/PaymentFailure",
                    pending = baseUrl + "/Contact/PaymentPending"
                },
                auto_return = "approved",
                notification_url = baseUrl + "/Contact/PaymentWebhook",
                external_reference = reservation.Id.ToString()
            };

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.Headers["Authorization"] = "Bearer " + accessToken;
                    var response = client.UploadString(
                        "https://api.mercadopago.com/checkout/preferences",
                        JsonConvert.SerializeObject(payload));
                    var result = JsonConvert.DeserializeObject<dynamic>(response);
                    reservation.MercadoPagoPreferenceId = (string)result.id;
                    return (string)result.init_point;
                }
            }
            catch { return null; }
        }

        private void SendConfirmationEmail(Reservation reservation, Plan plan, decimal total, string reservationNumber)
        {
            if (string.IsNullOrEmpty(reservation.Email)) return;

            var allSettings = _db.SiteSettings.Where(s => s.Group == "Pagos").ToList();
            string bankAccount = "", bankName = "", bankCLABE = "";
            foreach (var bs in allSettings)
            {
                if (bs.Key == "BankAccount") bankAccount = bs.Value ?? "";
                else if (bs.Key == "BankName") bankName = bs.Value ?? "";
                else if (bs.Key == "BankCLABE") bankCLABE = bs.Value ?? "";
            }

            var planName = plan != null ? plan.Title : "—";
            var fecha = reservation.PreferredDate.HasValue ? reservation.PreferredDate.Value.ToString("dd/MM/yyyy") : "—";
            bool isPaid = reservation.PaymentMethod == "online";
            var payLabel = isPaid ? "Pago en Línea (Confirmado)" : "Pago en Ventanilla (Efectivo)";

            var body = new StringBuilder();
            body.Append("<div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;'>");
            body.Append("<div style='background:#2D5016;padding:30px;text-align:center;'>");
            body.Append("<h1 style='color:white;margin:0;'>Rancho El Pato</h1>");
            body.AppendFormat("<p style='color:#C4882A;margin:5px 0 0;font-size:1.1em;'>{0}</p>",
                isPaid ? "¡Pago Confirmado!" : "Confirmación de Reservación");
            body.Append("</div>");
            body.Append("<div style='padding:30px;background:#fff;'>");
            body.AppendFormat("<p>Hola <strong>{0}</strong>, {1}</p>",
                reservation.ContactName,
                isPaid ? "tu pago fue aprobado. ¡Ya tienes tu lugar reservado!" : "recibimos tu solicitud de reservación.");

            body.Append("<div style='background:#f0f9f0;border:2px solid #2D5016;border-radius:8px;padding:16px;text-align:center;margin:16px 0;'>");
            body.Append("<p style='margin:0 0 4px;color:#666;font-size:0.9em;'>Número de reservación</p>");
            body.AppendFormat("<p style='margin:0;color:#2D5016;font-size:1.8em;font-weight:bold;letter-spacing:2px;'>{0}</p>", reservationNumber);
            body.Append("</div>");

            body.Append("<table style='width:100%;border-collapse:collapse;'>");
            body.AppendFormat("<tr><td style='padding:8px;border-bottom:1px solid #eee;color:#666;'>Experiencia</td><td style='padding:8px;border-bottom:1px solid #eee;'><strong>{0}</strong></td></tr>", planName);
            body.AppendFormat("<tr><td style='padding:8px;border-bottom:1px solid #eee;color:#666;'>Fecha</td><td style='padding:8px;border-bottom:1px solid #eee;'>{0}</td></tr>", fecha);
            body.AppendFormat("<tr><td style='padding:8px;border-bottom:1px solid #eee;color:#666;'>Autos</td><td style='padding:8px;border-bottom:1px solid #eee;'>{0}</td></tr>", reservation.NumberOfCars);
            body.AppendFormat("<tr><td style='padding:8px;border-bottom:1px solid #eee;color:#666;'>Personas</td><td style='padding:8px;border-bottom:1px solid #eee;'>{0}</td></tr>", reservation.GuestCount > 0 ? reservation.GuestCount.ToString() : "—");
            body.AppendFormat("<tr><td style='padding:8px;border-bottom:1px solid #eee;color:#666;'>Pago</td><td style='padding:8px;border-bottom:1px solid #eee;'>{0}</td></tr>", payLabel);
            body.AppendFormat("<tr><td style='padding:8px;color:#666;font-weight:bold;'>Total</td><td style='padding:8px;color:#2D5016;font-size:1.2em;font-weight:bold;'>${0:N0} MXN</td></tr>", total);
            body.Append("</table>");

            if (!isPaid && (!string.IsNullOrEmpty(bankAccount) || !string.IsNullOrEmpty(bankCLABE)))
            {
                body.Append("<div style='margin-top:20px;padding:15px;background:#f7f4ee;border-left:4px solid #2D5016;'>");
                body.Append("<p style='margin:0 0 8px;font-weight:bold;'>Datos para depósito / transferencia:</p>");
                if (!string.IsNullOrEmpty(bankName)) body.AppendFormat("<p style='margin:2px 0;'>Banco: {0}</p>", bankName);
                if (!string.IsNullOrEmpty(bankAccount)) body.AppendFormat("<p style='margin:2px 0;'>Cuenta: {0}</p>", bankAccount);
                if (!string.IsNullOrEmpty(bankCLABE)) body.AppendFormat("<p style='margin:2px 0;'>CLABE: {0}</p>", bankCLABE);
                body.Append("</div>");
            }

            var closing = isPaid
                ? "<p style='margin-top:20px;color:#2D5016;font-weight:bold;'>¡Nos vemos en el rancho!</p>"
                : "<p style='margin-top:20px;color:#666;'>Nos pondremos en contacto para confirmar tu visita. Presenta tu número de reservación al llegar.</p>";
            body.Append(closing);
            body.Append("</div>");
            body.Append("<div style='background:#111;padding:15px;text-align:center;color:#999;font-size:12px;'>Rancho El Pato — Experiencias únicas en contacto con la naturaleza</div>");
            body.Append("</div>");

            var subject = isPaid ? "¡Pago Confirmado! Reservación en Rancho El Pato" : "Confirmación de Reservación — Rancho El Pato";
            EmailService.Send(reservation.Email, subject, body.ToString());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
