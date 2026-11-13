using System;
using System.Collections.Generic;
using System.Globalization;
using YachtDice.Server.Logic.Rooms;

namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// The time that a player has to play a turn, according to the speed of the game. The seconds of each speed
    /// come from the configuration of the server, so a new speed only needs its own value there.
    /// </summary>
    public sealed class TurnDuration
    {
        private const int MinimumSeconds = 1;
        private const string NullSecondsMessage = "The seconds of each speed cannot be null.";
        private const string InvalidSecondsFormat = "The turn of the speed {0} must last at least one second.";
        private const string MissingSpeedFormat = "There is no turn duration for the speed {0}.";

        private readonly Dictionary<GameSpeed, int> _secondsBySpeed;

        /// <summary>
        /// Initializes the durations with the seconds of each speed.
        /// </summary>
        /// <param name="secondsBySpeed">Seconds that a turn lasts for each speed of the game.</param>
        /// <exception cref="ArgumentNullException">The seconds are null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A speed has less than one second.</exception>
        public TurnDuration(IDictionary<GameSpeed, int> secondsBySpeed)
        {
            if (secondsBySpeed == null)
            {
                throw new ArgumentNullException(nameof(secondsBySpeed), NullSecondsMessage);
            }

            foreach (KeyValuePair<GameSpeed, int> speedSeconds in secondsBySpeed)
            {
                ValidateSeconds(speedSeconds);
            }

            _secondsBySpeed = new Dictionary<GameSpeed, int>(secondsBySpeed);
        }

        /// <summary>
        /// Gets the seconds of a turn.
        /// </summary>
        /// <param name="speed">Speed chosen for the game.</param>
        /// <returns>The seconds that a player has to play a turn.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The speed has no duration configured.</exception>
        public int GetSeconds(GameSpeed speed)
        {
            if (!_secondsBySpeed.TryGetValue(speed, out int seconds))
            {
                string message = string.Format(CultureInfo.InvariantCulture, MissingSpeedFormat, speed);

                throw new ArgumentOutOfRangeException(nameof(speed), message);
            }

            return seconds;
        }

        private static void ValidateSeconds(KeyValuePair<GameSpeed, int> speedSeconds)
        {
            if (speedSeconds.Value < MinimumSeconds)
            {
                string message = string.Format(CultureInfo.InvariantCulture, InvalidSecondsFormat, speedSeconds.Key);

                throw new ArgumentOutOfRangeException(nameof(speedSeconds), message);
            }
        }
    }
}
