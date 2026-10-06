using System;
using System.Globalization;
using System.Net.Mail;
using System.Threading.Tasks;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Services
{
    /// <summary>
    /// Genera un código de verificación y lo envía al correo del jugador.
    /// </summary>
    public class TwoFactorDeliveryService
    {
        private const string SentLogMessage = "Código de verificación enviado por correo.";
        private const string SendFailedLogMessage = "No se pudo enviar el código de verificación.";

        private readonly EmailService _emailService = new EmailService();
        private readonly TwoFactorCodeService _codeService;

        /// <summary>
        /// Inicializa el servicio con el generador de códigos que se compartirá con la validación.
        /// </summary>
        /// <param name="codeService">Servicio que genera y valida los códigos.</param>
        public TwoFactorDeliveryService(TwoFactorCodeService codeService)
        {
            if (codeService == null)
            {
                throw new ArgumentNullException(nameof(codeService), "The code service cannot be null.");
            }

            _codeService = codeService;
        }

        /// <summary>
        /// Genera un código nuevo y lo envía al correo indicado.
        /// </summary>
        /// <param name="email">Correo del jugador (destinatario).</param>
        /// <param name="playerDisplayName">Nombre del jugador para el saludo.</param>
        /// <returns>True si el correo se envió correctamente.</returns>
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
                await AppLogger.InfoAsync(SentLogMessage);
            }
            catch (SmtpException ex)
            {
                await AppLogger.ErrorAsync(SendFailedLogMessage, ex);
            }
            catch (InvalidOperationException ex)
            {
                await AppLogger.ErrorAsync(SendFailedLogMessage, ex);
            }
            catch (FormatException ex)
            {
                await AppLogger.ErrorAsync(SendFailedLogMessage, ex);
            }
            catch (ArgumentException ex)
            {
                await AppLogger.ErrorAsync(SendFailedLogMessage, ex);
            }

            return wasSent;
        }
    }
}