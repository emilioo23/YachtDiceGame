using System;
using System.Globalization;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Authentication;
using YachtDice.Server.Logic.Players;

namespace YachtDice.Server.Logic.TwoFactor
{
    /// <summary>
    /// Validates the verification codes and sends them again when the player asks for it.
    /// </summary>
    public class TwoFactorVerificationService : IVerificationService
    {
        private const string NullDeliveryMessage = "The delivery data of the code cannot be null.";
        private const string NullDependencyMessage = "A dependency of the two-factor verification cannot be null.";

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
            _playerRepository = playerRepository
                ?? throw new ArgumentNullException(nameof(playerRepository), NullDependencyMessage);
            _codeService = codeService ?? throw new ArgumentNullException(nameof(codeService), NullDependencyMessage);
            _deliveryService = deliveryService
                ?? throw new ArgumentNullException(nameof(deliveryService), NullDependencyMessage);
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
        /// <param name="verificationCode">The code provided and the session challenge it answers.</param>
        /// <returns>Result status of the verification process.</returns>
        public TwoFactorValidationStatus VerifyPlayerCode(string email, VerificationCode verificationCode)
        {
            TwoFactorValidationStatus status = _codeService.ValidateCode(email, verificationCode);

            if (status == TwoFactorValidationStatus.Valid)
            {
                ActivateIfUnverified(email);
            }

            return status;
        }

        private void ActivateIfUnverified(string email)
        {
            Player player = _playerRepository.FindByEmail(email).FirstOrDefault();

            if (player != null && player.State == PlayerState.Unverified.ToString())
            {
                _playerRepository.SaveState(player.Id, PlayerState.Active.ToString());
            }
        }

        /// <summary>
        /// Processes the code resend required by the interface.
        /// </summary>
        /// <param name="email">The player's email address.</param>
        /// <param name="delivery">The session challenge and the culture name for email localization.</param>
        /// <returns>
        /// Result status of the resend process. A code is only sent when the challenge was issued to that email;
        /// for any other email the answer is the same as for a sent code, so the screen cannot be used to find
        /// out which emails have an account (section 8.8 of the standard).
        /// </returns>
        /// <exception cref="ArgumentNullException">The delivery data is null.</exception>
        public ResendCodeStatus ResendPlayerCode(string email, CodeDelivery delivery)
        {
            if (delivery == null)
            {
                throw new ArgumentNullException(nameof(delivery), NullDeliveryMessage);
            }

            Player player = _playerRepository.FindByEmail(email).FirstOrDefault();
            ResendCodeStatus status = ResendCodeStatus.CodeSent;

            if (IsOwnerOfTheChallenge(player, delivery))
            {
                status = ResendTo(player, delivery);
            }

            return status;
        }

        private static bool IsOwnerOfTheChallenge(Player player, CodeDelivery delivery)
        {
            return player != null
                && !string.IsNullOrEmpty(delivery.OwnerName)
                && string.Equals(player.DisplayName, delivery.OwnerName, StringComparison.Ordinal);
        }

        private ResendCodeStatus ResendTo(Player player, CodeDelivery delivery)
        {
            ResendCodeStatus status;

            if (_codeService.IsLocked(player.Email))
            {
                status = ResendCodeStatus.VerificationLocked;
            }
            else if (!_codeService.IsResendAllowed(player.Email, delivery.ChallengeId))
            {
                status = ResendCodeStatus.ResendTooSoon;
            }
            else
            {
                var culture = new CultureInfo(delivery.CultureName);
                bool isSent = _deliveryService.SendCode(player, delivery.ChallengeId, culture);
                status = isSent ? ResendCodeStatus.CodeSent : ResendCodeStatus.CodeSendFailed;
            }

            return status;
        }
    }
}