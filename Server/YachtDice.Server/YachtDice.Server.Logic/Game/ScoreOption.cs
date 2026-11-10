namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// A box that the player can choose with the current dice and the points that the box would receive.
    /// </summary>
    public sealed class ScoreOption
    {
        /// <summary>
        /// Initializes a new option.
        /// </summary>
        /// <param name="category">Box that can be chosen.</param>
        /// <param name="points">Points that the box would receive.</param>
        public ScoreOption(ScoreCategory category, int points)
        {
            Category = category;
            Points = points;
        }

        /// <summary>
        /// Gets the box that can be chosen.
        /// </summary>
        public ScoreCategory Category { get; }

        /// <summary>
        /// Gets the points that the box would receive.
        /// </summary>
        public int Points { get; }
    }
}
