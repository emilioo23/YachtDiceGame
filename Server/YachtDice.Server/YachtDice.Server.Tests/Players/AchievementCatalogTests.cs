using System;
using System.Collections.Generic;
using Xunit;
using YachtDice.Server.Logic.Players;
using YachtDice.Server.Tests.Configuration;

namespace YachtDice.Server.Tests.Players
{
    /// <summary>
    /// Conditions that unlock the achievements and the titles of the players.
    /// </summary>
    public class AchievementCatalogTests
    {
        private const int NoValue = 0;
        private const int FirstGameAchievementId = 1;
        private const int FirstWinAchievementId = 2;
        private const int ThirdWinAchievementId = 3;
        private const int FifthWinAchievementId = 4;
        private const int TenthGameAchievementId = 8;
        private const int WellStartedAchievementId = 12;
        private const int FastShooterAchievementId = 13;
        private const int AlmostPerfectAchievementId = 14;
        private const int YachtMasterAchievementId = 15;
        private const int DominantAchievementId = 16;
        private const int ApprenticeAchievementId = 18;
        private const int ExperiencedAchievementId = 19;
        private const int EliteAchievementId = 20;
        private const int GrandMasterAchievementId = 21;
        private const int CollectorAchievementId = 22;
        private const int MountainAchievementId = 23;
        private const int AchievementCount = 23;
        private const int UnknownTitleId = 99;
        private const int OneGame = 1;
        private const int FiveGames = 5;
        private const int TenGames = 10;
        private const int OneWin = 1;
        private const int FiveWins = 5;
        private const int SixWins = 6;
        private const int PerfectWinRate = 100;
        private const int DominantWinRate = 60;
        private const int HighestScore = 300;
        private const int TopLevel = 20;
        private const int MountainExperience = 5000;

        private readonly AchievementCatalog _catalog = GameRulesFile.Load().Achievements;

        [Fact]
        public void GetAll_Always_ReturnsEveryAchievement()
        {
            Assert.Equal(AchievementCount, _catalog.Rules.Count);
        }

        [Fact]
        public void GetUnlockedIds_PlayerWithoutGames_ReturnsNoAchievements()
        {
            List<int> unlockedIds = _catalog.GetUnlockedIds(new PlayerStatistics());

            Assert.Empty(unlockedIds);
        }

        [Fact]
        public void GetUnlockedIds_OneGameAndOneWin_UnlocksTheFirstGameAndTheFirstWin()
        {
            var statistics = new PlayerStatistics { GamesPlayed = OneGame, Wins = OneWin };
            var idsExpected = new List<int> { FirstGameAchievementId, FirstWinAchievementId };

            List<int> unlockedIds = _catalog.GetUnlockedIds(statistics);

            Assert.Equal(idsExpected, unlockedIds);
        }

        [Fact]
        public void GetUnlockedIds_BestScoreOfThreeHundred_UnlocksTheFourScoreAchievements()
        {
            var statistics = new PlayerStatistics { GamesPlayed = OneGame, BestScore = HighestScore };
            var idsExpected = new List<int>
            {
                FirstGameAchievementId,
                WellStartedAchievementId,
                FastShooterAchievementId,
                AlmostPerfectAchievementId,
                YachtMasterAchievementId
            };

            List<int> unlockedIds = _catalog.GetUnlockedIds(statistics);

            Assert.Equal(idsExpected, unlockedIds);
        }

        [Fact]
        public void GetUnlockedIds_PerfectWinRateWithFewGames_DoesNotUnlockTheWinRateAchievements()
        {
            var statistics = new PlayerStatistics
            {
                GamesPlayed = FiveGames,
                Wins = FiveWins,
                WinRatePercentage = PerfectWinRate
            };
            var idsExpected = new List<int>
            {
                FirstGameAchievementId,
                FirstWinAchievementId,
                ThirdWinAchievementId,
                FifthWinAchievementId
            };

            List<int> unlockedIds = _catalog.GetUnlockedIds(statistics);

            Assert.Equal(idsExpected, unlockedIds);
        }

        [Fact]
        public void GetUnlockedIds_SixtyPercentOfTenGames_UnlocksTheDominantAchievement()
        {
            var statistics = new PlayerStatistics
            {
                GamesPlayed = TenGames,
                Wins = SixWins,
                WinRatePercentage = DominantWinRate
            };
            var idsExpected = new List<int>
            {
                FirstGameAchievementId,
                FirstWinAchievementId,
                ThirdWinAchievementId,
                FifthWinAchievementId,
                TenthGameAchievementId,
                DominantAchievementId
            };

            List<int> unlockedIds = _catalog.GetUnlockedIds(statistics);

            Assert.Equal(idsExpected, unlockedIds);
        }

        [Fact]
        public void GetUnlockedIds_TopLevel_UnlocksTheFourLevelAchievements()
        {
            var statistics = new PlayerStatistics { Level = TopLevel };
            var idsExpected = new List<int>
            {
                ApprenticeAchievementId,
                ExperiencedAchievementId,
                EliteAchievementId,
                GrandMasterAchievementId
            };

            List<int> unlockedIds = _catalog.GetUnlockedIds(statistics);

            Assert.Equal(idsExpected, unlockedIds);
        }

        [Fact]
        public void GetUnlockedIds_FiveThousandExperience_UnlocksTheTwoExperienceAchievements()
        {
            var statistics = new PlayerStatistics { Experience = MountainExperience };
            var idsExpected = new List<int> { CollectorAchievementId, MountainAchievementId };

            List<int> unlockedIds = _catalog.GetUnlockedIds(statistics);

            Assert.Equal(idsExpected, unlockedIds);
        }

        [Fact]
        public void IsTitleAvailable_DefaultTitle_ReturnsTrueWithoutAchievements()
        {
            bool isAvailable = _catalog.IsTitleAvailable(AchievementCatalog.DefaultTitleId, new List<int>());

            Assert.True(isAvailable);
        }

        [Fact]
        public void IsTitleAvailable_UnlockedTitle_ReturnsTrue()
        {
            var unlockedIds = new List<int> { FirstWinAchievementId };

            bool isAvailable = _catalog.IsTitleAvailable(FirstWinAchievementId, unlockedIds);

            Assert.True(isAvailable);
        }

        [Fact]
        public void IsTitleAvailable_LockedTitle_ReturnsFalse()
        {
            bool isAvailable = _catalog.IsTitleAvailable(FirstWinAchievementId, new List<int>());

            Assert.False(isAvailable);
        }

        [Fact]
        public void IsTitleAvailable_UnknownTitleEvenIfListed_ReturnsFalse()
        {
            var unlockedIds = new List<int> { UnknownTitleId };

            bool isAvailable = _catalog.IsTitleAvailable(UnknownTitleId, unlockedIds);

            Assert.False(isAvailable);
        }

        [Fact]
        public void ExistsTitle_AchievementWithoutTitle_ReturnsFalse()
        {
            Assert.False(_catalog.ExistsTitle(CollectorAchievementId));
        }

        [Fact]
        public void ExistsTitle_UnknownId_ReturnsFalse()
        {
            Assert.False(_catalog.ExistsTitle(UnknownTitleId));
        }

        [Fact]
        public void ExistsTitle_DefaultTitle_ReturnsTrue()
        {
            Assert.True(_catalog.ExistsTitle(AchievementCatalog.DefaultTitleId));
        }

        [Fact]
        public void IsUnlocked_NoValueForTheMetric_ReturnsFalse()
        {
            var rule = new AchievementRule(FirstGameAchievementId, AchievementMetric.GamesPlayed, OneGame);

            Assert.False(AchievementCatalog.IsUnlocked(rule, new PlayerStatistics { GamesPlayed = NoValue }));
        }

        [Fact]
        public void Constructor_RepeatedIdentifier_ThrowsArgumentException()
        {
            var rules = new[]
            {
                new AchievementRule(FirstGameAchievementId, AchievementMetric.GamesPlayed, OneGame),
                new AchievementRule(FirstGameAchievementId, AchievementMetric.Wins, OneWin)
            };

            Assert.Throws<ArgumentException>(() => new AchievementCatalog(rules));
        }

        [Fact]
        public void Constructor_IdentifierOfTheDefaultTitle_ThrowsArgumentException()
        {
            var rules = new[]
            {
                new AchievementRule(AchievementCatalog.DefaultTitleId, AchievementMetric.GamesPlayed, OneGame)
            };

            Assert.Throws<ArgumentException>(() => new AchievementCatalog(rules));
        }

        [Fact]
        public void Constructor_NullRules_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AchievementCatalog(null));
        }

        [Fact]
        public void Constructor_NewRule_UnlocksATitle()
        {
            var rule = new AchievementRule(FirstWinAchievementId, AchievementMetric.Wins, OneGame);

            Assert.True(rule.HasTitle);
        }
    }
}
