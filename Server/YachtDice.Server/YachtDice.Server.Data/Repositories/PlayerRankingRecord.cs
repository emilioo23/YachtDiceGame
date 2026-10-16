namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Total points and wins accumulated by a player across all the games played.
    /// </summary>
    public class PlayerRankingRecord
    {
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