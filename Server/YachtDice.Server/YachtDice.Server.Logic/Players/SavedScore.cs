namespace YachtDice.Server.Logic.Players
{
    /// <summary>
    /// Points of one category of a scorecard of a past game.
    /// </summary>
    public class SavedScore
    {
        /// <summary>
        /// Gets or sets the name of the category.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the points written in the category.
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the player filled the category.
        /// </summary>
        public bool IsUsed { get; set; }
    }
}
