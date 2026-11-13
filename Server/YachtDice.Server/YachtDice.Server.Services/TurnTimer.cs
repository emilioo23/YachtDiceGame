using System;
using System.Collections.Generic;

namespace YachtDice.Server.Services
{
    using LogicGame = YachtDice.Server.Logic.Game;

    /// <summary>
    /// Counts the time of the turn of every game in progress. When the time of a turn is over it tells it to
    /// the owner, which plays the turn for the player. The private steps only run while the lock is held.
    /// </summary>
    internal sealed class TurnTimer
    {
        private const int NoSecondsLeft = 0;

        private readonly object _syncRoot = new object();
        private readonly Dictionary<string, TurnClock> _clocks =
            new Dictionary<string, TurnClock>(StringComparer.OrdinalIgnoreCase);
        private readonly Action<LogicGame.GameTurn> _onExpired;

        /// <summary>
        /// Initializes a timer without clocks.
        /// </summary>
        /// <param name="onExpired">Action to run with the room code and the turn number when a turn ends.</param>
        public TurnTimer(Action<LogicGame.GameTurn> onExpired)
        {
            _onExpired = onExpired;
        }

        /// <summary>
        /// Starts counting a turn. The countdown of the previous turn of the same room is cancelled.
        /// </summary>
        /// <param name="turn">Turn to count, with the code of its room and its number.</param>
        /// <param name="seconds">Seconds that the player has.</param>
        public void Start(LogicGame.GameTurn turn, int seconds)
        {
            var clock = new TurnClock(turn.TurnNumber, seconds, () => _onExpired(turn));

            lock (_syncRoot)
            {
                DisposeClock(turn.RoomCode);
                _clocks[turn.RoomCode] = clock;
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
        /// <param name="turn">Turn to look for, with the code of its room and its number.</param>
        /// <returns>True if the clock of the room counts that turn.</returns>
        public bool IsCounting(LogicGame.GameTurn turn)
        {
            bool isCounting;

            lock (_syncRoot)
            {
                isCounting = _clocks.ContainsKey(turn.RoomCode)
                    && _clocks[turn.RoomCode].TurnNumber == turn.TurnNumber;
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
