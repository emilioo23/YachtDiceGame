namespace YachtDice.Server.Logic.Players
{
    /// <summary>
    /// Shows an achievement and whether a player has already unlocked it.
    /// </summary>
    public class AchievementStatus
    {
        /// <summary>
        /// Gets or sets the identifier of the achievement.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the player has unlocked the achievement.
        /// </summary>
        public bool IsUnlocked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the achievement unlocks a title.
        /// </summary>
        public bool HasTitle { get; set; }
    }
}
