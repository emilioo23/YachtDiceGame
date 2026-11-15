using System;
using System.Collections.Generic;

namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Counts the wrong passwords of each account and locks it: three in a row lock it for five minutes, and
    /// ten in the same twenty-four hours lock it for a whole day. The counts live in the memory of the server,
    /// shared by every login, so each change happens inside the lock.
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

        private readonly object _syncRoot = new object();
        private readonly Dictionary<int, LoginAttempts> _attemptsByPlayer = new Dictionary<int, LoginAttempts>();

        /// <summary>
        /// Tells whether an account is locked at a moment.
        /// </summary>
        /// <param name="playerId">Identifier of the account.</param>
        /// <param name="now">Current moment, in UTC.</param>
        /// <returns>The lock that is still running, or <see cref="LoginLock.None"/>.</returns>
        public LoginLock GetLock(int playerId, DateTime now)
        {
            LoginLock currentLock = LoginLock.None;

            lock (_syncRoot)
            {
                bool hasAttempts = _attemptsByPlayer.TryGetValue(playerId, out LoginAttempts attempts);

                if (hasAttempts && attempts.LockedUntil > now)
                {
                    currentLock = attempts.Lock;
                }
            }

            return currentLock;
        }

        /// <summary>
        /// Counts a wrong password and locks the account when it reaches a limit.
        /// </summary>
        /// <param name="playerId">Identifier of the account.</param>
        /// <param name="now">Moment of the wrong password, in UTC.</param>
        /// <returns>The lock that the failure started, or <see cref="LoginLock.None"/>.</returns>
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
                    StartLock(attempts, newLock, now);
                }
            }

            return newLock;
        }

        /// <summary>
        /// Forgets the wrong passwords in a row after a correct one. The failures of the day are kept.
        /// </summary>
        /// <param name="playerId">Identifier of the account.</param>
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

        private static void StartLock(LoginAttempts attempts, LoginLock newLock, DateTime now)
        {
            attempts.Lock = newLock;
            attempts.ConsecutiveFailures = NoFailures;

            if (newLock == LoginLock.Daily)
            {
                attempts.LockedUntil = now.AddHours(DailyLockHours);
                attempts.FailureTimes.Clear();
            }
            else
            {
                attempts.LockedUntil = now.AddMinutes(TemporaryLockMinutes);
            }
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
