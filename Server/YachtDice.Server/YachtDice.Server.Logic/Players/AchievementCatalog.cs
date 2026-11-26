using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Data.Logging;

namespace YachtDice.Server.Logic.Players
{
    /// <summary>
    /// Achievements of the game, read from the configuration of the server, and the rules that decide which ones a
    /// player has unlocked.
    /// </summary>
    public sealed class AchievementCatalog
    {
        /// <summary>
        /// Identifier of the title that every player has from the start.
        /// </summary>
        public const int DefaultTitleId = 0;

        private const int NoMeasuredValue = 0;

        private readonly IReadOnlyList<AchievementRule> _rules;

        /// <summary>
        /// Initializes the catalog with the rules of the achievements, in the order in which they are shown.
        /// </summary>
        /// <param name="rules">Rules of the achievements.</param>
        /// <exception cref="ArgumentNullException">The rules are null.</exception>
        /// <exception cref="ArgumentException">Two rules share an identifier or one uses the default title.</exception>
        public AchievementCatalog(IEnumerable<AchievementRule> rules)
        {
            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules), LogMessages.NullRulesMessage);
            }

            List<AchievementRule> ruleList = rules.ToList();
            bool hasRepeatedId = ruleList.Select(rule => rule.Id).Distinct().Count() != ruleList.Count;
            bool usesDefaultTitle = ruleList.Any(IsDefaultTitleRule);

            if (hasRepeatedId)
            {
                throw new ArgumentException(LogMessages.RepeatedIdMessage, nameof(rules));
            }

            if (usesDefaultTitle)
            {
                throw new ArgumentException(LogMessages.DefaultTitleIdMessage, nameof(rules));
            }

            _rules = ruleList;
        }

        /// <summary>
        /// Gets the rules of all the achievements, in the order in which they are shown.
        /// </summary>
        public IReadOnlyList<AchievementRule> Rules => _rules;

        /// <summary>
        /// Decides if the statistics of a player unlock an achievement.
        /// </summary>
        /// <param name="rule">Rule of the achievement.</param>
        /// <param name="statistics">Statistics of the player.</param>
        /// <returns>True if the player has unlocked the achievement.</returns>
        public static bool IsUnlocked(AchievementRule rule, PlayerStatistics statistics)
        {
            bool hasPlayedEnough = statistics.GamesPlayed >= rule.MinimumGames;

            return hasPlayedEnough && Measure(rule.Metric, statistics) >= rule.RequiredValue;
        }

        /// <summary>
        /// Gets the identifiers of the achievements that the statistics of a player unlock.
        /// </summary>
        /// <param name="statistics">Statistics of the player.</param>
        /// <returns>The identifiers of the unlocked achievements.</returns>
        public List<int> GetUnlockedIds(PlayerStatistics statistics)
        {
            var unlockedIds = new List<int>();

            foreach (AchievementRule rule in _rules)
            {
                if (IsUnlocked(rule, statistics))
                {
                    unlockedIds.Add(rule.Id);
                }
            }

            return unlockedIds;
        }

        /// <summary>
        /// Decides if a player can use a title.
        /// </summary>
        /// <param name="titleId">Identifier of the title.</param>
        /// <param name="unlockedIds">Identifiers of the achievements that the player has unlocked.</param>
        /// <returns>True if the title is the default one or its achievement is unlocked.</returns>
        public bool IsTitleAvailable(int titleId, List<int> unlockedIds)
        {
            return titleId == DefaultTitleId || (ExistsTitle(titleId) && unlockedIds.Contains(titleId));
        }

        /// <summary>
        /// Decides if a title exists.
        /// </summary>
        /// <param name="titleId">Identifier of the title.</param>
        /// <returns>True if it is the default title or the title of an achievement.</returns>
        public bool ExistsTitle(int titleId)
        {
            Func<AchievementRule, bool> hasThatTitle = rule => rule.Id == titleId && rule.HasTitle;

            return titleId == DefaultTitleId || _rules.Any(hasThatTitle);
        }

        private static bool IsDefaultTitleRule(AchievementRule rule)
        {
            return rule.Id == DefaultTitleId;
        }

        private static int Measure(AchievementMetric metric, PlayerStatistics statistics)
        {
            int value;

            switch (metric)
            {
                case AchievementMetric.GamesPlayed:
                    value = statistics.GamesPlayed;
                    break;
                case AchievementMetric.Wins:
                    value = statistics.Wins;
                    break;
                case AchievementMetric.BestScore:
                    value = statistics.BestScore;
                    break;
                case AchievementMetric.WinRatePercentage:
                    value = statistics.WinRatePercentage;
                    break;
                case AchievementMetric.Level:
                    value = statistics.Level;
                    break;
                case AchievementMetric.Experience:
                    value = statistics.Experience;
                    break;
                default:
                    value = NoMeasuredValue;
                    break;
            }

            return value;
        }
    }
}
