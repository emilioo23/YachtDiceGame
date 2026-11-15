using System;
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
    /// Checks the player's credentials and starts the two-step verification when the player has it turned on.
    /// </summary>
    public class LoginService : ILoginService
    {
        private const string InvalidEmailLogMessage = "Login attempt with a malformed email.";
        private const string UnknownEmailLogMessage = "Login attempt with an email that is not registered.";
        private const string WrongPasswordLogMessage = "Login attempt with a wrong password.";
        private const string LockedAccountLogMessage = "Login attempt while the two-step verification is locked.";
        private const string NullCredentialsMessage = "The login credentials cannot be null.";
        private const string NullDependencyMessage = "A dependency of the login service cannot be null.";
        private const string LockedLoginLogMessage = "Login attempt while the account is locked by wrong passwords.";

        private readonly AppLogger _logger;
        private readonly IPlayerRepository _playerRepository;
        private readonly TwoFactorDeliveryService _deliveryService;
        private readonly TwoFactorVerificationService _verificationService;
        private readonly LoginAttemptTracker _attemptTracker = new LoginAttemptTracker();

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
            _playerRepository = playerRepository
                ?? throw new ArgumentNullException(nameof(playerRepository), NullDependencyMessage);
            _verificationService = verificationService
                ?? throw new ArgumentNullException(nameof(verificationService), NullDependencyMessage);
            _deliveryService = deliveryService
                ?? throw new ArgumentNullException(nameof(deliveryService), NullDependencyMessage);
            _logger = new AppLogger(GetType());
        }

        /// <summary>
        /// Checks the credentials and, if they are correct, sends the verification code or completes the login.
        /// </summary>
        /// <param name="credentials">Credentials and language of the player.</param>
        /// <returns>Result of the login attempt.</returns>
        public LoginResult LoginPlayer(LoginCredentials credentials)
        {
            if (credentials == null)
            {
                throw new ArgumentNullException(nameof(credentials), NullCredentialsMessage);
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
        /// Checks the email first and then the password, so the player knows which of the two failed.
        /// </summary>
        /// <param name="credentials">The login credentials provided by the user.</param>
        /// <returns>The login result status.</returns>
        private LoginResult Authenticate(LoginCredentials credentials)
        {
            string email = credentials.Email.Trim();
            LoginResult result;

            if (EmailAddressValidator.IsValid(email))
            {
                result = CheckPlayer(email, credentials);
            }
            else
            {
                result = Reject(LoginStatus.InvalidEmail, InvalidEmailLogMessage);
            }

            return result;
        }

        private LoginResult CheckPlayer(string email, LoginCredentials credentials)
        {
            Player player = _playerRepository.FindByEmail(email).FirstOrDefault();
            LoginResult result;

            if (player == null)
            {
                result = Reject(LoginStatus.EmailNotRegistered, UnknownEmailLogMessage);
            }
            else
            {
                result = CheckPassword(player, credentials);
            }

            return result;
        }

        private LoginResult CheckPassword(Player player, LoginCredentials credentials)
        {
            DateTime now = DateTime.UtcNow;
            LoginLock currentLock = _attemptTracker.GetLock(player.Id, now);
            LoginResult result;

            if (currentLock != LoginLock.None)
            {
                result = RejectLocked(currentLock);
            }
            else if (!PasswordHasher.Verify(credentials.Password, player.PwdHash))
            {
                result = RejectWrongPassword(player.Id, now);
            }
            else
            {
                _attemptTracker.RegisterSuccess(player.Id);
                result = CompleteLogin(player, credentials.Culture);
            }

            return result;
        }

        private LoginResult RejectWrongPassword(int playerId, DateTime now)
        {
            LoginLock newLock = _attemptTracker.RegisterFailure(playerId, now);
            LoginResult result;

            if (newLock == LoginLock.None)
            {
                result = Reject(LoginStatus.InvalidPassword, WrongPasswordLogMessage);
            }
            else
            {
                result = RejectLocked(newLock);
            }

            return result;
        }

        private LoginResult RejectLocked(LoginLock currentLock)
        {
            LoginStatus status = currentLock == LoginLock.Daily
                ? LoginStatus.LockedForTheDay
                : LoginStatus.TemporarilyLocked;

            return Reject(status, LockedLoginLogMessage);
        }

        private LoginResult CompleteLogin(Player player, CultureInfo culture)
        {
            LoginResult result;

            if (_verificationService.IsLocked(player.Email))
            {
                _logger.Warning(LockedAccountLogMessage);
                result = new LoginResult(LoginStatus.VerificationLocked, string.Empty);
            }
            else if (_playerRepository.HasTwoFactorEnabled(player.Id))
            {
                result = SendCode(player, culture);
            }
            else
            {
                result = new LoginResult(LoginStatus.LoggedIn, player, string.Empty);
            }

            return result;
        }

        private LoginResult Reject(LoginStatus status, string logMessage)
        {
            _logger.Warning(logMessage);

            return new LoginResult(status, string.Empty);
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
            bool isSent = _deliveryService.SendCode(player, challengeId, culture);
            LoginStatus status = isSent ? LoginStatus.VerificationCodeSent : LoginStatus.CodeSendFailed;

            return new LoginResult(status, player, challengeId);
        }
    }
}