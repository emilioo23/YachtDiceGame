using System;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace YachtDice.Services
{
    /// <summary>
    /// Sends the application's emails through SMTP.
    /// </summary>
    public class EmailService
    {
        private const string SmtpHostKey = "SmtpHost";
        private const string SmtpPortKey = "SmtpPort";
        private const string SmtpUserKey = "SmtpUser";
        private const string SmtpPasswordKey = "SmtpPassword";

        /// <summary>
        /// Sends a plain-text email to the given recipient.
        /// </summary>
        /// <param name="toAddress">Recipient's email address.</param>
        /// <param name="subject">Subject of the email.</param>
        /// <param name="body">Plain-text body of the email.</param>
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