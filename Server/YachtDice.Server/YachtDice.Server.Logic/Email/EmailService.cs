using System;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Net.Mail;

namespace YachtDice.Server.Logic.Email
{
    /// <summary>
    /// Sends the application's emails through SMTP.
    /// </summary>
    public class EmailService : IEmailService
    {
        private const string SmtpHostKey = "SmtpHost";
        private const string SmtpPortKey = "SmtpPort";
        private const string SmtpUserKey = "SmtpUser";
        private const string SmtpPasswordKey = "SmtpPassword";
        private const int SmtpTimeoutMilliseconds = 10000;
        private const string NullMessageMessage = "The email to send cannot be null.";
        private const string EmptyRecipientMessage = "The recipient address cannot be empty.";
        private const string MissingSettingMessage = "The setting '{0}' is missing in the configuration file.";

        /// <summary>
        /// Sends a plain-text email to its recipient.
        /// </summary>
        /// <param name="message">Email to send, with its recipient, subject and body.</param>
        /// <exception cref="ArgumentNullException">The email is null.</exception>
        /// <exception cref="ArgumentException">The recipient address is empty.</exception>
        public void Send(EmailMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message), NullMessageMessage);
            }

            if (string.IsNullOrWhiteSpace(message.ToAddress))
            {
                throw new ArgumentException(EmptyRecipientMessage, nameof(message));
            }

            string host = ReadSetting(SmtpHostKey);
            int port = int.Parse(ReadSetting(SmtpPortKey), CultureInfo.InvariantCulture);
            string user = ReadSetting(SmtpUserKey);
            string password = ReadSetting(SmtpPasswordKey);

            using (var mail = new MailMessage(user, message.ToAddress, message.Subject, message.Body))
            {
                using (var client = new SmtpClient(host, port))
                {
                    client.EnableSsl = true;
                    client.Timeout = SmtpTimeoutMilliseconds;
                    client.Credentials = new NetworkCredential(user, password);

                    client.Send(mail);
                }
            }
        }

        private static string ReadSetting(string key)
        {
            string value = ConfigurationManager.AppSettings[key];

            if (string.IsNullOrWhiteSpace(value))
            {
                string message = string.Format(CultureInfo.InvariantCulture, MissingSettingMessage, key);

                throw new InvalidOperationException(message);
            }

            return value;
        }
    }
}