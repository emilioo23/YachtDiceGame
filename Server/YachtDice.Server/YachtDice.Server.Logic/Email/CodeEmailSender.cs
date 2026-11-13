using System;
using System.Globalization;
using System.Net.Mail;
using YachtDice.Server.Data;
using YachtDice.Server.Logic.Logging;
using YachtDice.Server.Logic.Resources;

namespace YachtDice.Server.Logic.Email
{
    /// <summary>
    /// Sends by email a temporary code, such as a verification code or a password recovery code, with the texts
    /// of a template in the language of the player. A failure of the delivery is written in the log and reported
    /// to the caller, so the player can ask for the code again.
    /// </summary>
    public class CodeEmailSender
    {
        private const string SentLogFormat = "The email {0} was sent.";
        private const string SendFailedLogFormat = "The email {0} could not be sent.";
        private const string NullEmailServiceMessage = "The email service cannot be null.";
        private const string NullTemplateMessage = "The template of the email cannot be null.";
        private const string NullArgumentMessage = "The value cannot be null.";

        private readonly IEmailService _emailService;
        private readonly CodeEmailTemplate _template;
        private readonly AppLogger _logger;

        /// <summary>
        /// Initializes the sender for one kind of email.
        /// </summary>
        /// <param name="emailService">Service that delivers the emails.</param>
        /// <param name="template">Texts of the email and the minutes that the code lasts.</param>
        public CodeEmailSender(IEmailService emailService, CodeEmailTemplate template)
        {
            _emailService = emailService
                ?? throw new ArgumentNullException(nameof(emailService), NullEmailServiceMessage);
            _template = template
                ?? throw new ArgumentNullException(nameof(template), NullTemplateMessage);
            _logger = new AppLogger(GetType());
        }

        /// <summary>
        /// Sends the code to the email of the player.
        /// </summary>
        /// <param name="player">Player who receives the code.</param>
        /// <param name="code">Code to send.</param>
        /// <param name="culture">Language of the email.</param>
        /// <returns>True if the email was delivered to the mail server.</returns>
        /// <exception cref="ArgumentNullException">The player or the culture is null.</exception>
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
            string subject = EmailStrings.ResourceManager.GetString(_template.SubjectResourceKey, culture);
            var message = new EmailMessage
            {
                ToAddress = player.Email,
                Subject = subject,
                Body = BuildBody(player.DisplayName, code, culture)
            };

            try
            {
                _emailService.Send(message);
                isSent = true;
                _logger.Info(DescribeEmail(SentLogFormat));
            }
            catch (Exception ex) when (IsDeliveryFailure(ex))
            {
                _logger.Error(ex, DescribeEmail(SendFailedLogFormat));
            }

            return isSent;
        }

        private static bool IsDeliveryFailure(Exception exception)
        {
            bool isMailFailure = exception is SmtpException || exception is InvalidOperationException;

            return isMailFailure || exception is FormatException || exception is ArgumentException;
        }

        private string BuildBody(string playerDisplayName, string code, CultureInfo culture)
        {
            string template = EmailStrings.ResourceManager.GetString(_template.BodyResourceKey, culture);

            return string.Format(culture, template, playerDisplayName, code, _template.ExpirationMinutes);
        }

        private string DescribeEmail(string logFormat)
        {
            return string.Format(CultureInfo.InvariantCulture, logFormat, _template.SubjectResourceKey);
        }
    }
}
