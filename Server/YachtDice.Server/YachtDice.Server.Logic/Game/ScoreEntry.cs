namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// A box of a scorecard, as it is at some moment.
    /// </summary>
    public sealed class ScoreEntry
    {
        /// <summary>
        /// Initializes a new entry.
        /// </summary>
        /// <param name="category">The box.</param>
        /// <param name="isFilled">True if the box already has points.</param>
        /// <param name="points">Points of the box, zero while it is open.</param>
        public ScoreEntry(ScoreCategory category, bool isFilled, int points)
        {
            Category = category;
            IsFilled = isFilled;
            Points = points;
        }

        /// <summary>
        /// Gets the box.
        /// </summary>
        public ScoreCategory Category { get; }

        /// <summary>
        /// Gets a value indicating whether the box already has points.
        /// </summary>
        public bool IsFilled { get; }

        /// <summary>
        /// Gets the points of the box.
        /// </summary>
        public int Points { get; }
    }
}
