using System;
using Xunit;
using YachtDice.Server.Logic.Passkeys;

namespace YachtDice.Server.Tests.Passkeys
{
    /// <summary>
    /// Rules of the store of the challenges that wait for an answer.
    /// </summary>
    public class PasskeyChallengeStoreTests
    {
        private const int PlayerId = 1;
        private const int MinutesAfterExpiration = 1;
        private static readonly DateTime Start = new DateTime(2026, 11, 18, 12, 0, 0, DateTimeKind.Utc);

        private DateTime _now = Start;
        private readonly PasskeyChallengeStore _store;

        /// <summary>
        /// Prepares a store whose clock the tests can move.
        /// </summary>
        public PasskeyChallengeStoreTests()
        {
            _store = new PasskeyChallengeStore(() => _now);
        }

        [Fact]
        public void TryTake_IssuedChallenge_ReturnsTrue()
        {
            PendingChallenge issued = _store.Issue(PasskeyPurpose.Login, 0);

            bool isUsable = _store.TryTake(issued.Id, PasskeyPurpose.Login, out PendingChallenge pending);

            Assert.True(isUsable);
        }

        [Fact]
        public void TryTake_ChallengeTakenBefore_ReturnsFalse()
        {
            PendingChallenge issued = _store.Issue(PasskeyPurpose.Login, 0);

            _store.TryTake(issued.Id, PasskeyPurpose.Login, out PendingChallenge firstTime);
            bool isUsable = _store.TryTake(issued.Id, PasskeyPurpose.Login, out PendingChallenge secondTime);

            Assert.False(isUsable);
        }

        [Fact]
        public void TryTake_ExpiredChallenge_ReturnsFalse()
        {
            PendingChallenge issued = _store.Issue(PasskeyPurpose.Login, 0);

            _now = issued.ExpiresAt.AddMinutes(MinutesAfterExpiration);
            bool isUsable = _store.TryTake(issued.Id, PasskeyPurpose.Login, out PendingChallenge pending);

            Assert.False(isUsable);
        }

        [Fact]
        public void TryTake_ChallengeOfAnotherPurpose_ReturnsFalse()
        {
            PendingChallenge issued = _store.Issue(PasskeyPurpose.Registration, PlayerId);

            bool isUsable = _store.TryTake(issued.Id, PasskeyPurpose.Login, out PendingChallenge pending);

            Assert.False(isUsable);
        }

        [Fact]
        public void TryTake_UnknownIdentifier_ReturnsFalse()
        {
            bool isUsable = _store.TryTake("unknown", PasskeyPurpose.Login, out PendingChallenge pending);

            Assert.False(isUsable);
        }

        [Fact]
        public void Issue_TooManyChallengesWaiting_ReturnsNull()
        {
            for (int index = 0; index < PasskeyRules.MaxPendingChallenges; index++)
            {
                _store.Issue(PasskeyPurpose.Login, 0);
            }

            PendingChallenge issued = _store.Issue(PasskeyPurpose.Login, 0);

            Assert.Null(issued);
        }

        [Fact]
        public void Constructor_NullClock_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PasskeyChallengeStore(null));
        }
    }
}
