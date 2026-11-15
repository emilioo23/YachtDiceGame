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
        /// <param name="standingEntry">The player and the points.</param>
        public RankedStanding(int rank, StandingEntry standingEntry)
        {
            Rank = rank;
            PlayerId = standingEntry.PlayerId;
            Name = standingEntry.Name;
            Total = standingEntry.Total;
            ExperienceGained = standingEntry.ExperienceGained;
            UpperSubtotal = standingEntry.UpperSubtotal;
            UpperBonus = standingEntry.UpperBonus;
            YachtBonus = standingEntry.YachtBonus;
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
        /// Gets the experience that the player earned in the game.
        /// </summary>
        public int ExperienceGained { get; }

        /// <summary>
        /// Gets the points of the upper section, before its bonus.
        /// </summary>
        public int UpperSubtotal { get; }

        /// <summary>
        /// Gets the bonus of the upper section.
        /// </summary>
        public int UpperBonus { get; }

        /// <summary>
        /// Gets the bonus of the extra Yachts.
        /// </summary>
        public int YachtBonus { get; }

        /// <summary>
        /// Gets the points of the lower section: what is left of the total without the upper section and the
        /// bonuses.
        /// </summary>
        public int LowerTotal => Total - UpperSubtotal - UpperBonus - YachtBonus;

        /// <summary>
        /// Gets a value indicating whether the player has the first place.
        /// </summary>
        public bool IsWinner => Rank == StandingsBuilder.FirstRank;
    }
}
