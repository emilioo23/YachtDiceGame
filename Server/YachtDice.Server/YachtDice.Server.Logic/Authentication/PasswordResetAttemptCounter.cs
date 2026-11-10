using System.Collections.Generic;

namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Counts the wrong recovery codes entered for each player, so a code cannot be guessed by brute force.
    /// </summary>
    public class PasswordResetAttemptCounter
    {
        private const int NoFailedAttempts = 0;
        private const int OneAttempt = 1;

        private readonly object _lock = new object();
        private readonly Dictionary<int, int> _failedAttempts = new Dictionary<int, int>();

        /// <summary>
        /// Adds one wrong attempt for the player.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>The number of wrong attempts counted so far.</returns>
        public int RegisterFailure(int playerId)
        {
            int attempts;

            lock (_lock)
            {
                _failedAttempts.TryGetValue(playerId, out attempts);
                attempts += OneAttempt;
                _failedAttempts[playerId] = attempts;
            }

            return attempts;
        }

        /// <summary>
        /// Forgets the wrong attempts of the player.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        public void Reset(int playerId)
        {
            lock (_lock)
            {
                _failedAttempts.Remove(playerId);
            }
        }

        /// <summary>
        /// Gets the wrong attempts counted for the player.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>The number of wrong attempts, zero if there are none.</returns>
        public int GetFailures(int playerId)
        {
            int attempts;

            lock (_lock)
            {
                if (!_failedAttempts.TryGetValue(playerId, out attempts))
                {
                    attempts = NoFailedAttempts;
                }
            }

            return attempts;
        }
    }
}
