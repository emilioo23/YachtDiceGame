using System.Collections.Generic;

namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Counts the wrong recovery codes entered for each player, so a code cannot be guessed by brute force.
    /// </summary>
    public class PasswordResetAttemptCounter
    {
        private const int OneAttempt = 1;

        private readonly object _syncRoot = new object();
        private readonly Dictionary<int, int> _failedAttempts = new Dictionary<int, int>();

        /// <summary>
        /// Adds one wrong attempt for the player.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>The number of wrong attempts counted so far.</returns>
        public int RegisterFailure(int playerId)
        {
            int attempts;

            lock (_syncRoot)
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
            lock (_syncRoot)
            {
                _failedAttempts.Remove(playerId);
            }
        }
    }
}
