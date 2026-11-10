using System;
using System.Threading;

namespace YachtDice.Server.Services
{
    /// <summary>
    /// The countdown of one turn of one game: when it ends and the timer that fires at that moment.
    /// </summary>
    internal sealed class TurnClock : IDisposable
    {
        private readonly Timer _timer;

        /// <summary>
        /// Starts the countdown of a turn.
        /// </summary>
        /// <param name="turnNumber">Number of the turn that is counted.</param>
        /// <param name="seconds">Seconds until the time is over.</param>
        /// <param name="onExpired">Action to run when the time is over.</param>
        public TurnClock(int turnNumber, int seconds, Action onExpired)
        {
            TurnNumber = turnNumber;
            Deadline = DateTime.UtcNow.AddSeconds(seconds);
            _timer = new Timer(state => onExpired(), null, TimeSpan.FromSeconds(seconds), Timeout.InfiniteTimeSpan);
        }

        /// <summary>
        /// Gets the number of the turn that is counted.
        /// </summary>
        public int TurnNumber { get; }

        /// <summary>
        /// Gets the moment in which the time of the turn is over.
        /// </summary>
        public DateTime Deadline { get; }

        /// <summary>
        /// Stops the countdown.
        /// </summary>
        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
