using System;
using System.Threading.Tasks;
using YachtDice.Client.Logic.Game;
using YachtDice.Client.Logic.Logging;

namespace YachtDice.Client.Logic.Connection
{
    /// <summary>
    /// Tries again and again to get back the connection of a match, waiting between the attempts, until it
    /// works or the server does not wait for the player anymore (after
    /// <see cref="ReconnectWaitTracker.ReconnectSeconds"/>). What an attempt does and how the time
    /// passes are given by the caller, so the loop does not depend on the network or on the clock.
    /// </summary>
    public sealed class ReconnectLoop
    {
        /// <summary>
        /// Seconds that the client waits between one attempt to reconnect and the next one.
        /// </summary>
        public const int RetryIntervalSeconds = 5;

        private const int NoAttempts = 0;
        private const int OneAttempt = 1;

        private readonly Func<Task<bool>> _tryRestore;
        private readonly Func<int, Task> _wait;
        private readonly Func<int> _getElapsedSeconds;

        /// <summary>
        /// Initializes the loop.
        /// </summary>
        /// <param name="tryRestore">One attempt to get the connection back; it tells whether it worked.</param>
        /// <param name="wait">Waits the given seconds between two attempts.</param>
        /// <param name="getElapsedSeconds">Gives the seconds that passed since the connection was lost.</param>
        /// <exception cref="ArgumentNullException">Any of the delegates is null.</exception>
        public ReconnectLoop(Func<Task<bool>> tryRestore, Func<int, Task> wait, Func<int> getElapsedSeconds)
        {
            _tryRestore = tryRestore ?? throw new ArgumentNullException(nameof(tryRestore),
                LogMessages.NullDelegateMessage);
            _wait = wait ?? throw new ArgumentNullException(nameof(wait), LogMessages.NullDelegateMessage);
            _getElapsedSeconds = getElapsedSeconds
                ?? throw new ArgumentNullException(nameof(getElapsedSeconds), LogMessages.NullDelegateMessage);
        }

        /// <summary>
        /// Event triggered when an attempt starts, with the number of the attempt; the first one is 1.
        /// </summary>
        public event Action<int> AttemptStarted;

        /// <summary>
        /// Tries to get the connection back until it works or the time of the server is over.
        /// </summary>
        /// <returns>True if the connection came back; false if the time ran out.</returns>
        public async Task<bool> RunAsync()
        {
            int attempt = NoAttempts;
            var isRestored = false;

            while (!isRestored && _getElapsedSeconds() < ReconnectWaitTracker.ReconnectSeconds)
            {
                attempt += OneAttempt;
                AttemptStarted?.Invoke(attempt);
                isRestored = await _tryRestore();

                if (!isRestored)
                {
                    await _wait(RetryIntervalSeconds);
                }
            }

            return isRestored;
        }
    }
}
