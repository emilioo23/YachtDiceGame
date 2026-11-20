using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;

namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Counts the wrong passwords of each account and locks it: three in a row lock it for five minutes, and
    /// ten in the same twenty-four hours lock it for a whole day. The counts live in the memory of the server,
    /// shared by every login, so each change happens inside the lock. The lock itself is saved in the database,
    /// so a restart of the server does not open a locked account.
    /// </summary>
    public sealed class LoginAttemptTracker
    {
        /// <summary>
        /// Wrong passwords in a row that lock the account for a few minutes.
        /// </summary>
        public const int FailuresForTemporaryLock = 3;

        /// <summary>
        /// Minutes that the account waits after a temporary lock.
        /// </summary>
        public const int TemporaryLockMinutes = 5;

        /// <summary>
        /// Wrong passwords in twenty-four hours that lock the account for a whole day.
        /// </summary>
        public const int FailuresForDailyLock = 10;

        /// <summary>
        /// Hours that the account waits after a daily lock, and hours that a wrong password is remembered.
        /// </summary>
        public const int DailyLockHours = 24;

        private const int NoFailures = 0;
        private const string NullRepositoryMessage = "The data access of the players cannot be null.";

        private readonly object _syncRoot = new object();
        private readonly Dictionary<int, LoginAttempts> _attemptsByPlayer = new Dictionary<int, LoginAttempts>();
        private readonly IPlayerRepository _playerRepository;

        /// <summary>
        /// Initializes the tracker with the data access where the locks are saved.
        /// </summary>
        /// <param name="playerRepository">Data access of the players, which keeps their locks.</param>
        public LoginAttemptTracker(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository
                ?? throw new ArgumentNullException(nameof(playerRepository), NullRepositoryMessage);
        }

        /// <summary>
        /// Tells whether an account is locked at a moment.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="now">Current moment, in UTC.</param>
        /// <returns>The lock that is still running, or <see cref="LoginLock.None"/>.</returns>
        /// <exception cref="Data.Exceptions.DataAccessException">The saved lock could not be read.</exception>
        public LoginLock GetLock(int playerId, DateTime now)
        {
            LoginLock currentLock = LoginLock.None;
            AccountLock savedLock = _playerRepository.FindLocks(playerId).FirstOrDefault();
            bool isRunning = savedLock != null && savedLock.LockedUntil > now;

            if (isRunning && Enum.TryParse(savedLock.LockType, out LoginLock savedType))
            {
                currentLock = savedType;
            }

            return currentLock;
        }

        /// <summary>
        /// Counts a wrong password and locks the account when it reaches a limit.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="now">Current moment, in UTC.</param>
        /// <returns>The lock that starts with this wrong password, or <see cref="LoginLock.None"/>.</returns>
        /// <exception cref="Data.Exceptions.DataAccessException">The new lock could not be saved.</exception>
        public LoginLock RegisterFailure(int playerId, DateTime now)
        {
            LoginLock newLock;

            lock (_syncRoot)
            {
                LoginAttempts attempts = FindOrCreate(playerId);
                DateTime oldestRemembered = now.AddHours(-DailyLockHours);
                Predicate<DateTime> isForgotten = failureTime => failureTime <= oldestRemembered;

                attempts.FailureTimes.RemoveAll(isForgotten);
                attempts.FailureTimes.Add(now);
                attempts.ConsecutiveFailures++;
                newLock = ChooseLock(attempts);

                if (newLock != LoginLock.None)
                {
                    ResetCounts(attempts, newLock);
                }
            }

            if (newLock != LoginLock.None)
            {
                _playerRepository.SaveLock(playerId, newLock.ToString(), CalculateEnd(newLock, now));
            }

            return newLock;
        }

        /// <summary>
        /// Forgets the wrong passwords in a row after a correct password.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        public void RegisterSuccess(int playerId)
        {
            lock (_syncRoot)
            {
                if (_attemptsByPlayer.TryGetValue(playerId, out LoginAttempts attempts))
                {
                    attempts.ConsecutiveFailures = NoFailures;
                }
            }
        }

        private static LoginLock ChooseLock(LoginAttempts attempts)
        {
            LoginLock newLock = LoginLock.None;

            if (attempts.FailureTimes.Count >= FailuresForDailyLock)
            {
                newLock = LoginLock.Daily;
            }
            else if (attempts.ConsecutiveFailures >= FailuresForTemporaryLock)
            {
                newLock = LoginLock.Temporary;
            }

            return newLock;
        }

        private static void ResetCounts(LoginAttempts attempts, LoginLock newLock)
        {
            attempts.ConsecutiveFailures = NoFailures;

            if (newLock == LoginLock.Daily)
            {
                attempts.FailureTimes.Clear();
            }
        }

        private static DateTime CalculateEnd(LoginLock newLock, DateTime now)
        {
            DateTime lockedUntil = now.AddMinutes(TemporaryLockMinutes);

            if (newLock == LoginLock.Daily)
            {
                lockedUntil = now.AddHours(DailyLockHours);
            }

            return lockedUntil;
        }

        private LoginAttempts FindOrCreate(int playerId)
        {
            if (!_attemptsByPlayer.TryGetValue(playerId, out LoginAttempts attempts))
            {
                attempts = new LoginAttempts();
                _attemptsByPlayer[playerId] = attempts;
            }

            return attempts;
        }
    }
}
