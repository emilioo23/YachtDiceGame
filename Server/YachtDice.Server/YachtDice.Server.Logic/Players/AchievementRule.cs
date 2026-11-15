using System;

namespace YachtDice.Server.Logic.Players
{
    /// <summary>
    /// Condition that a player must meet to unlock an achievement and, when it has one, a title.
    /// </summary>
    public sealed class AchievementRule
    {
        private const string NullGoalMessage = "The goal of the achievement cannot be null.";

        /// <summary>
        /// Initializes the rule of an achievement.
        /// </summary>
        /// <param name="id">Identifier of the achievement, which is also the identifier of its title.</param>
        /// <param name="goal">Statistic that the achievement measures and the value that unlocks it.</param>
        /// <param name="hasTitle">True if unlocking the achievement also unlocks a title.</param>
        /// <exception cref="ArgumentNullException">The goal is null.</exception>
        public AchievementRule(int id, AchievementGoal goal, bool hasTitle)
        {
            Id = id;
            Goal = goal ?? throw new ArgumentNullException(nameof(goal), NullGoalMessage);
            HasTitle = hasTitle;
        }

        /// <summary>
        /// Gets the identifier of the achievement.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets the statistic that the achievement measures and the value that unlocks it.
        /// </summary>
        public AchievementGoal Goal { get; }

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
