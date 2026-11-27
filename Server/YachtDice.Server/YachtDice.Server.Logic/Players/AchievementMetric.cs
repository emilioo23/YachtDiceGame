namespace YachtDice.Server.Logic.Players
{
    /// <summary>
    /// Statistic of a player that an achievement measures.
    /// </summary>
    public enum AchievementMetric
    {
        /// <summary>
        /// Number of games played.
        /// </summary>
        GamesPlayed,

        /// <summary>
        /// Number of games won.
        /// </summary>
        Wins,

        /// <summary>
        /// Highest score in a game.
        /// </summary>
        BestScore,

        /// <summary>
        /// Percentage of games won.
        /// </summary>
        WinRatePercentage,

        /// <summary>
        /// Level of the player.
        /// </summary>
        Level,

        /// <summary>
        /// Total experience of the player.
        /// </summary>
        Experience,

        /// <summary>
        /// Number of games played against the computer.
        /// </summary>
        GamesAgainstComputer,

        /// <summary>
        /// Number of games won against the computer.
        /// </summary>
        WinsAgainstComputer
    }
}
