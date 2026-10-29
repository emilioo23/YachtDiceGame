using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Logging;
using YachtDice.Server.Logic.Security;
using YachtDice.Server.Logic.TwoFactor;

namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Checks the player's credentials and starts the two-step verification.
    /// </summary>
    public class LoginService : ILoginService
    {
        private const string InvalidCredentialsLogMessage = "Login attempt with invalid credentials.";
        private const string LockedAccountLogMessage = "Login attempt while the two-step verification is locked.";
        private const string NullRequestMessage = "The login request cannot be null.";
        private const string NullDependencyMessage = "The dependency cannot be null.";

        private readonly AppLogger _logger = new AppLogger(typeof(LoginService));
        private readonly IPlayerRepository _playerRepository;
        private readonly TwoFactorDeliveryService _deliveryService;
        private readonly TwoFactorVerificationService _verificationService;

        /// <summary>
        /// Initializes the service with the components it needs.
        /// </summary>
        /// <param name="playerRepository">Repository used to read players.</param>
        /// <param name="verificationService">Service that handles verification challenges and lockouts.</param>
        /// <param name="deliveryService">Service that sends the verification code by email.</param>
        public LoginService(
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
        /// Checks the credentials and, if they are correct, sends the verification code.
        /// </summary>
        /// <param name="credentials">Credentials and language of the player.</param>
        /// <returns>Result of the login attempt.</returns>
        public LoginResult LoginPlayer(LoginCredentials credentials)
        {
            if (credentials == null)
            {
                throw new ArgumentNullException(nameof(credentials), NullRequestMessage);
            }

            LoginResult result;

            if (string.IsNullOrWhiteSpace(credentials.Email) || string.IsNullOrWhiteSpace(credentials.Password))
            {
                result = new LoginResult(LoginStatus.MissingRequiredField, string.Empty);
            }
            else
            {
                result = Authenticate(credentials);
            }

            return result;
        }

        /// <summary>
        /// Authenticates the user synchronously by checking the database and password hash.
        /// </summary>
        /// <param name="credentials">The login credentials provided by the user.</param>
        /// <returns>The login result status.</returns>
        private LoginResult Authenticate(LoginCredentials credentials)
        {
            List<Player> matches = _playerRepository.FindByEmail(credentials.Email.Trim());
            Player player = matches.FirstOrDefault();
            LoginResult result;

            if (player == null || !PasswordHasher.Verify(credentials.Password, player.PwdHash))
            {
                _logger.Warning(InvalidCredentialsLogMessage);
                result = new LoginResult(LoginStatus.InvalidCredentials, string.Empty);
            }
            else if (_verificationService.IsLocked(player.Email))
            {
                _logger.Warning(LockedAccountLogMessage);
                result = new LoginResult(LoginStatus.VerificationLocked, string.Empty);
            }
            else
            {
                result = SendCode(player, credentials.Culture);
            }

            return result;
        }

        /// <summary>
        /// Sends the verification code synchronously to the authenticated user.
        /// </summary>
        /// <param name="player">The authenticated player entity.</param>
        /// <param name="culture">The culture information for localization.</param>
        /// <returns>The result containing the challenge ID if successful.</returns>
        private LoginResult SendCode(Player player, CultureInfo culture)
        {
            string challengeId = _verificationService.CreateChallenge();
            bool wasSent = _deliveryService.SendCode(player, culture, challengeId);
            LoginStatus status = wasSent ? LoginStatus.VerificationCodeSent : LoginStatus.CodeSendFailed;

            return new LoginResult(status, player.DisplayName, challengeId);
        }
    }
}