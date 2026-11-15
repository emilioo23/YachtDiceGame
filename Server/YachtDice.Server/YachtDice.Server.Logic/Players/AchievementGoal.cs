namespace YachtDice.Server.Logic.Players
{
    /// <summary>
    /// Statistic that an achievement measures and the value that the statistic must reach.
    /// </summary>
    public sealed class AchievementGoal
    {
        /// <summary>
        /// Initializes the goal of an achievement.
        /// </summary>
        /// <param name="metric">Statistic that the achievement measures.</param>
        /// <param name="requiredValue">Value of the statistic that unlocks the achievement.</param>
        public AchievementGoal(AchievementMetric metric, int requiredValue)
        {
            Metric = metric;
            RequiredValue = requiredValue;
        }

        /// <summary>
        /// Gets the statistic that the achievement measures.
        /// </summary>
        public AchievementMetric Metric { get; }

        /// <summary>
        /// Gets the value of the statistic that unlocks the achievement.
        /// </summary>
        public int RequiredValue { get; }
    }
}
