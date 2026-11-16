using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Logging;
using YachtDice.Server.Logic.Security;

namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Lets a player who is logged in change the password. The player has to type the current password, so a
    /// person who finds the computer open cannot take over the account. A wrong current password counts as a
    /// wrong password of the login, so this operation cannot be used to guess the password without limit.
    /// </summary>
    public class PasswordChangeService
    {
        private const string ChangedLogMessage = "A player changed the password from the settings.";
        private const string WrongPasswordLogMessage = "A wrong current password was typed in a password change.";
        private const string NullDependencyMessage = "A dependency of the password change cannot be null.";
        private const string NullRequestMessage = "The password change request cannot be null.";
        private const int RecentLockDays = 7;

        private readonly IPlayerRepository _playerRepository;
        private readonly IPasswordResetRepository _passwordRepository;
        private readonly LoginAttemptTracker _attemptTracker;
        private readonly AppLogger _logger;

        /// <summary>
        /// Initializes the service with the data access it needs.
        /// </summary>
        /// <param name="playerRepository">Data access of the players.</param>
        /// <param name="passwordRepository">Data access that saves the new password and remembers the old ones.</param>
        /// <exception cref="ArgumentNullException">Any of the parameters is null.</exception>
        public PasswordChangeService(IPlayerRepository playerRepository, IPasswordResetRepository passwordRepository)
        {
            _playerRepository = playerRepository
                ?? throw new ArgumentNullException(nameof(playerRepository), NullDependencyMessage);
            _passwordRepository = passwordRepository
                ?? throw new ArgumentNullException(nameof(passwordRepository), NullDependencyMessage);
            _attemptTracker = new LoginAttemptTracker(playerRepository);
            _logger = new AppLogger(GetType());
        }

        /// <summary>
        /// Changes the password of a player after checking the current one.
        /// </summary>
        /// <param name="displayName">Name of the player who is logged in.</param>
        /// <param name="request">Current password and new password.</param>
        /// <returns>The result of the change.</returns>
        /// <exception cref="ArgumentNullException">The request is null.</exception>
        /// <exception cref="Data.Exceptions.DataAccessException">The password could not be read or saved.</exception>
        public PasswordChangeStatus ChangePassword(string displayName, PasswordChangeRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), NullRequestMessage);
            }

            Player player = FindPlayer(displayName);
            PasswordChangeStatus status = PasswordChangeStatus.PlayerNotFound;

            if (player != null)
            {
                status = CheckAndChange(player, request);
            }

            return status;
        }

        private PasswordChangeStatus CheckAndChange(Player player, PasswordChangeRequest request)
        {
            PasswordChangeStatus status = FindProblem(player, request);

            if (status == PasswordChangeStatus.Success)
            {
                _passwordRepository.CompleteReset(player.Id, PasswordHasher.ComputeHash(request.NewPassword));
                _attemptTracker.RegisterSuccess(player.Id);
                _logger.Info(ChangedLogMessage);
            }

            return status;
        }

        private PasswordChangeStatus FindProblem(Player player, PasswordChangeRequest request)
        {
            PasswordChangeStatus status = PasswordChangeStatus.Success;

            if (_attemptTracker.GetLock(player.Id, DateTime.UtcNow) != LoginLock.None)
            {
                status = PasswordChangeStatus.TooManyAttempts;
            }
            else if (!IsCurrentPassword(player, request.CurrentPassword))
            {
                _attemptTracker.RegisterFailure(player.Id, DateTime.UtcNow);
                _logger.Info(WrongPasswordLogMessage);
                status = PasswordChangeStatus.WrongCurrentPassword;
            }
            else if (!CredentialRules.IsStrongPassword(request.NewPassword))
            {
                status = PasswordChangeStatus.InvalidNewPassword;
            }
            else if (IsUsedBefore(player.Id, request.NewPassword))
            {
                status = PasswordChangeStatus.PasswordUsedBefore;
            }

            return status;
        }

        private static bool IsCurrentPassword(Player player, string password)
        {
            bool hasValidSize = !string.IsNullOrEmpty(password) && password.Length <= CredentialRules.PasswordMaxLength;

            return hasValidSize && PasswordHasher.Verify(password, player.PwdHash);
        }

        private bool IsUsedBefore(int playerId, string newPassword)
        {
            Func<string, bool> isSamePassword = passwordHash => PasswordHasher.Verify(newPassword, passwordHash);

            return _passwordRepository.FindRecentPasswordHashes(playerId).Any(isSamePassword);
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
                List<DateTime> changeDates = _passwordRepository.FindPasswordChangeDates(player.Id);
                AccountLock lastLock = _playerRepository.FindLocks(player.Id).FirstOrDefault();

                summary.HasPasswordChange = changeDates.Any();
                summary.LastPasswordChange = changeDates.FirstOrDefault();
                AddLock(summary, lastLock);
            }

            return summary;
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
            List<Player> matches = string.IsNullOrWhiteSpace(displayName)
                ? new List<Player>()
                : _playerRepository.FindByName(displayName);

            return matches.FirstOrDefault();
        }
    }
}
