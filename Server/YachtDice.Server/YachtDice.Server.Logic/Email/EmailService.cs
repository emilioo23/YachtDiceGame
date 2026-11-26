using System;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using YachtDice.Server.Data.Logging;
using YachtDice.Server.Logic.Logging;

namespace YachtDice.Server.Logic.Email
{
    /// <summary>
    /// Sends the emails of the game and delivers them without stopping the operation when the mail server fails.
    /// </summary>
    public class EmailService : IEmailService
    {
        private const string SmtpHostKey = "SmtpHost";
        private const string SmtpPortKey = "SmtpPort";
        private const string SmtpUserVariable = "YACHTDICE_SMTP_USER";
        private const string SmtpPasswordVariable = "YACHTDICE_SMTP_PASSWORD";
        private const int SmtpTimeoutMilliseconds = 10000;

        /// <summary>
        /// Sends a plain-text email to its recipient.
        /// </summary>
        /// <param name="message">Email to send, with its recipient, subject and body.</param>
        /// <exception cref="ArgumentNullException">The email is null.</exception>
        /// <exception cref="ArgumentException">The recipient address is empty.</exception>
        /// <exception cref="InvalidOperationException">A setting or an environment variable is missing.</exception>
        public void Send(EmailMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message), LogMessages.EmailNullMessageMessage);
            }

            if (string.IsNullOrWhiteSpace(message.ToAddress))
            {
                throw new ArgumentException(LogMessages.EmptyRecipientMessage, nameof(message));
            }

            string host = ReadSetting(SmtpHostKey);
            int port = int.Parse(ReadSetting(SmtpPortKey), CultureInfo.InvariantCulture);
            string user = ReadVariable(SmtpUserVariable);
            string password = ReadVariable(SmtpPasswordVariable);

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

        /// <summary>
        /// Sends the email.
        /// </summary>
        /// <param name="emailService">Service that delivers the emails.</param>
        /// <param name="message">Email to send.</param>
        /// <param name="logger">Logger of the class that sends the email.</param>
        /// <returns>True if the email was delivered to the mail server.</returns>
        internal static bool TrySend(IEmailService emailService, EmailMessage message, AppLogger logger)
        {
            var isSent = false;

            try
            {
                emailService.Send(message);
                isSent = true;
            }
            catch (Exception ex) when (IsDeliveryFailure(ex))
            {
                logger.Error(ex, LogMessages.EmailSendFailedLogMessage);
            }

            return isSent;
        }

        private static string ReadSetting(string key)
        {
            string value = ConfigurationManager.AppSettings[key];

            if (string.IsNullOrWhiteSpace(value))
            {
                string message = string.Format(CultureInfo.InvariantCulture, LogMessages.MissingSettingMessage, key);

                throw new InvalidOperationException(message);
            }

            return value;
        }

        private static string ReadVariable(string name)
        {
            string value = Environment.GetEnvironmentVariable(name);

            if (string.IsNullOrWhiteSpace(value))
            {
                string message = string.Format(CultureInfo.InvariantCulture, LogMessages.MissingVariableFormat, name);

                throw new InvalidOperationException(message);
            }

            return value;
        }

        private static bool IsDeliveryFailure(Exception exception)
        {
            bool isMailFailure = exception is SmtpException || exception is InvalidOperationException;

            return isMailFailure || exception is FormatException || exception is ArgumentException;
        }
    }
}
