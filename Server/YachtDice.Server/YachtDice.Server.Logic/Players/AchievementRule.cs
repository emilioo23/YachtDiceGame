namespace YachtDice.Server.Logic.Players
{
    /// <summary>
    /// Condition that a player must meet to unlock an achievement and, when it has one, a title.
    /// </summary>
    public sealed class AchievementRule
    {
        /// <summary>
        /// Initializes the rule of an achievement.
        /// </summary>
        /// <param name="id">Identifier of the achievement, which is also the identifier of its title.</param>
        /// <param name="metric">Statistic that the achievement measures.</param>
        /// <param name="requiredValue">Value of the statistic that unlocks the achievement.</param>
        /// <param name="hasTitle">True if unlocking the achievement also unlocks a title.</param>
        public AchievementRule(int id, AchievementMetric metric, int requiredValue, bool hasTitle)
        {
            Id = id;
            Metric = metric;
            RequiredValue = requiredValue;
            HasTitle = hasTitle;
        }

        /// <summary>
        /// Gets the identifier of the achievement.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets the statistic that the achievement measures.
        /// </summary>
        public AchievementMetric Metric { get; }

        /// <summary>
        /// Gets the value of the statistic that unlocks the achievement.
        /// </summary>
        public int RequiredValue { get; }

        /// <summary>
        /// Gets a value indicating whether the achievement unlocks a title.
        /// </summary>
        public bool HasTitle { get; }

        /// <summary>
        /// Gets or sets the games a player must have played before the rule is checked.
        /// </summary>
        public int MinimumGames { get; set; }
    }
}
