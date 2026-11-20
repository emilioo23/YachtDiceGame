using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;

namespace YachtDice.Server.Logic.Passkeys
{
    /// <summary>
    /// Keeps in memory the challenges that were issued and are waiting for an answer. A challenge can be used
    /// only once and only before it expires, so an answer that someone copies cannot be played again.
    /// </summary>
    public sealed class PasskeyChallengeStore
    {
        private const string NullClockMessage = "The clock cannot be null.";

        private readonly ConcurrentDictionary<string, PendingChallenge> _pending =
            new ConcurrentDictionary<string, PendingChallenge>();

        private readonly Func<DateTime> _clock;

        /// <summary>
        /// Initializes a store that uses the clock of the computer.
        /// </summary>
        public PasskeyChallengeStore()
            : this(() => DateTime.UtcNow)
        {
        }

        /// <summary>
        /// Initializes a store with the clock that decides when a challenge expires.
        /// </summary>
        /// <param name="clock">Function that gives the current moment in UTC.</param>
        /// <exception cref="ArgumentNullException">The clock is null.</exception>
        public PasskeyChallengeStore(Func<DateTime> clock)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock), NullClockMessage);
        }

        /// <summary>
        /// Issues a new challenge.
        /// </summary>
        /// <param name="purpose">What the challenge is for.</param>
        /// <param name="playerId">Player that asked for it; zero for a login.</param>
        /// <returns>The challenge, or null if too many challenges are waiting.</returns>
        public PendingChallenge Issue(PasskeyPurpose purpose, int playerId)
        {
            PendingChallenge pending = null;

            RemoveExpired();

            if (_pending.Count < PasskeyRules.MaxPendingChallenges)
            {
                pending = new PendingChallenge(
                    Base64Url.Encode(CreateRandom()),
                    CreateRandom(),
                    _clock().AddMinutes(PasskeyRules.ChallengeMinutes))
                {
                    Purpose = purpose,
                    PlayerId = playerId
                };
                _pending[pending.Id] = pending;
            }

            return pending;
        }

        /// <summary>
        /// Takes a challenge out of the store. It is removed even if it is not valid, so it cannot be tried twice.
        /// </summary>
        /// <param name="challengeId">Identifier that the client sent.</param>
        /// <param name="purpose">Purpose that the challenge must have.</param>
        /// <param name="pending">The challenge; null if it was not found, expired or has another purpose.</param>
        /// <returns>True if the challenge can be used.</returns>
        public bool TryTake(string challengeId, PasskeyPurpose purpose, out PendingChallenge pending)
        {
            bool isUsable = false;

            pending = null;

            if (!string.IsNullOrEmpty(challengeId) && _pending.TryRemove(challengeId, out PendingChallenge found))
            {
                isUsable = found.Purpose == purpose && found.ExpiresAt > _clock();

                if (isUsable)
                {
                    pending = found;
                }
            }

            return isUsable;
        }

        private void RemoveExpired()
        {
            DateTime now = _clock();
            Func<PendingChallenge, bool> hasExpired = item => item.ExpiresAt <= now;
            string[] expiredIds = _pending.Values
                .Where(hasExpired)
                .Select(item => item.Id)
                .ToArray();

            foreach (string expiredId in expiredIds)
            {
                _pending.TryRemove(expiredId, out PendingChallenge removed);
            }
        }

        private static byte[] CreateRandom()
        {
            var bytes = new byte[PasskeyRules.ChallengeSize];

            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(bytes);
            }

            return bytes;
        }
    }
}
