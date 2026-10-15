namespace YachtDice.Server.Logic.Players
{
    /// <summary>
    /// One row of the ranking of players.
    /// </summary>
    public class LeaderboardEntry
    {
        /// <summary>
        /// Place of the player in the ranking, starting at 1.
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Display name of the player.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Sum of the final scores of all the games of the player.
        /// </summary>
        public int TotalPoints { get; set; }

        /// <summary>
        /// Number of games won by the player.
        /// </summary>
        public int Wins { get; set; }
    }
}