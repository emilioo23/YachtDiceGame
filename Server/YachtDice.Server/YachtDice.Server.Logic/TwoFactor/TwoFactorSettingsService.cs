using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Security;

namespace YachtDice.Server.Logic.TwoFactor
{
    /// <summary>
    /// Lets a player turn the two-step verification on or off.
    /// </summary>
    public class TwoFactorSettingsService : ITwoFactorSettingsService
    {
        private const int VisibleLocalPartChars = 2;
        private const char MaskCharacter = '*';
        private const char AtSign = '@';
        private const string NullDependencyMessage = "The dependency cannot be null.";

        private readonly IPlayerRepository _playerRepository;
        private readonly TwoFactorVerificationService _verificationService;
        private readonly TwoFactorDeliveryService _deliveryService;

        /// <summary>
        /// Initializes the service with the components it needs.
        /// </summary>
        /// <param name="playerRepository">Repository used to read and update players.</param>
        /// <param name="verificationService">Service that handles verification challenges and codes.</param>
        /// <param name="deliveryService">Service that sends the verification code by email.</param>
        public TwoFactorSettingsService(
            IPlayerRepository playerRepository,
            TwoFactorVerificationService verificationService,
            TwoFactorDeliveryService deliveryService)
        {
            if (playerRepository == null)
            {
                throw new ArgumentNullException(nameof(playerRepository), NullDependencyMessage);
            }

            if (verificationService == null)
            {
                throw new ArgumentNullException(nameof(verificationService), NullDependencyMessage);
            }

            if (deliveryService == null)
            {
                throw new ArgumentNullException(nameof(deliveryService), NullDependencyMessage);
            }

            _playerRepository = playerRepository;
            _verificationService = verificationService;
            _deliveryService = deliveryService;
        }

        /// <summary>
        /// Indicates whether the player has the two-step verification turned on.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>True if the player must enter a code to log in.</returns>
        public bool IsEnabled(string displayName)
        {
            Player player = FindPlayers(displayName).FirstOrDefault();

            return player != null && _playerRepository.HasTwoFactorEnabled(player.Id);
        }

        /// <summary>
        /// Sends a code to the player's email to confirm that the player wants to turn the verification on.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="culture">Language in which the email is written.</param>
        /// <returns>Outcome of the request and the data of the challenge.</returns>
        public TwoFactorActivationResult StartActivation(string displayName, CultureInfo culture)
        {
            Player player = FindPlayers(displayName).FirstOrDefault();
            TwoFactorActivationResult result;

            if (player == null)
            {
                result = CreateFailure(TwoFactorActivationStatus.PlayerNotFound);
            }
            else if (_playerRepository.HasTwoFactorEnabled(player.Id))
            {
                result = CreateFailure(TwoFactorActivationStatus.AlreadyEnabled);
            }
            else if (_verificationService.IsLocked(player.Email))
            {
                result = CreateFailure(TwoFactorActivationStatus.VerificationLocked);
            }
            else
            {
                result = SendActivationCode(player, culture);
            }

            return result;
        }

        /// <summary>
        /// Checks the code entered by the player and, if it is correct, turns the verification on.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="code">Verification code entered by the player.</param>
        /// <param name="challengeId">Identifier of the activation challenge.</param>
        /// <returns>Result of the validation.</returns>
        public TwoFactorValidationStatus ConfirmActivation(string displayName, string code, string challengeId)
        {
            Player player = FindPlayers(displayName).FirstOrDefault();
            TwoFactorValidationStatus status = TwoFactorValidationStatus.Incorrect;

            if (player != null)
            {
                status = _verificationService.VerifyPlayerCode(player.Email, code, challengeId);

                if (status == TwoFactorValidationStatus.Valid)
                {
                    _playerRepository.SaveTwoFactorEnabled(player.Id, true);
                }
            }

            return status;
        }

        /// <summary>
        /// Sends a new code while the player is turning the verification on.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="cultureName">Name of the language in which the email is written.</param>
        /// <param name="challengeId">Identifier of the activation challenge.</param>
        /// <returns>Outcome of the resend attempt.</returns>
        public ResendCodeStatus ResendActivationCode(string displayName, string cultureName, string challengeId)
        {
            Player player = FindPlayers(displayName).FirstOrDefault();
            ResendCodeStatus status = ResendCodeStatus.PlayerNotFound;

            if (player != null)
            {
                status = _verificationService.ResendPlayerCode(player.Email, cultureName, challengeId);
            }

            return status;
        }

        /// <summary>
        /// Turns the verification off after checking the player's password.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="password">Current password of the player.</param>
        /// <returns>Outcome of the request.</returns>
        public TwoFactorChangeStatus Disable(string displayName, string password)
        {
            Player player = FindPlayers(displayName).FirstOrDefault();
            TwoFactorChangeStatus status;

            if (player == null)
            {
                status = TwoFactorChangeStatus.PlayerNotFound;
            }
            else if (string.IsNullOrEmpty(password) || !PasswordHasher.Verify(password, player.PwdHash))
            {
                status = TwoFactorChangeStatus.InvalidPassword;
            }
            else
            {
                _playerRepository.SaveTwoFactorEnabled(player.Id, false);
                status = TwoFactorChangeStatus.Success;
            }

            return status;
        }

        private static TwoFactorActivationResult CreateFailure(TwoFactorActivationStatus status)
        {
            return new TwoFactorActivationResult(status, string.Empty, string.Empty);
        }

        private static string MaskEmail(string email)
        {
            int atIndex = email.IndexOf(AtSign);
            string maskedEmail = email;

            if (atIndex > VisibleLocalPartChars)
            {
                string visiblePart = email.Substring(0, VisibleLocalPartChars);
                string hiddenPart = new string(MaskCharacter, atIndex - VisibleLocalPartChars);

                maskedEmail = visiblePart + hiddenPart + email.Substring(atIndex);
            }

            return maskedEmail;
        }

        private List<Player> FindPlayers(string displayName)
        {
            var players = new List<Player>();

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                players = _playerRepository.FindByName(displayName);
            }

            return players;
        }

        private TwoFactorActivationResult SendActivationCode(Player player, CultureInfo culture)
        {
            string challengeId = _verificationService.CreateChallenge();
            var isSent = _deliveryService.SendCode(player, culture, challengeId);
            TwoFactorActivationResult result = CreateFailure(TwoFactorActivationStatus.CodeSendFailed);

            if (isSent)
            {
                result = new TwoFactorActivationResult(
                    TwoFactorActivationStatus.CodeSent,
                    challengeId,
                    MaskEmail(player.Email));
            }

            return result;
        }
    }
}
