using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Authentication;

namespace YachtDice.Server.Logic.TwoFactor
{
    /// <summary>
    /// Validates the verification codes and sends them again when the player asks for it.
    /// </summary>
    public class TwoFactorVerificationService : IVerificationService
    {
        private const string NullDependencyMessage = "The dependency cannot be null.";

        private readonly IPlayerRepository _playerRepository;
        private readonly TwoFactorCodeService _codeService;
        private readonly TwoFactorDeliveryService _deliveryService;

        /// <summary>
        /// Initializes the service with the components it needs.
        /// </summary>
        /// <param name="playerRepository">Repository used to read players.</param>
        /// <param name="codeService">Service that keeps the verification codes.</param>
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
        /// Indicates whether verification is locked for the player because of too many failed attempts.
        /// </summary>
        /// <param name="email">The player's email address.</param>
        /// <returns>True if the lockout is still active.</returns>
        public bool IsLocked(string email)
        {
            return _codeService.IsLocked(email);
        }

        /// <summary>
        /// Processes the code verification required by the interface.
        /// </summary>
        /// <param name="email">The player's email address.</param>
        /// <param name="code">The verification code provided.</param>
        /// <param name="challengeId">The session challenge identifier.</param>
        /// <returns>Result status of the verification process.</returns>
        public TwoFactorValidationStatus VerifyPlayerCode(string email, string code, string challengeId)
        {
            return _codeService.Validate(email, code, challengeId);
        }

        /// <summary>
        /// Processes the code resend required by the interface.
        /// </summary>
        /// <param name="email">The player's email address.</param>
        /// <param name="cultureName">The culture name for email localization.</param>
        /// <param name="challengeId">The session challenge identifier.</param>
        /// <returns>Result status of the resend process.</returns>
        public ResendCodeStatus ResendPlayerCode(string email, string cultureName, string challengeId)
        {
            List<Player> matches = _playerRepository.FindByEmail(email);
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
                var culture = new CultureInfo(cultureName);
                bool wasSent = _deliveryService.SendCode(player, culture, challengeId);
                status = wasSent ? ResendCodeStatus.CodeSent : ResendCodeStatus.CodeSendFailed;
            }

            return status;
        }
    }
}