using System;
using System.Collections.Generic;

namespace YachtDice.Server.Services
{
    /// <summary>
    /// Counts the time of the turn of every game in progress. When the time of a turn is over it tells it to
    /// the owner, which plays the turn for the player.
    /// </summary>
    internal sealed class TurnTimer
    {
        private const int NoSecondsLeft = 0;

        private readonly object _syncRoot = new object();
        private readonly Dictionary<string, TurnClock> _clocks =
            new Dictionary<string, TurnClock>(StringComparer.OrdinalIgnoreCase);
        private readonly Action<string, int> _onExpired;

        /// <summary>
        /// Initializes a timer without clocks.
        /// </summary>
        /// <param name="onExpired">Action to run with the room code and the turn number when a turn ends.</param>
        public TurnTimer(Action<string, int> onExpired)
        {
            _onExpired = onExpired;
        }

        /// <summary>
        /// Starts counting a turn. The countdown of the previous turn of the same room is cancelled.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="turnNumber">Number of the turn.</param>
        /// <param name="seconds">Seconds that the player has.</param>
        public void Start(string roomCode, int turnNumber, int seconds)
        {
            var clock = new TurnClock(turnNumber, seconds, () => _onExpired(roomCode, turnNumber));

            lock (_syncRoot)
            {
                DisposeClock(roomCode);
                _clocks[roomCode] = clock;
            }
        }

        /// <summary>
        /// Stops counting the turn of a room.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        public void Stop(string roomCode)
        {
            lock (_syncRoot)
            {
                DisposeClock(roomCode);
            }
        }

        /// <summary>
        /// Tells whether a turn is already being counted.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="turnNumber">Number of the turn.</param>
        /// <returns>True if the clock of the room counts that turn.</returns>
        public bool IsCounting(string roomCode, int turnNumber)
        {
            bool isCounting;

            lock (_syncRoot)
            {
                isCounting = _clocks.ContainsKey(roomCode) && _clocks[roomCode].TurnNumber == turnNumber;
            }

            return isCounting;
        }

        /// <summary>
        /// Gets the seconds that the current turn of a room has left.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <returns>The seconds left, or zero if no turn is counted.</returns>
        public int GetSecondsLeft(string roomCode)
        {
            var secondsLeft = NoSecondsLeft;

            lock (_syncRoot)
            {
                if (_clocks.ContainsKey(roomCode))
                {
                    var remaining = _clocks[roomCode].Deadline - DateTime.UtcNow;
                    secondsLeft = Math.Max(NoSecondsLeft, (int)Math.Ceiling(remaining.TotalSeconds));
                }
            }

            return secondsLeft;
        }

        // The caller must hold _syncRoot.
        private void DisposeClock(string roomCode)
        {
            if (_clocks.ContainsKey(roomCode))
            {
                _clocks[roomCode].Dispose();
                _clocks.Remove(roomCode);
            }
        }
    }
}
