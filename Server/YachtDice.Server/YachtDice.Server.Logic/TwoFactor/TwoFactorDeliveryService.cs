using System;
using System.Globalization;
using YachtDice.Server.Data;
using YachtDice.Server.Logic.Email;

namespace YachtDice.Server.Logic.TwoFactor
{
    /// <summary>
    /// Generates a verification code and sends it to the player's email.
    /// </summary>
    public class TwoFactorDeliveryService
    {
        private const string SubjectResourceKey = "TwoFactor_EmailSubject";
        private const string BodyResourceKey = "TwoFactor_EmailBody";
        private const string NullCodeServiceMessage = "The code service cannot be null.";
        private const string NullArgumentMessage = "The value cannot be null.";

        private static readonly CodeEmailTemplate EmailTemplate =
            new CodeEmailTemplate(SubjectResourceKey, BodyResourceKey, TwoFactorCodeService.ExpirationMinutes);

        private readonly TwoFactorCodeService _codeService;
        private readonly CodeEmailSender _emailSender;

        /// <summary>
        /// Initializes the service with the code generator shared with the validation.
        /// </summary>
        /// <param name="codeService">Service that generates and validates the codes.</param>
        /// <param name="emailService">Service that sends the emails.</param>
        public TwoFactorDeliveryService(TwoFactorCodeService codeService, IEmailService emailService)
        {
            _codeService = codeService ?? throw new ArgumentNullException(nameof(codeService), NullCodeServiceMessage);
            _emailSender = new CodeEmailSender(emailService, EmailTemplate);
        }

        /// <summary>
        /// Generates a new code and sends it to the given email synchronously.
        /// </summary>
        /// <param name="player">Player who receives the code. Its email is the recipient.</param>
        /// <param name="challengeId">Unique session challenge identifier.</param>
        /// <param name="culture">Language in which the email is written.</param>
        /// <returns>True if the email was sent successfully.</returns>
        /// <exception cref="ArgumentNullException">The player or the culture is null.</exception>
        public bool SendCode(Player player, string challengeId, CultureInfo culture)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player), NullArgumentMessage);
            }

            string code = _codeService.GenerateCode(player.Email, challengeId);

            return _emailSender.Send(player, code, culture);
        }
    }
}
