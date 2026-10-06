using System;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace YachtDice.Services
{
    /// <summary>
    /// Envía correos electrónicos de la aplicación por SMTP.
    /// </summary>
    public class EmailService
    {
        private const string SmtpHostKey = "SmtpHost";
        private const string SmtpPortKey = "SmtpPort";
        private const string SmtpUserKey = "SmtpUser";
        private const string SmtpPasswordKey = "SmtpPassword";

        /// <summary>
        /// Envía un correo de texto plano al destinatario indicado.
        /// </summary>
        /// <param name="toAddress">Correo del destinatario.</param>
        /// <param name="subject">Asunto del correo.</param>
        /// <param name="body">Cuerpo del correo en texto plano.</param>
        public async Task SendAsync(string toAddress, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(toAddress))
            {
                throw new ArgumentException("The recipient address cannot be empty.", nameof(toAddress));
            }

            string host = ConfigurationManager.AppSettings[SmtpHostKey];
            int port = int.Parse(ConfigurationManager.AppSettings[SmtpPortKey], CultureInfo.InvariantCulture);
            string user = ConfigurationManager.AppSettings[SmtpUserKey];
            string password = ConfigurationManager.AppSettings[SmtpPasswordKey];

            using (var message = new MailMessage(user, toAddress, subject, body))
            {
                using (var client = new SmtpClient(host, port))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(user, password);
                    await client.SendMailAsync(message);
                }
            }
        }
    }
}