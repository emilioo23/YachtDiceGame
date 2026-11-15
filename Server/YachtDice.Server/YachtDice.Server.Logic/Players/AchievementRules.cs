using System;
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

        private const int FirstGameId = 1;
        private const int FirstWinId = 2;
        private const int ThirdWinId = 3;
        private const int FifthWinId = 4;
        private const int TenthWinId = 5;
        private const int TwentyFifthWinId = 6;
        private const int FiftiethWinId = 7;
        private const int TenthGameId = 8;
        private const int TwentyFifthGameId = 9;
        private const int FiftiethGameId = 10;
        private const int HundredthGameId = 11;
        private const int WellStartedId = 12;
        private const int FastShooterId = 13;
        private const int AlmostPerfectId = 14;
        private const int YachtMasterId = 15;
        private const int DominantId = 16;
        private const int UntouchableId = 17;
        private const int ApprenticeId = 18;
        private const int ExperiencedId = 19;
        private const int EliteId = 20;
        private const int GrandMasterId = 21;
        private const int CollectorId = 22;
        private const int MountainId = 23;
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
            TitledRule(FirstGameId, AchievementMetric.GamesPlayed, FirstGameCount),
            TitledRule(FirstWinId, AchievementMetric.Wins, FirstWinCount),
            TitledRule(ThirdWinId, AchievementMetric.Wins, ThirdWinCount),
            TitledRule(FifthWinId, AchievementMetric.Wins, FifthWinCount),
            TitledRule(TenthWinId, AchievementMetric.Wins, TenthWinCount),
            TitledRule(TwentyFifthWinId, AchievementMetric.Wins, TwentyFifthWinCount),
            TitledRule(FiftiethWinId, AchievementMetric.Wins, FiftiethWinCount),
            TitledRule(TenthGameId, AchievementMetric.GamesPlayed, TenthGameCount),
            TitledRule(TwentyFifthGameId, AchievementMetric.GamesPlayed, TwentyFifthGameCount),
            TitledRule(FiftiethGameId, AchievementMetric.GamesPlayed, FiftiethGameCount),
            TitledRule(HundredthGameId, AchievementMetric.GamesPlayed, HundredthGameCount),
            TitledRule(WellStartedId, AchievementMetric.BestScore, WellStartedScore),
            TitledRule(FastShooterId, AchievementMetric.BestScore, FastShooterScore),
            TitledRule(AlmostPerfectId, AchievementMetric.BestScore, AlmostPerfectScore),
            TitledRule(YachtMasterId, AchievementMetric.BestScore, YachtMasterScore),
            new AchievementRule(
                DominantId,
                new AchievementGoal(AchievementMetric.WinRatePercentage, DominantWinRate),
                WithTitle)
            {
                MinimumGames = DominantMinimumGames
            },
            new AchievementRule(
                UntouchableId,
                new AchievementGoal(AchievementMetric.WinRatePercentage, UntouchableWinRate),
                WithTitle)
            {
                MinimumGames = UntouchableMinimumGames
            },
            TitledRule(ApprenticeId, AchievementMetric.Level, ApprenticeLevel),
            TitledRule(ExperiencedId, AchievementMetric.Level, ExperiencedLevel),
            TitledRule(EliteId, AchievementMetric.Level, EliteLevel),
            TitledRule(GrandMasterId, AchievementMetric.Level, GrandMasterLevel),
            new AchievementRule(
                CollectorId,
                new AchievementGoal(AchievementMetric.Experience, CollectorExperience),
                WithoutTitle),
            TitledRule(MountainId, AchievementMetric.Experience, MountainExperience)
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

            return hasPlayedEnough && Measure(rule.Goal.Metric, statistics) >= rule.Goal.RequiredValue;
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
            Func<AchievementRule, bool> hasThatTitle = rule => rule.Id == titleId && rule.HasTitle;

            return titleId == DefaultTitleId || Rules.Any(hasThatTitle);
        }

        private static AchievementRule TitledRule(int id, AchievementMetric metric, int requiredValue)
        {
            return new AchievementRule(id, new AchievementGoal(metric, requiredValue), WithTitle);
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
