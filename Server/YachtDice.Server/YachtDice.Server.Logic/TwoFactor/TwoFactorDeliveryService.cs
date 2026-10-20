using System;
using System.Globalization;
using System.Net.Mail;
using YachtDice.Server.Logic.Email;
using YachtDice.Server.Logic.Logging;
using YachtDice.Server.Logic.Resources;

namespace YachtDice.Server.Logic.TwoFactor
{
    /// <summary>
    /// Generates a verification code and sends it to the player's email.
    /// </summary>
    public class TwoFactorDeliveryService
    {
        private const string SubjectResourceKey = "TwoFactor_EmailSubject";
        private const string BodyResourceKey = "TwoFactor_EmailBody";
        private const string SentLogMessage = "Verification code sent by email.";
        private const string SendFailedLogMessage = "The verification code could not be sent.";
        private const string NullCodeServiceMessage = "The code service cannot be null.";
        private const string NullEmailServiceMessage = "The email service cannot be null.";

        private readonly AppLogger _logger = new AppLogger(typeof(TwoFactorDeliveryService));
        private readonly TwoFactorCodeService _codeService;
        private readonly IEmailService _emailService;

        /// <summary>
        /// Initializes the service with the code generator shared with the validation.
        /// </summary>
        /// <param name="codeService">Service that generates and validates the codes.</param>
        /// <param name="emailService">Service that sends the emails.</param>
        public TwoFactorDeliveryService(TwoFactorCodeService codeService, IEmailService emailService)
        {
            if (codeService == null)
            {
                throw new ArgumentNullException(nameof(codeService), NullCodeServiceMessage);
            }

            if (emailService == null)
            {
                throw new ArgumentNullException(nameof(emailService), NullEmailServiceMessage);
            }

            _codeService = codeService;
            _emailService = emailService;
        }

        /// <summary>
        /// Generates a new code and sends it to the given email synchronously.
        /// </summary>
        /// <param name="email">Player's email address (recipient).</param>
        /// <param name="playerDisplayName">Player's name, used in the greeting.</param>
        /// <param name="culture">Language in which the email is written.</param>
        /// <param name="challengeId">Unique session challenge identifier.</param>
        /// <returns>True if the email was sent successfully.</returns>
        public bool SendCode(string email, string playerDisplayName, CultureInfo culture, string challengeId)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            var wasSent = false;
            string code = _codeService.GenerateCode(email, challengeId);
            string subject = EmailStrings.ResourceManager.GetString(SubjectResourceKey, culture);
            string body = BuildBody(playerDisplayName, code, culture);

            try
            {
                _emailService.Send(email, subject, body);
                wasSent = true;
                _logger.Info(SentLogMessage);
            }
            catch (SmtpException ex)
            {
                _logger.Error(ex, SendFailedLogMessage);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error(ex, SendFailedLogMessage);
            }
            catch (FormatException ex)
            {
                _logger.Error(ex, SendFailedLogMessage);
            }
            catch (ArgumentException ex)
            {
                _logger.Error(ex, SendFailedLogMessage);
            }

            return wasSent;
        }

        private static string BuildBody(string playerDisplayName, string code, CultureInfo culture)
        {
            string template = EmailStrings.ResourceManager.GetString(BodyResourceKey, culture);

            return string.Format(
                culture,
                template,
                playerDisplayName,
                code,
                TwoFactorCodeService.ExpirationMinutes);
        }
    }
}