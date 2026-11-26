using System;
using System.Globalization;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Logging;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Logging;
using YachtDice.Server.Logic.Players;
using YachtDice.Server.Logic.Security;
using YachtDice.Server.Logic.TwoFactor;

namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Checks the player's credentials and starts the two-step verification when the player has it turned on.
    /// </summary>
    public class LoginService : ILoginService
    {
        private const char EmailSeparator = '@';
        private const string TimingEqualizerPassword = "password-used-only-to-take-the-same-time";

        private readonly AppLogger _logger;
        private readonly IPlayerRepository _playerRepository;
        private readonly TwoFactorCodeService _codeService;
        private readonly TwoFactorVerificationService _verificationService;
        private readonly LoginAttemptTracker _attemptTracker;
        private readonly string _timingEqualizerHash;

        /// <summary>
        /// Initializes the service with the components it needs.
        /// </summary>
        /// <param name="playerRepository">Repository used to read players.</param>
        /// <param name="verificationService">Service that handles verification challenges and lockouts.</param>
        /// <param name="codeService">Service that creates and sends the verification codes.</param>
        public LoginService(
            IPlayerRepository playerRepository,
            TwoFactorVerificationService verificationService,
            TwoFactorCodeService codeService)
        {
            _playerRepository = playerRepository
                ?? throw new ArgumentNullException(nameof(playerRepository), LogMessages.LoginNullDependencyMessage);
            _verificationService = verificationService
                ?? throw new ArgumentNullException(nameof(verificationService), LogMessages.LoginNullDependencyMessage);
            _codeService = codeService
                ?? throw new ArgumentNullException(nameof(codeService), LogMessages.LoginNullDependencyMessage);
            _attemptTracker = new LoginAttemptTracker(playerRepository);
            _timingEqualizerHash = CredentialRules.ComputeHash(TimingEqualizerPassword);
            _logger = new AppLogger(GetType());
        }

        /// <summary>
        /// Event triggered when an account is locked for the day because of too many wrong passwords, with the
        /// player and the language of the login, so the player can be told by email.
        /// </summary>
        public event Action<Player, CultureInfo> AccountLockedForTheDay;

        /// <summary>
        /// Checks the credentials and, if they are correct, sends the verification code or completes the login.
        /// </summary>
        /// <param name="credentials">Credentials and language of the player.</param>
        /// <returns>Result of the login attempt.</returns>
        public LoginResult LoginPlayer(LoginCredentials credentials)
        {
            if (credentials == null)
            {
                throw new ArgumentNullException(nameof(credentials), LogMessages.NullCredentialsMessage);
            }

            LoginResult result;

            if (string.IsNullOrWhiteSpace(credentials.Identifier) || string.IsNullOrWhiteSpace(credentials.Password))
            {
                result = new LoginResult(LoginStatus.MissingRequiredField, string.Empty);
            }
            else if (credentials.Password.Length > CredentialRules.PasswordMaxLength)
            {
                result = Reject(LoginStatus.InvalidCredentials, LogMessages.TooLongPasswordLogMessage);
            }
            else
            {
                result = Authenticate(credentials);
            }

            return result;
        }

        /// <summary>
        /// Looks for the account by email when the text has an at sign and by username when it does not, because a
        /// username cannot have one. When the account is not registered and when the password is wrong the answer is
        /// the same, so the screen cannot be used to find out which emails or usernames have an account (section 8.8
        /// of the standard).
        /// </summary>
        /// <param name="credentials">The login credentials provided by the user.</param>
        /// <returns>The login result status.</returns>
        private LoginResult Authenticate(LoginCredentials credentials)
        {
            string identifier = credentials.Identifier.Trim();
            LoginResult result;

            if (identifier.Contains(EmailSeparator))
            {
                result = AuthenticateByEmail(identifier, credentials);
            }
            else
            {
                result = AuthenticateByUsername(identifier, credentials);
            }

            return result;
        }

        private LoginResult AuthenticateByEmail(string email, LoginCredentials credentials)
        {
            LoginResult result;

            if (CredentialRules.IsValid(email))
            {
                Player player = _playerRepository.FindByEmail(email).FirstOrDefault();

                result = CheckPlayer(player, credentials, LogMessages.UnknownEmailLogMessage);
            }
            else
            {
                result = Reject(LoginStatus.InvalidEmail, LogMessages.InvalidEmailLogMessage);
            }

            return result;
        }

        private LoginResult AuthenticateByUsername(string username, LoginCredentials credentials)
        {
            LoginResult result;

            if (CredentialRules.IsValidUsername(username))
            {
                Player player = _playerRepository.FindByUsername(username).FirstOrDefault();

                result = CheckPlayer(player, credentials, LogMessages.UnknownUsernameLogMessage);
            }
            else
            {
                result = Reject(LoginStatus.InvalidCredentials, LogMessages.InvalidUsernameLogMessage);
            }

            return result;
        }

        private LoginResult CheckPlayer(Player player, LoginCredentials credentials, string unknownLogMessage)
        {
            LoginResult result;

            if (player == null)
            {
                CredentialRules.Verify(credentials.Password, _timingEqualizerHash);
                result = Reject(LoginStatus.InvalidCredentials, unknownLogMessage);
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
                result = RejectLocked(player, currentLock);
            }
            else if (!CredentialRules.Verify(credentials.Password, player.PwdHash))
            {
                result = RejectWrongPassword(player, credentials.Culture, now);
            }
            else
            {
                _attemptTracker.RegisterSuccess(player.Id);
                result = CompleteLogin(player, credentials.Culture);
            }

            return result;
        }

        private LoginResult RejectWrongPassword(Player player, CultureInfo culture, DateTime now)
        {
            LoginLock newLock = _attemptTracker.RegisterFailure(player.Id, now);
            LoginResult result;

            if (newLock == LoginLock.None)
            {
                result = Reject(LoginStatus.InvalidCredentials, Describe(LogMessages.LoginWrongPasswordLogFormat,
                    player));
            }
            else
            {
                NotifyDailyLock(player, culture, newLock);
                result = RejectLocked(player, newLock);
            }

            return result;
        }

        private void NotifyDailyLock(Player player, CultureInfo culture, LoginLock newLock)
        {
            if (newLock == LoginLock.Daily)
            {
                _logger.Warning(Describe(LogMessages.DailyLockLogFormat, player));
                AccountLockedForTheDay?.Invoke(player, culture);
            }
        }

        private static string Describe(string logFormat, Player player)
        {
            return string.Format(CultureInfo.InvariantCulture, logFormat, player.Id);
        }

        private LoginResult RejectLocked(Player player, LoginLock currentLock)
        {
            return Reject(LoginAttemptTracker.ToStatus(currentLock), Describe(LogMessages.LockedLoginLogFormat,
                player));
        }

        private LoginResult CompleteLogin(Player player, CultureInfo culture)
        {
            LoginResult result;

            if (_verificationService.IsLocked(player.Email))
            {
                _logger.Warning(LogMessages.LockedAccountLogMessage);
                result = new LoginResult(LoginStatus.VerificationLocked, string.Empty);
            }
            else if (IsUnverified(player) || _playerRepository.HasTwoFactorEnabled(player.Id))
            {
                result = SendCode(player, culture);
            }
            else
            {
                _logger.Info(Describe(LogMessages.LoginLoggedInLogFormat, player));
                result = new LoginResult(LoginStatus.LoggedIn, player, string.Empty);
            }

            return result;
        }

        private static bool IsUnverified(Player player)
        {
            return player.State == PlayerState.Unverified.ToString();
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
            bool isSent = _codeService.SendCode(player, challengeId, culture);
            LoginStatus status = isSent ? LoginStatus.VerificationCodeSent : LoginStatus.CodeSendFailed;

            if (isSent)
            {
                _logger.Info(Describe(LogMessages.CodeSentLogFormat, player));
            }

            return new LoginResult(status, player, challengeId);
        }
    }
}