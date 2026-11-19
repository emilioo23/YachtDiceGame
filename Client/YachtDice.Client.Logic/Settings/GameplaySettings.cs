namespace YachtDice.Client.Logic.Settings
{
    /// <summary>
    /// What the player chose about the help that the game gives while playing. It does not draw anything; the
    /// windows read it when they show the scorecard.
    /// </summary>
    public sealed class GameplaySettings
    {
        /// <summary>
        /// Gets a value indicating whether the points that a category would give are shown before the player
        /// chooses it.
        /// </summary>
        public bool ShowPotentialPoints { get; private set; } = true;

        /// <summary>
        /// Turns on or off the preview of the points of each category.
        /// </summary>
        /// <param name="isShown">True to show the points before scoring; false to hide them.</param>
        public void SetShowPotentialPoints(bool isShown)
        {
            ShowPotentialPoints = isShown;
        }
    }
}
