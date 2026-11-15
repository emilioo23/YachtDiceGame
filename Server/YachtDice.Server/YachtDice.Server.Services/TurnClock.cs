using System;
using System.Threading;

namespace YachtDice.Server.Services
{
    using LogicLogging = YachtDice.Server.Logic.Logging;

    /// <summary>
    /// The countdown of one turn of one game: when it ends and the timer that fires at that moment.
    /// It is the only entry point of the timer threads, so it is also the only place that catches what
    /// the expired action throws: an error without a catch in a timer thread closes the whole server.
    /// </summary>
    internal sealed class TurnClock : IDisposable
    {
        private const string ExpiredActionFailedLogMessage = "The action of an expired countdown failed.";

        private readonly Timer _timer;
        private readonly Action _onExpired;
        private readonly LogicLogging.AppLogger _logger;

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
            _onExpired = onExpired;
            _logger = new LogicLogging.AppLogger(GetType());
            _timer = new Timer(HandleExpired, null, TimeSpan.FromSeconds(seconds), Timeout.InfiniteTimeSpan);
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

        private void HandleExpired(object state)
        {
            try
            {
                _onExpired();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ExpiredActionFailedLogMessage);
            }
        }
    }
}
