using System;
using System.Globalization;
using System.Net.Mail;
using YachtDice.Server.Data;
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

        private readonly AppLogger _logger;
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
            _logger = new AppLogger(GetType());
        }

        /// <summary>
        /// Generates a new code and sends it to the given email synchronously.
        /// </summary>
        /// <param name="player">Player who receives the code. Its email is the recipient.</param>
        /// <param name="culture">Language in which the email is written.</param>
        /// <param name="challengeId">Unique session challenge identifier.</param>
        /// <returns>True if the email was sent successfully.</returns>
        public bool SendCode(Player player, CultureInfo culture, string challengeId)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            var wasSent = false;
            string code = _codeService.GenerateCode(player.Email, challengeId);
            string subject = EmailStrings.ResourceManager.GetString(SubjectResourceKey, culture);
            string body = BuildBody(player.DisplayName, code, culture);

            try
            {
                _emailService.Send(player.Email, subject, body);
                wasSent = true;
                _logger.Info(SentLogMessage);
            }
            catch (SmtpException ex)
            {
                LogSendFailure(ex);
            }
            catch (InvalidOperationException ex)
            {
                LogSendFailure(ex);
            }
            catch (FormatException ex)
            {
                LogSendFailure(ex);
            }
            catch (ArgumentException ex)
            {
                LogSendFailure(ex);
            }

            return wasSent;
        }

        private void LogSendFailure(Exception exception)
        {
            _logger.Error(exception, SendFailedLogMessage);
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