using System;
using Xunit;
using YachtDice.Server.Logic.Authentication;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Authentication
{
    /// <summary>
    /// Locks of an account after wrong passwords: five minutes after three in a row, a day after ten in a day.
    /// </summary>
    public class LoginAttemptTrackerTests
    {
        private const int PlayerId = 7;
        private const int OtherPlayerId = 8;
        private const int FourMinutes = 4;
        private const int SixMinutes = 6;
        private const int TenMinutes = 10;
        private const int TwentyFiveHours = 25;

        private static readonly DateTime Start = DateTime.UtcNow;

        private readonly InMemoryPlayerRepository _players = new InMemoryPlayerRepository();
        private readonly LoginAttemptTracker _tracker;

        /// <summary>
        /// Prepares a tracker whose locks are saved in memory.
        /// </summary>
        public LoginAttemptTrackerTests()
        {
            _tracker = new LoginAttemptTracker(_players);
        }

        [Fact]
        public void RegisterFailure_TwoWrongPasswords_DoesNotLock()
        {
            _tracker.RegisterFailure(PlayerId, Start);

            LoginLock newLock = _tracker.RegisterFailure(PlayerId, Start);

            Assert.Equal(LoginLock.None, newLock);
        }

        [Fact]
        public void RegisterFailure_ThirdWrongPasswordInARow_LocksTemporarily()
        {
            FailTimes(PlayerId, LoginAttemptTracker.FailuresForTemporaryLock - 1, Start);

            LoginLock newLock = _tracker.RegisterFailure(PlayerId, Start);

            Assert.Equal(LoginLock.Temporary, newLock);
        }

        [Fact]
        public void GetLock_FourMinutesAfterATemporaryLock_IsStillLocked()
        {
            FailTimes(PlayerId, LoginAttemptTracker.FailuresForTemporaryLock, Start);

            LoginLock currentLock = _tracker.GetLock(PlayerId, Start.AddMinutes(FourMinutes));

            Assert.Equal(LoginLock.Temporary, currentLock);
        }

        [Fact]
        public void GetLock_SixMinutesAfterATemporaryLock_IsUnlocked()
        {
            FailTimes(PlayerId, LoginAttemptTracker.FailuresForTemporaryLock, Start);

            LoginLock currentLock = _tracker.GetLock(PlayerId, Start.AddMinutes(SixMinutes));

            Assert.Equal(LoginLock.None, currentLock);
        }

        [Fact]
        public void RegisterSuccess_AfterTwoWrongPasswords_StartsTheCountAgain()
        {
            FailTimes(PlayerId, LoginAttemptTracker.FailuresForTemporaryLock - 1, Start);
            _tracker.RegisterSuccess(PlayerId);

            LoginLock newLock = _tracker.RegisterFailure(PlayerId, Start);

            Assert.Equal(LoginLock.None, newLock);
        }

        [Fact]
        public void RegisterFailure_TenthWrongPasswordOfTheDay_LocksForTheDay()
        {
            DateTime moment = Start;

            for (int failureIndex = 1; failureIndex < LoginAttemptTracker.FailuresForDailyLock; failureIndex++)
            {
                _tracker.RegisterFailure(PlayerId, moment);
                _tracker.RegisterSuccess(PlayerId);
                moment = moment.AddMinutes(TenMinutes);
            }

            LoginLock newLock = _tracker.RegisterFailure(PlayerId, moment);

            Assert.Equal(LoginLock.Daily, newLock);
        }

        [Fact]
        public void GetLock_TwentyFiveHoursAfterADailyLock_IsUnlocked()
        {
            FailTimes(PlayerId, LoginAttemptTracker.FailuresForDailyLock, Start);

            LoginLock currentLock = _tracker.GetLock(PlayerId, Start.AddHours(TwentyFiveHours));

            Assert.Equal(LoginLock.None, currentLock);
        }

        [Fact]
        public void GetLock_AnotherAccount_IsNotLocked()
        {
            FailTimes(PlayerId, LoginAttemptTracker.FailuresForTemporaryLock, Start);

            LoginLock currentLock = _tracker.GetLock(OtherPlayerId, Start);

            Assert.Equal(LoginLock.None, currentLock);
        }

        [Fact]
        public void GetLock_LockSavedBeforeTheServerRestarted_KeepsTheAccountLocked()
        {
            FailTimes(PlayerId, LoginAttemptTracker.FailuresForTemporaryLock, Start);
            var restartedTracker = new LoginAttemptTracker(_players);

            LoginLock lockResult = restartedTracker.GetLock(PlayerId, Start.AddMinutes(FourMinutes));

            Assert.Equal(LoginLock.Temporary, lockResult);
        }

        [Fact]
        public void GetLock_SavedLockAlreadyEnded_ReturnsNone()
        {
            _players.SaveLock(PlayerId, LoginLock.Daily.ToString(), Start);

            LoginLock lockResult = _tracker.GetLock(PlayerId, Start.AddMinutes(TenMinutes));

            Assert.Equal(LoginLock.None, lockResult);
        }

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new LoginAttemptTracker(null));
        }

        [Fact]
        public void ToStatus_TemporaryLock_ReturnsTemporarilyLocked()
        {
            LoginStatus status = LoginAttemptTracker.ToStatus(LoginLock.Temporary);

            Assert.Equal(LoginStatus.TemporarilyLocked, status);
        }

        [Fact]
        public void ToStatus_DailyLock_ReturnsLockedForTheDay()
        {
            LoginStatus status = LoginAttemptTracker.ToStatus(LoginLock.Daily);

            Assert.Equal(LoginStatus.LockedForTheDay, status);
        }

        [Fact]
        public void ToStatus_LockForReports_ReturnsSuspendedByReports()
        {
            LoginStatus status = LoginAttemptTracker.ToStatus(LoginLock.Reported);

            Assert.Equal(LoginStatus.SuspendedByReports, status);
        }

        [Fact]
        public void RegisterFailure_WrongPasswordDuringATemporaryLock_KeepsTheAccountLocked()
        {
            FailTimes(PlayerId, LoginAttemptTracker.FailuresForTemporaryLock, Start);
            _tracker.RegisterFailure(PlayerId, Start.AddMinutes(FourMinutes));

            LoginLock currentLock = _tracker.GetLock(PlayerId, Start.AddMinutes(FourMinutes));

            Assert.Equal(LoginLock.Temporary, currentLock);
        }

        [Fact]
        public void RegisterSuccess_AccountWithoutFailures_DoesNotLock()
        {
            _tracker.RegisterSuccess(PlayerId);

            LoginLock currentLock = _tracker.GetLock(PlayerId, Start);

            Assert.Equal(LoginLock.None, currentLock);
        }

        private void FailTimes(int playerId, int failures, DateTime moment)
        {
            for (int failureIndex = 0; failureIndex < failures; failureIndex++)
            {
                _tracker.RegisterFailure(playerId, moment);
            }
        }
    }
}
