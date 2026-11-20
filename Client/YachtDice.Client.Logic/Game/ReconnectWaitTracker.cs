using System;
using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Client.Logic.Game
{
    /// <summary>
    /// The seconds that each absent player has left to come back to the match. The server owns the real
    /// time and takes the player out when it runs out: this class only counts down between the states that
    /// the server sends, so the players can see how long they have to wait.
    /// </summary>
    public sealed class ReconnectWaitTracker
    {
        /// <summary>
        /// Seconds that an absent player has to come back. It must match the time of the server.
        /// </summary>
        public const int ReconnectSeconds = 60;

        private const int NoSeconds = 0;
        private const int OneSecond = 1;
        private const int ExpiredToleranceSeconds = 2;

        private readonly Dictionary<string, int> _secondsLeft = new Dictionary<string, int>();
        private readonly HashSet<string> _reported = new HashSet<string>();

        /// <summary>
        /// Updates who is absent. A player who just left starts with the full time; a player who came back
        /// is forgotten.
        /// </summary>
        /// <param name="absentPlayerIds">Identifiers of the players who are not connected to the match.</param>
        public void Update(IEnumerable<string> absentPlayerIds)
        {
            var absent = new HashSet<string>(absentPlayerIds);

            Func<string, bool> hasComeBack = id => !absent.Contains(id);
            Func<string, bool> justLeft = id => !_secondsLeft.ContainsKey(id);

            foreach (string playerId in _secondsLeft.Keys.Where(hasComeBack).ToList())
            {
                _secondsLeft.Remove(playerId);
                _reported.Remove(playerId);
            }

            foreach (string playerId in absent.Where(justLeft).ToList())
            {
                _secondsLeft[playerId] = ReconnectSeconds;
            }
        }

        /// <summary>
        /// Counts one second for every absent player. The time never goes below zero.
        /// </summary>
        public void Tick()
        {
            foreach (string playerId in _secondsLeft.Keys.ToList())
            {
                _secondsLeft[playerId] = Math.Max(NoSeconds, _secondsLeft[playerId] - OneSecond);
            }
        }

        /// <summary>
        /// Gets the players that the match is still waiting for.
        /// </summary>
        /// <returns>The identifiers of the absent players who still have time.</returns>
        public IReadOnlyList<string> GetWaitingPlayerIds()
        {
            Func<KeyValuePair<string, int>, bool> hasTimeLeft = absentPlayer => absentPlayer.Value > NoSeconds;

            return _secondsLeft.Where(hasTimeLeft).Select(absentPlayer => absentPlayer.Key).ToList();
        }

        /// <summary>
        /// Gets the seconds that an absent player has left.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>The seconds left, or zero if the player is not absent.</returns>
        public int GetSecondsLeft(string playerId)
        {
            return _secondsLeft.ContainsKey(playerId) ? _secondsLeft[playerId] : NoSeconds;
        }

        /// <summary>
        /// Takes the players whose time is over and were not reported yet. A small margin covers the delay
        /// between the clock of the server and the clock of the player.
        /// </summary>
        /// <returns>The identifiers of the players who did not come back in time.</returns>
        public IReadOnlyList<string> TakeExpiredPlayerIds()
        {
            Func<KeyValuePair<string, int>, bool> isExpiredAndNotReported =
                absentPlayer => absentPlayer.Value <= ExpiredToleranceSeconds && !_reported.Contains(absentPlayer.Key);
            List<string> expired = _secondsLeft
                .Where(isExpiredAndNotReported)
                .Select(absentPlayer => absentPlayer.Key)
                .ToList();

            foreach (string playerId in expired)
            {
                _reported.Add(playerId);
                _secondsLeft[playerId] = NoSeconds;
            }

            return expired;
        }
    }
}
