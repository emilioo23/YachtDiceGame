using System.Collections.Generic;
using Xunit;
using YachtDice.Server.Logic.Players;

namespace YachtDice.Server.Tests.Players
{
    /// <summary>
    /// Conditions that unlock the achievements and the titles of the players.
    /// </summary>
    public class AchievementRulesTests
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

        [Fact]
        public void GetAll_ReturnsEveryAchievement()
        {
            Assert.Equal(AchievementCount, AchievementRules.GetAll().Count);
        }

        [Fact]
        public void GetUnlockedIds_PlayerWithoutGames_ReturnsNoAchievements()
        {
            List<int> unlockedIds = AchievementRules.GetUnlockedIds(new PlayerStatistics());

            Assert.Empty(unlockedIds);
        }

        [Fact]
        public void GetUnlockedIds_OneGameAndOneWin_UnlocksTheFirstGameAndTheFirstWin()
        {
            var statistics = new PlayerStatistics { GamesPlayed = OneGame, Wins = OneWin };
            var expectedIds = new List<int> { FirstGameAchievementId, FirstWinAchievementId };

            List<int> unlockedIds = AchievementRules.GetUnlockedIds(statistics);

            Assert.Equal(expectedIds, unlockedIds);
        }

        [Fact]
        public void GetUnlockedIds_BestScoreOfThreeHundred_UnlocksTheFourScoreAchievements()
        {
            var statistics = new PlayerStatistics { GamesPlayed = OneGame, BestScore = HighestScore };
            var expectedIds = new List<int>
            {
                FirstGameAchievementId,
                WellStartedAchievementId,
                FastShooterAchievementId,
                AlmostPerfectAchievementId,
                YachtMasterAchievementId
            };

            List<int> unlockedIds = AchievementRules.GetUnlockedIds(statistics);

            Assert.Equal(expectedIds, unlockedIds);
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
            var expectedIds = new List<int>
            {
                FirstGameAchievementId,
                FirstWinAchievementId,
                ThirdWinAchievementId,
                FifthWinAchievementId
            };

            List<int> unlockedIds = AchievementRules.GetUnlockedIds(statistics);

            Assert.Equal(expectedIds, unlockedIds);
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
            var expectedIds = new List<int>
            {
                FirstGameAchievementId,
                FirstWinAchievementId,
                ThirdWinAchievementId,
                FifthWinAchievementId,
                TenthGameAchievementId,
                DominantAchievementId
            };

            List<int> unlockedIds = AchievementRules.GetUnlockedIds(statistics);

            Assert.Equal(expectedIds, unlockedIds);
        }

        [Fact]
        public void GetUnlockedIds_TopLevel_UnlocksTheFourLevelAchievements()
        {
            var statistics = new PlayerStatistics { Level = TopLevel };
            var expectedIds = new List<int>
            {
                ApprenticeAchievementId,
                ExperiencedAchievementId,
                EliteAchievementId,
                GrandMasterAchievementId
            };

            List<int> unlockedIds = AchievementRules.GetUnlockedIds(statistics);

            Assert.Equal(expectedIds, unlockedIds);
        }

        [Fact]
        public void GetUnlockedIds_FiveThousandExperience_UnlocksTheTwoExperienceAchievements()
        {
            var statistics = new PlayerStatistics { Experience = MountainExperience };
            var expectedIds = new List<int> { CollectorAchievementId, MountainAchievementId };

            List<int> unlockedIds = AchievementRules.GetUnlockedIds(statistics);

            Assert.Equal(expectedIds, unlockedIds);
        }

        [Fact]
        public void IsTitleAvailable_DefaultTitle_ReturnsTrueWithoutAchievements()
        {
            bool isAvailable = AchievementRules.IsTitleAvailable(AchievementRules.DefaultTitleId, new List<int>());

            Assert.True(isAvailable);
        }

        [Fact]
        public void IsTitleAvailable_UnlockedTitle_ReturnsTrue()
        {
            var unlockedIds = new List<int> { FirstWinAchievementId };

            bool isAvailable = AchievementRules.IsTitleAvailable(FirstWinAchievementId, unlockedIds);

            Assert.True(isAvailable);
        }

        [Fact]
        public void IsTitleAvailable_LockedTitle_ReturnsFalse()
        {
            bool isAvailable = AchievementRules.IsTitleAvailable(FirstWinAchievementId, new List<int>());

            Assert.False(isAvailable);
        }

        [Fact]
        public void IsTitleAvailable_UnknownTitleEvenIfListed_ReturnsFalse()
        {
            var unlockedIds = new List<int> { UnknownTitleId };

            bool isAvailable = AchievementRules.IsTitleAvailable(UnknownTitleId, unlockedIds);

            Assert.False(isAvailable);
        }

        [Fact]
        public void ExistsTitle_AchievementWithoutTitle_ReturnsFalse()
        {
            Assert.False(AchievementRules.ExistsTitle(CollectorAchievementId));
        }

        [Fact]
        public void ExistsTitle_UnknownId_ReturnsFalse()
        {
            Assert.False(AchievementRules.ExistsTitle(UnknownTitleId));
        }

        [Fact]
        public void ExistsTitle_DefaultTitle_ReturnsTrue()
        {
            Assert.True(AchievementRules.ExistsTitle(AchievementRules.DefaultTitleId));
        }

        [Fact]
        public void IsUnlocked_NoValueForTheMetric_ReturnsFalse()
        {
            var rule = new AchievementRule(FirstGameAchievementId, AchievementMetric.GamesPlayed, OneGame, true);

            Assert.False(AchievementRules.IsUnlocked(rule, new PlayerStatistics { GamesPlayed = NoValue }));
        }
    }
}
