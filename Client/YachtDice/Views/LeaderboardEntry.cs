namespace YachtDice.Views
{
    /// <summary>
    /// Row of the ranking as it is shown in the leaderboard window.
    /// </summary>
    public class LeaderboardEntry
    {
        /// <summary>
        /// Gets or sets the medal or the position shown next to the player.
        /// </summary>
        public string Medal { get; set; }

        /// <summary>
        /// Gets or sets the name of the player.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the total points of the player.
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// Gets or sets the number of games won by the player.
        /// </summary>
        public int Wins { get; set; }
    }
}
