using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Email;
using YachtDice.Server.Logic.Logging;
using YachtDice.Server.Logic.Security;

namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Recovers and changes the passwords of the accounts.
    /// </summary>
    public class PasswordService : IPasswordRecoveryService
    {
        private const string ChangedLogMessage = "Password changed with a recovery code.";
        private const string TooManyAttemptsLogMessage = "Recovery code discarded after too many wrong attempts.";
        private const string RequestedLogMessage = "A password recovery code was requested for an account.";
        private const string ChangedFromSettingsLogMessage = "A player changed the password from the settings.";
        private const string NullArgumentMessage = "The value cannot be null.";
        private const string EmailSubjectResourceKey = "PasswordReset_EmailSubject";
        private const string EmailBodyResourceKey = "PasswordReset_EmailBody";
        private const string NullDependencyMessage = "A dependency of the password service cannot be null.";
        private const string WrongPasswordLogMessage = "A wrong current password was typed in a password change.";
        private const int RecentLockDays = 7;
        private static readonly CodeEmailTemplate EmailTemplate =
            new CodeEmailTemplate(
                EmailSubjectResourceKey,
                EmailBodyResourceKey,
                SecureCodes.ExpirationMinutes);
        private readonly AppLogger _logger;
        private readonly IPlayerRepository _playerRepository;
        private readonly IPasswordResetRepository _resetRepository;
        private readonly CodeEmailSender _emailSender;
        private readonly PasswordResetAttemptCounter _attempts = new PasswordResetAttemptCounter();
        private readonly LoginAttemptTracker _attemptTracker;

        /// <summary>
        /// Initializes the service with the components it needs.
        /// </summary>
        /// <param name="playerRepository">Repository used to read players.</param>
        /// <param name="resetRepository">Repository that keeps the recovery codes.</param>
        /// <param name="emailService">Service that delivers the email with the recovery code.</param>
        public PasswordService(
            IPlayerRepository playerRepository,
            IPasswordResetRepository resetRepository,
            IEmailService emailService)
        {
            _playerRepository = playerRepository
                ?? throw new ArgumentNullException(nameof(playerRepository), NullDependencyMessage);
            _resetRepository = resetRepository
                ?? throw new ArgumentNullException(nameof(resetRepository), NullDependencyMessage);
            _emailSender = new CodeEmailSender(emailService, EmailTemplate);
            _attemptTracker = new LoginAttemptTracker(playerRepository);
            _logger = new AppLogger(GetType());
        }

        /// <summary>
        /// Generates a recovery code and sends it to the email of the account.
        /// </summary>
        /// <param name="email">Email of the account to recover.</param>
        /// <param name="culture">Language in which the email is written.</param>
        /// <returns>Outcome of the request.</returns>
        public PasswordResetRequestStatus RequestReset(string email, CultureInfo culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture), NullArgumentMessage);
            }

            PasswordResetRequestStatus status = PasswordResetRequestStatus.MissingRequiredField;

            if (!string.IsNullOrWhiteSpace(email))
            {
                Player player = _playerRepository.FindByEmail(email.Trim()).FirstOrDefault();
                status = PasswordResetRequestStatus.CodeSent;

                if (player != null && !WasCodeSentRecently(player.Id))
                {
                    _logger.Info(RequestedLogMessage);
                    status = CreateAndSendCode(player, culture);
                }
            }

            return status;
        }

        /// <summary>
        /// Checks the recovery code and, if it is correct, saves the new password.
        /// </summary>
        /// <param name="email">Email of the account to recover.</param>
        /// <param name="code">Recovery code written by the player.</param>
        /// <param name="newPassword">Password that the player wants.</param>
        /// <returns>Outcome of the confirmation.</returns>
        public PasswordResetStatus ConfirmReset(string email, string code, string newPassword)
        {
            PasswordResetStatus status;

            if (!HasAllFields(email, code, newPassword))
            {
                status = PasswordResetStatus.MissingRequiredField;
            }
            else if (!CredentialRules.IsStrongPassword(newPassword))
            {
                status = PasswordResetStatus.WeakPassword;
            }
            else
            {
                status = CheckPlayerCode(email, code, newPassword);
            }

            return status;
        }

        /// <summary>
        /// Changes the password of a player after checking the current one.
        /// </summary>
        /// <param name="displayName">Name of the player who is logged in.</param>
        /// <param name="currentPassword">Password that the player uses now.</param>
        /// <param name="newPassword">Password that the player wants.</param>
        /// <returns>The result of the change.</returns>
        /// <exception cref="Data.Exceptions.DataAccessException">The password could not be read or saved.</exception>
        public PasswordChangeStatus ChangePassword(string displayName, string currentPassword, string newPassword)
        {
            Player player = FindPlayer(displayName);
            PasswordChangeStatus status = PasswordChangeStatus.PlayerNotFound;

            if (player != null)
            {
                status = CheckAndChange(player, currentPassword, newPassword);
            }

            return status;
        }

        /// <summary>
        /// Gets the last change of password of the player and the last lock of the account, if it was recent.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The summary; it says that nothing happened if the player does not exist.</returns>
        public SecuritySummary GetSecuritySummary(string displayName)
        {
            Player player = FindPlayer(displayName);
            var summary = new SecuritySummary { LockReason = string.Empty };

            if (player != null)
            {
                List<DateTime> changeDates = _resetRepository.FindPasswordChangeDates(player.Id);
                AccountLock lastLock = _playerRepository.FindLocks(player.Id).FirstOrDefault();

                summary.HasPasswordChange = changeDates.Any();
                summary.LastPasswordChange = changeDates.FirstOrDefault();
                AddLock(summary, lastLock);
            }

            return summary;
        }

        private static bool HasAllFields(string email, string code, string newPassword)
        {
            return !string.IsNullOrWhiteSpace(email)
                && !string.IsNullOrWhiteSpace(code)
                && !string.IsNullOrWhiteSpace(newPassword);
        }

        private bool WasCodeSentRecently(int playerId)
        {
            PwdResetToken token = _resetRepository.FindByPlayerId(playerId).FirstOrDefault();
            var isRecent = false;

            if (token != null)
            {
                DateTime createdAt = token.ExpireDate.AddMinutes(-SecureCodes.ExpirationMinutes);
                isRecent = DateTime.UtcNow < createdAt.AddSeconds(SecureCodes.ResendCooldownSeconds);
            }

            return isRecent;
        }

        private PasswordResetRequestStatus CreateAndSendCode(Player player, CultureInfo culture)
        {
            string code = SecureCodes.Generate(SecureCodes.CodeLength);
            string codeHash = CredentialRules.ComputeResetCodeHash(player.Id, code);

            var token = new PwdResetToken
            {
                PlayerId = player.Id,
                Token = codeHash,
                ExpireDate = DateTime.UtcNow.AddMinutes(SecureCodes.ExpirationMinutes)
            };

            _resetRepository.Save(token);
            _attempts.Reset(player.Id);

            bool isSent = _emailSender.Send(player, code, culture);

            if (!isSent)
            {
                _resetRepository.Delete(player.Id);
            }

            return isSent ? PasswordResetRequestStatus.CodeSent : PasswordResetRequestStatus.CodeSendFailed;
        }

        private PasswordResetStatus CheckPlayerCode(string email, string code, string newPassword)
        {
            Player player = _playerRepository.FindByEmail(email.Trim()).FirstOrDefault();
            PasswordResetStatus status = PasswordResetStatus.CodeExpired;

            if (player != null)
            {
                status = CheckSavedCode(player, code, newPassword);
            }

            return status;
        }

        private PasswordResetStatus CheckSavedCode(Player player, string code, string newPassword)
        {
            PwdResetToken token = _resetRepository.FindByPlayerId(player.Id).FirstOrDefault();
            PasswordResetStatus status = PasswordResetStatus.CodeExpired;

            if (token != null && token.ExpireDate < DateTime.UtcNow)
            {
                _resetRepository.Delete(player.Id);
            }
            else if (token != null)
            {
                status = CheckCodeMatch(token, code, newPassword);
            }

            return status;
        }

        private PasswordResetStatus CheckCodeMatch(PwdResetToken token, string code, string newPassword)
        {
            int playerId = token.PlayerId;
            string enteredHash = CredentialRules.ComputeResetCodeHash(playerId, code.Trim());
            bool isCorrect = string.Equals(enteredHash, token.Token, StringComparison.OrdinalIgnoreCase);
            PasswordResetStatus status;

            if (isCorrect)
            {
                status = SaveRecoveredPassword(playerId, newPassword);
            }
            else
            {
                status = RegisterWrongCode(playerId);
            }

            return status;
        }

        private PasswordResetStatus SaveRecoveredPassword(int playerId, string newPassword)
        {
            Func<string, bool> isSamePassword = passwordHash => CredentialRules.Verify(newPassword, passwordHash);
            bool isUsedBefore = _resetRepository.FindRecentPasswordHashes(playerId).Any(isSamePassword);
            PasswordResetStatus status = PasswordResetStatus.PasswordUsedBefore;

            if (!isUsedBefore)
            {
                _resetRepository.CompleteReset(playerId, CredentialRules.ComputeHash(newPassword));
                _attempts.Reset(playerId);
                _logger.Info(ChangedLogMessage);
                status = PasswordResetStatus.PasswordChanged;
            }

            return status;
        }

        private PasswordResetStatus RegisterWrongCode(int playerId)
        {
            PasswordResetStatus status = PasswordResetStatus.IncorrectCode;

            if (_attempts.RegisterFailure(playerId) >= SecureCodes.MaxFailedAttempts)
            {
                _resetRepository.Delete(playerId);
                _attempts.Reset(playerId);
                _logger.Info(TooManyAttemptsLogMessage);
                status = PasswordResetStatus.TooManyAttempts;
            }

            return status;
        }

        private PasswordChangeStatus CheckAndChange(Player player, string currentPassword, string newPassword)
        {
            PasswordChangeStatus status = FindProblem(player, currentPassword, newPassword);

            if (status == PasswordChangeStatus.Success)
            {
                _resetRepository.CompleteReset(player.Id, CredentialRules.ComputeHash(newPassword));
                _attemptTracker.RegisterSuccess(player.Id);
                _logger.Info(ChangedFromSettingsLogMessage);
            }

            return status;
        }

        private PasswordChangeStatus FindProblem(Player player, string currentPassword, string newPassword)
        {
            PasswordChangeStatus status = PasswordChangeStatus.Success;

            if (_attemptTracker.GetLock(player.Id, DateTime.UtcNow) != LoginLock.None)
            {
                status = PasswordChangeStatus.TooManyAttempts;
            }
            else if (!IsCurrentPassword(player, currentPassword))
            {
                _attemptTracker.RegisterFailure(player.Id, DateTime.UtcNow);
                _logger.Info(WrongPasswordLogMessage);
                status = PasswordChangeStatus.WrongCurrentPassword;
            }
            else if (!CredentialRules.IsStrongPassword(newPassword))
            {
                status = PasswordChangeStatus.InvalidNewPassword;
            }
            else if (IsUsedBefore(player.Id, newPassword))
            {
                status = PasswordChangeStatus.PasswordUsedBefore;
            }

            return status;
        }

        private static bool IsCurrentPassword(Player player, string password)
        {
            bool hasValidSize = !string.IsNullOrEmpty(password) && password.Length <= CredentialRules.PasswordMaxLength;

            return hasValidSize && CredentialRules.Verify(password, player.PwdHash);
        }

        private bool IsUsedBefore(int playerId, string newPassword)
        {
            Func<string, bool> isSamePassword = passwordHash => CredentialRules.Verify(newPassword, passwordHash);

            return _resetRepository.FindRecentPasswordHashes(playerId).Any(isSamePassword);
        }

        private static void AddLock(SecuritySummary summary, AccountLock lastLock)
        {
            DateTime recentLimit = DateTime.UtcNow.AddDays(-RecentLockDays);

            if (lastLock != null && lastLock.LockedUntil >= recentLimit)
            {
                summary.HasRecentLock = true;
                summary.LockedUntil = lastLock.LockedUntil;
                summary.LockReason = lastLock.LockType;
            }
        }

        private Player FindPlayer(string displayName)
        {
            List<Player> matches = new List<Player>();

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                matches = _playerRepository.FindByName(displayName);
            }

            return matches.FirstOrDefault();
        }
    }
}
