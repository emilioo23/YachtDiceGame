using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
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
    public class LoginService
    {
        private const string InvalidCredentialsLogMessage = "Login attempt with invalid credentials.";
        private const string LockedAccountLogMessage = "Login attempt while the two-step verification is locked.";
        private const string NullRequestMessage = "The login request cannot be null.";
        private const string NullDependencyMessage = "The dependency cannot be null.";

        private readonly AppLogger _logger = new AppLogger(typeof(LoginService));
        private readonly IPlayerRepository _playerRepository;
        private readonly TwoFactorCodeService _codeService;
        private readonly TwoFactorDeliveryService _deliveryService;

        /// <summary>
        /// Initializes the service with the components it needs.
        /// </summary>
        /// <param name="playerRepository">Repository used to read players.</param>
        /// <param name="codeService">Service that keeps the verification codes (shared instance).</param>
        /// <param name="deliveryService">Service that sends the verification code by email.</param>
        public LoginService(
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
        /// Checks the credentials and, if they are correct, sends the verification code.
        /// </summary>
        /// <param name="request">Credentials and language of the player.</param>
        /// <returns>Result of the login attempt.</returns>
        public async Task<LoginResult> LoginAsync(LoginRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), NullRequestMessage);
            }

            LoginResult result;

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                result = new LoginResult(LoginStatus.MissingRequiredField, string.Empty);
            }
            else
            {
                result = await AuthenticateAsync(request);
            }

            return result;
        }

        private async Task<LoginResult> AuthenticateAsync(LoginRequest request)
        {
            List<Player> matches = await _playerRepository.FindByEmailAsync(request.Email.Trim());
            Player player = matches.FirstOrDefault();
            LoginResult result;

            if (player == null || !PasswordHasher.Verify(request.Password, player.PwdHash))
            {
                _logger.Warning(InvalidCredentialsLogMessage);
                result = new LoginResult(LoginStatus.InvalidCredentials, string.Empty);
            }
            else if (_codeService.IsLocked(player.Email))
            {
                _logger.Warning(LockedAccountLogMessage);
                result = new LoginResult(LoginStatus.VerificationLocked, string.Empty);
            }
            else
            {
                result = await SendCodeAsync(player, request.Culture);
            }

            return result;
        }

        private async Task<LoginResult> SendCodeAsync(Player player, CultureInfo culture)
        {
            bool wasSent = await _deliveryService.SendCodeAsync(player.Email, player.DisplayName, culture);
            LoginStatus status = wasSent ? LoginStatus.VerificationCodeSent : LoginStatus.CodeSendFailed;

            return new LoginResult(status, player.DisplayName);
        }
    }
}