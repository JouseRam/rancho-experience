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
    }
}
