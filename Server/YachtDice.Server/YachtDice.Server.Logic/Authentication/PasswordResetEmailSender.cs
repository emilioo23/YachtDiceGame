using System;
using System.Globalization;
using System.Net.Mail;
using YachtDice.Server.Data;
using YachtDice.Server.Logic.Email;
using YachtDice.Server.Logic.Logging;
using YachtDice.Server.Logic.Resources;

namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Writes the email with the password recovery code and sends it to the player.
    /// </summary>
    public class PasswordResetEmailSender
    {
        private const string SubjectResourceKey = "PasswordReset_EmailSubject";
        private const string BodyResourceKey = "PasswordReset_EmailBody";
        private const string SentLogMessage = "Password recovery code sent by email.";
        private const string SendFailedLogMessage = "The password recovery code could not be sent.";
        private const string NullEmailServiceMessage = "The email service cannot be null.";
        private const string NullArgumentMessage = "The value cannot be null.";

        private readonly AppLogger _logger;
        private readonly IEmailService _emailService;

        /// <summary>
        /// Initializes the sender with the service that delivers the emails.
        /// </summary>
        /// <param name="emailService">Service that sends the emails.</param>
        public PasswordResetEmailSender(IEmailService emailService)
        {
            _emailService = emailService
                ?? throw new ArgumentNullException(nameof(emailService), NullEmailServiceMessage);
            _logger = new AppLogger(GetType());
        }

        /// <summary>
        /// Sends the recovery code to the email of the player.
        /// </summary>
        /// <param name="player">Player who receives the code. Its email is the recipient.</param>
        /// <param name="code">Recovery code in plain text.</param>
        /// <param name="culture">Language in which the email is written.</param>
        /// <returns>True if the email was sent successfully.</returns>
        public bool Send(Player player, string code, CultureInfo culture)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player), NullArgumentMessage);
            }

            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture), NullArgumentMessage);
            }

            var isSent = false;
            string subject = EmailStrings.ResourceManager.GetString(SubjectResourceKey, culture);
            string body = BuildBody(player.DisplayName, code, culture);

            try
            {
                _emailService.Send(player.Email, subject, body);
                isSent = true;
                _logger.Info(SentLogMessage);
            }
            catch (Exception ex) when (IsDeliveryFailure(ex))
            {
                _logger.Error(ex, SendFailedLogMessage);
            }

            return isSent;
        }

        private static bool IsDeliveryFailure(Exception exception)
        {
            return exception is SmtpException
                || exception is InvalidOperationException
                || exception is FormatException
                || exception is ArgumentException;
        }

        private static string BuildBody(string playerDisplayName, string code, CultureInfo culture)
        {
            string template = EmailStrings.ResourceManager.GetString(BodyResourceKey, culture);

            return string.Format(
                culture,
                template,
                playerDisplayName,
                code,
                PasswordRecoveryService.ExpirationMinutes);
        }
    }
}
