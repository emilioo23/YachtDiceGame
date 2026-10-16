namespace YachtDice.Server.Logic.Players
{
    /// <summary>
    /// Summary of the games played by a player.
    /// </summary>
    public class PlayerStatistics
    {
        /// <summary>
        /// Indicates whether the player exists; if not, the other values are empty.
        /// </summary>
        public bool IsPlayerFound { get; set; }

        /// <summary>
        /// Number of games played.
        /// </summary>
        public int GamesPlayed { get; set; }

        /// <summary>
        /// Number of games won.
        /// </summary>
        public int Wins { get; set; }

        /// <summary>
        /// Highest score obtained in a game.
        /// </summary>
        public int BestScore { get; set; }

        /// <summary>
        /// Percentage of games won, rounded to a whole number.
        /// </summary>
        public int WinRatePercentage { get; set; }

        /// <summary>
        /// Current level of the player.
        /// </summary>
        public int Level { get; set; }
    }
}