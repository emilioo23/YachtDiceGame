namespace YachtDice.Client.Logic.Game
{
    /// <summary>
    /// A player in the final scoreboard, with the place that the points give.
    /// </summary>
    public sealed class RankedStanding
    {
        /// <summary>
        /// Initializes a new standing.
        /// </summary>
        /// <param name="rank">Place of the player. Players with the same points share it.</param>
        /// <param name="entry">The player and the points.</param>
        public RankedStanding(int rank, StandingEntry entry)
        {
            Rank = rank;
            PlayerId = entry.PlayerId;
            Name = entry.Name;
            Total = entry.Total;
        }

        /// <summary>
        /// Gets the place of the player, starting at one.
        /// </summary>
        public int Rank { get; }

        /// <summary>
        /// Gets the identifier of the player.
        /// </summary>
        public string PlayerId { get; }

        /// <summary>
        /// Gets the name that the screens show for the player.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the total points of the player.
        /// </summary>
        public int Total { get; }

        /// <summary>
        /// Gets a value indicating whether the player has the first place.
        /// </summary>
        public bool IsWinner => Rank == StandingsBuilder.FirstRank;
    }
}
