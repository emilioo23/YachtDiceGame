using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;

namespace YachtDice.Server.Logic.TwoFactor
{
    /// <summary>
    /// Validates the verification codes and sends them again when the player asks for it.
    /// </summary>
    public class TwoFactorVerificationService
    {
        private const string NullDependencyMessage = "The dependency cannot be null.";

        private readonly IPlayerRepository _playerRepository;
        private readonly TwoFactorCodeService _codeService;
        private readonly TwoFactorDeliveryService _deliveryService;

        /// <summary>
        /// Initializes the service with the components it needs.
        /// </summary>
        /// <param name="playerRepository">Repository used to read players.</param>
        /// <param name="codeService">Service that keeps the verification codes (shared instance).</param>
        /// <param name="deliveryService">Service that sends the verification code by email.</param>
        public TwoFactorVerificationService(
            IPlayerRepository playerRepository,
            TwoFactorCodeService codeService,
            TwoFactorDeliveryService deliveryService)
        {
            if (playerRepository == null)
            {
                throw new ArgumentNullException(nameof(playerRepository), NullDependencyMessage);
            }

            if (codeService == null)
            {
                throw new ArgumentNullException(nameof(codeService), NullDependencyMessage);
            }

            if (deliveryService == null)
            {
                throw new ArgumentNullException(nameof(deliveryService), NullDependencyMessage);
            }

            _playerRepository = playerRepository;
            _codeService = codeService;
            _deliveryService = deliveryService;
        }

        /// <summary>
        /// Creates a new unique challenge identifier for a login session.
        /// </summary>
        /// <returns>The challenge string.</returns>
        public string CreateChallenge()
        {
            return _codeService.CreateNewChallengeId();
        }

        /// <summary>
        /// Validates the code entered by the player against the challenge.
        /// </summary>
        /// <param name="email">Player's email address.</param>
        /// <param name="code">Code entered by the player.</param>
        /// <param name="challengeId">Challenge identifier.</param>
        /// <returns>Validation result.</returns>
        public TwoFactorValidationResult VerifyCode(string email, string code, string challengeId)
        {
            return _codeService.Validate(email, code, challengeId);
        }

        /// <summary>
        /// Generates a new code and sends it, if the player is allowed to ask for one.
        /// </summary>
        /// <param name="email">Player's email address.</param>
        /// <param name="culture">Language in which the email is written.</param>
        /// <param name="challengeId">Challenge identifier.</param>
        /// <returns>Outcome of the request.</returns>
        public async Task<ResendCodeStatus> ResendCodeAsync(string email, CultureInfo culture, string challengeId)
        {
            List<Player> matches = await _playerRepository.FindByEmailAsync(email);
            Player player = matches.FirstOrDefault();
            ResendCodeStatus status;

            if (player == null)
            {
                status = ResendCodeStatus.PlayerNotFound;
            }
            else if (_codeService.IsLocked(player.Email))
            {
                status = ResendCodeStatus.VerificationLocked;
            }
            else if (!_codeService.IsResendAllowed(player.Email, challengeId))
            {
                status = ResendCodeStatus.ResendTooSoon;
            }
            else
            {
                bool wasSent = await _deliveryService.SendCodeAsync(player.Email, player.DisplayName, culture, challengeId);
                status = wasSent ? ResendCodeStatus.CodeSent : ResendCodeStatus.CodeSendFailed;
            }

            return status;
        }
    }
}