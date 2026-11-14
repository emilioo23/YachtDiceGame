using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Server.Logic.Players
{
    /// <summary>
    /// Defines the achievements of the game and decides which ones a player has unlocked.
    /// </summary>
    public static class AchievementRules
    {
        /// <summary>
        /// Identifier of the title that every player has from the start.
        /// </summary>
        public const int DefaultTitleId = 0;

        private const int FirstWinCount = 1;
        private const int ThirdWinCount = 3;
        private const int FifthWinCount = 5;
        private const int TenthWinCount = 10;
        private const int TwentyFifthWinCount = 25;
        private const int FiftiethWinCount = 50;
        private const int FirstGameCount = 1;
        private const int TenthGameCount = 10;
        private const int TwentyFifthGameCount = 25;
        private const int FiftiethGameCount = 50;
        private const int HundredthGameCount = 100;
        private const int WellStartedScore = 100;
        private const int FastShooterScore = 200;
        private const int AlmostPerfectScore = 250;
        private const int YachtMasterScore = 300;
        private const int DominantWinRate = 60;
        private const int UntouchableWinRate = 80;
        private const int DominantMinimumGames = 10;
        private const int UntouchableMinimumGames = 20;
        private const int ApprenticeLevel = 5;
        private const int ExperiencedLevel = 10;
        private const int EliteLevel = 15;
        private const int GrandMasterLevel = 20;
        private const int CollectorExperience = 1000;
        private const int MountainExperience = 5000;
        private const bool WithTitle = true;
        private const bool WithoutTitle = false;

        private static readonly IReadOnlyList<AchievementRule> Rules = new List<AchievementRule>
        {
            new AchievementRule(1, AchievementMetric.GamesPlayed, FirstGameCount, WithTitle),
            new AchievementRule(2, AchievementMetric.Wins, FirstWinCount, WithTitle),
            new AchievementRule(3, AchievementMetric.Wins, ThirdWinCount, WithTitle),
            new AchievementRule(4, AchievementMetric.Wins, FifthWinCount, WithTitle),
            new AchievementRule(5, AchievementMetric.Wins, TenthWinCount, WithTitle),
            new AchievementRule(6, AchievementMetric.Wins, TwentyFifthWinCount, WithTitle),
            new AchievementRule(7, AchievementMetric.Wins, FiftiethWinCount, WithTitle),
            new AchievementRule(8, AchievementMetric.GamesPlayed, TenthGameCount, WithTitle),
            new AchievementRule(9, AchievementMetric.GamesPlayed, TwentyFifthGameCount, WithTitle),
            new AchievementRule(10, AchievementMetric.GamesPlayed, FiftiethGameCount, WithTitle),
            new AchievementRule(11, AchievementMetric.GamesPlayed, HundredthGameCount, WithTitle),
            new AchievementRule(12, AchievementMetric.BestScore, WellStartedScore, WithTitle),
            new AchievementRule(13, AchievementMetric.BestScore, FastShooterScore, WithTitle),
            new AchievementRule(14, AchievementMetric.BestScore, AlmostPerfectScore, WithTitle),
            new AchievementRule(15, AchievementMetric.BestScore, YachtMasterScore, WithTitle),
            new AchievementRule(16, AchievementMetric.WinRatePercentage, DominantWinRate, WithTitle)
            {
                MinimumGames = DominantMinimumGames
            },
            new AchievementRule(17, AchievementMetric.WinRatePercentage, UntouchableWinRate, WithTitle)
            {
                MinimumGames = UntouchableMinimumGames
            },
            new AchievementRule(18, AchievementMetric.Level, ApprenticeLevel, WithTitle),
            new AchievementRule(19, AchievementMetric.Level, ExperiencedLevel, WithTitle),
            new AchievementRule(20, AchievementMetric.Level, EliteLevel, WithTitle),
            new AchievementRule(21, AchievementMetric.Level, GrandMasterLevel, WithTitle),
            new AchievementRule(22, AchievementMetric.Experience, CollectorExperience, WithoutTitle),
            new AchievementRule(23, AchievementMetric.Experience, MountainExperience, WithTitle)
        };

        /// <summary>
        /// Gets the rules of all the achievements, in the order in which they are shown.
        /// </summary>
        /// <returns>The rules of the achievements.</returns>
        public static IReadOnlyList<AchievementRule> GetAll()
        {
            return Rules;
        }

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
        public static List<int> GetUnlockedIds(PlayerStatistics statistics)
        {
            var unlockedIds = new List<int>();

            foreach (AchievementRule rule in Rules)
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
        public static bool IsTitleAvailable(int titleId, List<int> unlockedIds)
        {
            return titleId == DefaultTitleId || (ExistsTitle(titleId) && unlockedIds.Contains(titleId));
        }

        /// <summary>
        /// Decides if a title exists.
        /// </summary>
        /// <param name="titleId">Identifier of the title.</param>
        /// <returns>True if it is the default title or the title of an achievement.</returns>
        public static bool ExistsTitle(int titleId)
        {
            return titleId == DefaultTitleId || Rules.Any(rule => rule.Id == titleId && rule.HasTitle);
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
                default:
                    value = statistics.Experience;
                    break;
            }

            return value;
        }
    }
}
