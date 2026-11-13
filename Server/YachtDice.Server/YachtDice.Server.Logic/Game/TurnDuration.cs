using YachtDice.Server.Logic.Rooms;

namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// The time that a player has to play a turn, according to the speed of the game.
    /// </summary>
    public static class TurnDuration
    {
        private const int NormalSeconds = 60;
        private const int FastSeconds = 30;

        /// <summary>
        /// Gets the seconds of a turn.
        /// </summary>
        /// <param name="speed">Speed chosen for the game.</param>
        /// <returns>The seconds that a player has to play a turn.</returns>
        public static int GetSeconds(GameSpeed speed)
        {
            int seconds;

            switch (speed)
            {
                case GameSpeed.Fast:
                    seconds = FastSeconds;
                    break;
                default:
                    seconds = NormalSeconds;
                    break;
            }

            return seconds;
        }
    }
}
