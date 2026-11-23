using System;
using Xunit;
using YachtDice.Server.Logic.Players;

namespace YachtDice.Server.Tests.Players
{
    /// <summary>
    /// Levels of the players and the experience that they gain.
    /// </summary>
    public class LevelTableTests
    {
        private const int FirstLevel = 1;
        private const int SecondLevel = 2;
        private const int ThirdLevel = 3;
        private const int NoExperience = 0;
        private const int SecondLevelExperience = 200;
        private const int ThirdLevelExperience = 500;
        private const int BelowSecondLevelExperience = 199;
        private const int FarAboveLastLevelExperience = 9000;
        private const int NegativeExperience = -1;
        private const int NoLevel = 0;
        private const int AboveLastLevel = 9;
        private const int GameTotal = 180;
        private const int NoPoints = 0;
        private const int NegativeTotal = -1;
        private const int WinnerExperience = 280;
        private const int LoserExperience = 210;
        private static readonly int[] Thresholds = { NoExperience, SecondLevelExperience, ThirdLevelExperience };

        [Theory]
        [InlineData(NoExperience, FirstLevel)]
        [InlineData(BelowSecondLevelExperience, FirstLevel)]
        [InlineData(SecondLevelExperience, SecondLevel)]
        [InlineData(ThirdLevelExperience, ThirdLevel)]
        public void GetLevel_Experience_ReturnsTheHighestLevelReached(int experience, int levelExpected)
        {
            var levelTable = new LevelTable(Thresholds);

            int level = levelTable.GetLevel(experience);

            Assert.Equal(levelExpected, level);
        }

        [Fact]
        public void GetLevel_MoreExperienceThanTheLastLevel_ReturnsTheLastLevel()
        {
            var levelTable = new LevelTable(Thresholds);

            int level = levelTable.GetLevel(FarAboveLastLevelExperience);

            Assert.Equal(levelTable.MaxLevel, level);
        }

        [Fact]
        public void GetLevel_NegativeExperience_ThrowsArgumentOutOfRangeException()
        {
            var levelTable = new LevelTable(Thresholds);

            Assert.Throws<ArgumentOutOfRangeException>(() => levelTable.GetLevel(NegativeExperience));
        }

        [Theory]
        [InlineData(FirstLevel, NoExperience)]
        [InlineData(SecondLevel, SecondLevelExperience)]
        [InlineData(AboveLastLevel, ThirdLevelExperience)]
        public void GetLevelStart_Level_ReturnsTheExperienceThatTheLevelNeeds(int level, int experienceExpected)
        {
            var levelTable = new LevelTable(Thresholds);

            int levelStart = levelTable.GetLevelStart(level);

            Assert.Equal(experienceExpected, levelStart);
        }

        [Theory]
        [InlineData(FirstLevel, SecondLevelExperience)]
        [InlineData(SecondLevel, ThirdLevelExperience)]
        [InlineData(ThirdLevel, ThirdLevelExperience)]
        public void GetNextLevelStart_Level_ReturnsTheExperienceOfTheNextLevel(int level, int experienceExpected)
        {
            var levelTable = new LevelTable(Thresholds);

            int nextLevelStart = levelTable.GetNextLevelStart(level);

            Assert.Equal(experienceExpected, nextLevelStart);
        }

        [Fact]
        public void GetLevelStart_NoLevel_ThrowsArgumentOutOfRangeException()
        {
            var levelTable = new LevelTable(Thresholds);

            Assert.Throws<ArgumentOutOfRangeException>(() => levelTable.GetLevelStart(NoLevel));
        }

        [Fact]
        public void Constructor_NullThresholds_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new LevelTable(null));
        }

        [Fact]
        public void Constructor_EmptyThresholds_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new LevelTable(Array.Empty<int>()));
        }

        [Fact]
        public void Constructor_FirstLevelNeedsExperience_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => new LevelTable(new[] { SecondLevelExperience, ThirdLevelExperience }));
        }

        [Fact]
        public void Constructor_UnorderedThresholds_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => new LevelTable(new[] { NoExperience, ThirdLevelExperience, SecondLevelExperience }));
        }

        [Fact]
        public void CalculateGain_Winner_AddsTheWinnerBaseToTheTotal()
        {
            int experienceGained = LevelTable.CalculateGain(GameTotal, true);

            Assert.Equal(WinnerExperience, experienceGained);
        }

        [Fact]
        public void CalculateGain_Loser_AddsTheLoserBaseToTheTotal()
        {
            int experienceGained = LevelTable.CalculateGain(GameTotal, false);

            Assert.Equal(LoserExperience, experienceGained);
        }

        [Fact]
        public void CalculateGain_LoserWithoutPoints_GetsOnlyTheLoserBase()
        {
            int experienceGained = LevelTable.CalculateGain(NoPoints, false);

            Assert.Equal(LevelTable.LoserBaseExperience, experienceGained);
        }

        [Fact]
        public void CalculateGain_NegativeTotal_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => LevelTable.CalculateGain(NegativeTotal, true));
        }
    }
}
