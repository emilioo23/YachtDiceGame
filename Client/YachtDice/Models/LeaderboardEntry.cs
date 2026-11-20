namespace YachtDice.Models
{
    /// <summary>
    /// Row of the ranking as it is shown in the leaderboard window.
    /// </summary>
    public class LeaderboardEntry
    {
        /// <summary>
        /// Gets or sets the position of the player in the ranking. Players with the same points and wins share it.
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Gets or sets the medal or the position shown next to the player.
        /// </summary>
        public string Medal { get; set; }

        /// <summary>
        /// Gets or sets the name of the player.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the avatar of the player.
        /// </summary>
        public int AvatarId { get; set; }

        /// <summary>
        /// Gets or sets the total points of the player.
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// Gets or sets the number of games won by the player.
        /// </summary>
        public int Wins { get; set; }

        /// <summary>
        /// Gets or sets the number of games that the player finished.
        /// </summary>
        public int GamesPlayed { get; set; }

        /// <summary>
        /// Gets or sets the current level of the player.
        /// </summary>
        public int Level { get; set; }
    }
}
