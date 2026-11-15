using YachtDice.Client.Logic.Game;

namespace YachtDice.Client.Logic.Connection
{
    /// <summary>
    /// The time rules to get back a connection that was lost during a match. The server gives an absent
    /// player <see cref="ReconnectWaitTracker.ReconnectSeconds"/> to come back, so trying after that time is
    /// useless.
    /// </summary>
    public static class ReconnectPolicy
    {
        /// <summary>
        /// Seconds that the client waits between one attempt to reconnect and the next one.
        /// </summary>
        public const int RetryIntervalSeconds = 5;

        /// <summary>
        /// Decides whether it is still worth trying to reconnect.
        /// </summary>
        /// <param name="elapsedSeconds">Seconds that passed since the connection was lost.</param>
        /// <returns>True while the server still waits for the player; otherwise, false.</returns>
        public static bool CanAttempt(int elapsedSeconds)
        {
            return elapsedSeconds < ReconnectWaitTracker.ReconnectSeconds;
        }
    }
}
