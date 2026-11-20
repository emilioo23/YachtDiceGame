namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Points of one category of a saved scorecard.
    /// </summary>
    public class ScoreRecord
    {
        /// <summary>
        /// Gets or sets the name of the category, the same name that the Category table has.
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// Gets or sets the points written in the category.
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the player filled the category before the game ended.
        /// </summary>
        public bool IsUsed { get; set; }
    }
}
