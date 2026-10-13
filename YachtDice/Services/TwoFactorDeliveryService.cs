using System;
using System.Globalization;
using System.Net.Mail;
using System.Threading.Tasks;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Services
{
    /// <summary>
    /// Generates a verification code and sends it to the player's email.
    /// </summary>
    public class TwoFactorDeliveryService
    {
        private const string SentLogMessage = "Código de verificación enviado por correo.";
        private const string SendFailedLogMessage = "No se pudo enviar el código de verificación.";

        private readonly AppLogger _logger = new AppLogger(typeof(TwoFactorDeliveryService));
        private readonly EmailService _emailService = new EmailService();
        private readonly TwoFactorCodeService _codeService;

        /// <summary>
        /// Initializes the service with the code generator that is shared with the validation.
        /// </summary>
        /// <param name="codeService">Service that generates and validates the codes.</param>
        public TwoFactorDeliveryService(TwoFactorCodeService codeService)
        {
            if (codeService == null)
            {
                throw new ArgumentNullException(nameof(codeService), "The code service cannot be null.");
            }

            _codeService = codeService;
        }

        /// <summary>
        /// Generates a new code and sends it to the given email.
        /// </summary>
        /// <param name="email">Player's email address (recipient).</param>
        /// <param name="playerDisplayName">Player's name, used in the greeting.</param>
        /// <returns>True if the email was sent successfully.</returns>
        public async Task<bool> SendCodeAsync(string email, string playerDisplayName)
        {
            var wasSent = false;
            string code = _codeService.GenerateCode(email);

            string body = string.Format(
                CultureInfo.CurrentCulture,
                Strings.TwoFactor_EmailBody,
                playerDisplayName,
                code,
                TwoFactorCodeService.ExpirationMinutes);

            try
            {
                await _emailService.SendAsync(email, Strings.TwoFactor_EmailSubject, body);
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
    }
}