using System;

namespace YachtDice.Client.Logic.Game
{
    /// <summary>
    /// The seconds that the current turn has left, shown to the players. The server owns the real time: this
    /// class only counts down between the states that the server sends.
    /// </summary>
    public sealed class TurnCountdown
    {
        private const int NoSeconds = 0;
        private const int OneSecond = 1;

        /// <summary>
        /// Gets the seconds left in the turn.
        /// </summary>
        public int SecondsLeft { get; private set; }

        /// <summary>
        /// Sets the seconds left, for example when the server sends a new state.
        /// </summary>
        /// <param name="seconds">Seconds left. A negative value is taken as zero.</param>
        public void Reset(int seconds)
        {
            SecondsLeft = Math.Max(NoSeconds, seconds);
        }

        /// <summary>
        /// Counts one second. It never goes below zero.
        /// </summary>
        public void Tick()
        {
            SecondsLeft = Math.Max(NoSeconds, SecondsLeft - OneSecond);
        }
    }
}
