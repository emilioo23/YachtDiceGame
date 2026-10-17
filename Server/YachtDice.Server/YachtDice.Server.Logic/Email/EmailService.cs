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
        private const string EmptyRecipientMessage = "The recipient address cannot be empty.";
        private const string MissingSettingMessage = "The setting '{0}' is missing in the configuration file.";

        /// <summary>
        /// Sends a plain-text email to the given recipient.
        /// </summary>
        /// <param name="toAddress">Recipient's email address.</param>
        /// <param name="subject">Subject of the email.</param>
        /// <param name="body">Plain-text body of the email.</param>
        public void Send(string toAddress, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(toAddress))
            {
                throw new ArgumentException(EmptyRecipientMessage, nameof(toAddress));
            }

            string host = ReadSetting(SmtpHostKey);
            int port = int.Parse(ReadSetting(SmtpPortKey), CultureInfo.InvariantCulture);
            string user = ReadSetting(SmtpUserKey);
            string password = ReadSetting(SmtpPasswordKey);

            using (var message = new MailMessage(user, toAddress, subject, body))
            {
                using (var client = new SmtpClient(host, port))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(user, password);

                    client.Send(message);
                }
            }
        }

        private static string ReadSetting(string key)
        {
            string value = ConfigurationManager.AppSettings[key];

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, MissingSettingMessage, key));
            }

            return value;
        }
    }
}