using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace RanchoMvc.Services
{
    public static class EmailService
    {
        public static void Send(string to, string subject, string htmlBody)
        {
            var host = ConfigurationManager.AppSettings["SmtpHost"];
            if (string.IsNullOrEmpty(host) || host == "REEMPLAZAR") return;

            var portStr = ConfigurationManager.AppSettings["SmtpPort"];
            int port;
            if (!int.TryParse(portStr, out port)) { port = 587; }

            var user = ConfigurationManager.AppSettings["SmtpUser"] ?? "";
            var pass = ConfigurationManager.AppSettings["SmtpPass"] ?? "";
            var from = ConfigurationManager.AppSettings["SmtpFrom"] ?? user;

            try
            {
                using (var client = new SmtpClient(host, port))
                {
                    client.EnableSsl = false;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.Credentials = new NetworkCredential(user, pass);
                    var msg = new MailMessage(from, to, subject, htmlBody) { IsBodyHtml = true };
                    client.Send(msg);
                }
            }
            catch (Exception)
            {
                // Si el email falla no rompemos el flujo de la reservación
            }
        }

        public static void SendReservationStatus(string toEmail, string contactName,
            string groupName, string newStatus, int reservationId)
        {
            string siteName = ConfigurationManager.AppSettings["SiteName"] ?? "Rancho Experience";
            string accentColor = newStatus == "Confirmada" ? "#2D5016" : "#991b1b";
            string icon = newStatus == "Confirmada" ? "✅" : "❌";

            string subject = newStatus == "Confirmada"
                ? $"{icon} Tu cotización ha sido confirmada — {siteName}"
                : $"{icon} Actualización de tu cotización — {siteName}";

            string bodyMsg = newStatus == "Confirmada"
                ? $"¡Buenas noticias! Tu cotización <strong>#{reservationId}</strong> para <strong>{groupName}</strong> ha sido <strong>confirmada</strong>. En breve nos pondremos en contacto contigo para coordinar los detalles."
                : $"Te informamos que tu cotización <strong>#{reservationId}</strong> para <strong>{groupName}</strong> ha sido <strong>cancelada</strong>. Si tienes alguna duda, no dudes en contactarnos.";

            string html = $@"
<!DOCTYPE html>
<html lang='es'>
<head><meta charset='UTF-8'/></head>
<body style='margin:0;padding:0;background:#f4f6f4;font-family:Inter,Arial,sans-serif'>
  <table width='100%' cellpadding='0' cellspacing='0'>
    <tr><td align='center' style='padding:40px 16px'>
      <table width='560' cellpadding='0' cellspacing='0' style='background:white;border-radius:14px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,.08)'>
        <tr><td style='background:{accentColor};padding:28px 32px;text-align:center'>
          <h1 style='color:white;margin:0;font-size:22px;font-weight:700'>{siteName}</h1>
          <p style='color:rgba(255,255,255,.75);margin:6px 0 0;font-size:13px'>Actualización de cotización</p>
        </td></tr>
        <tr><td style='padding:32px'>
          <p style='margin:0 0 16px;color:#374151;font-size:15px'>Hola <strong>{contactName}</strong>,</p>
          <p style='margin:0 0 24px;color:#4b5563;font-size:14px;line-height:1.7'>{bodyMsg}</p>
          <div style='background:#f9fafb;border-radius:10px;padding:16px 20px;margin-bottom:24px;border:1px solid #e5e7eb'>
            <p style='margin:0;color:#9ca3af;font-size:11px;text-transform:uppercase;letter-spacing:.08em'>Número de cotización</p>
            <p style='margin:4px 0 0;color:{accentColor};font-size:22px;font-weight:800'>#{reservationId}</p>
          </div>
          <p style='margin:0;color:#9ca3af;font-size:12px'>Si tienes preguntas, responde a este correo o contáctanos directamente.</p>
        </td></tr>
        <tr><td style='background:#f9fafb;padding:16px 32px;text-align:center;border-top:1px solid #e5e7eb'>
          <p style='margin:0;color:#9ca3af;font-size:11px'>© {DateTime.Now.Year} {siteName} · Todos los derechos reservados</p>
        </td></tr>
      </table>
    </td></tr>
  </table>
</body>
</html>";

            Send(toEmail, subject, html);
        }
    }
}
