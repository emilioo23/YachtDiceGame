using Xunit;
using YachtDice.Client.Logic.Players;

namespace YachtDice.Client.Tests.Players
{
    /// <summary>
    /// How full the experience bar of a level is and when a game made the player reach a new level.
    /// </summary>
    public class LevelProgressTests
    {
        private const int LevelStart = 200;
        private const int NextLevelStart = 500;
        private const int HalfwayExperience = 350;
        private const int BelowLevelExperience = 100;
        private const int AboveNextLevelExperience = 800;
        private const int LastLevelExperience = 20900;
        private const int SmallGain = 100;
        private const int BigGain = 200;
        private const double HalfFraction = 0.5;

        [Fact]
        public void GetFraction_HalfwayThroughTheLevel_ReturnsHalf()
        {
            double fraction = LevelProgress.GetFraction(HalfwayExperience, LevelStart, NextLevelStart);

            Assert.Equal(HalfFraction, fraction);
        }

        [Fact]
        public void GetFraction_ExperienceBelowTheLevel_ReturnsEmpty()
        {
            double fraction = LevelProgress.GetFraction(BelowLevelExperience, LevelStart, NextLevelStart);

            Assert.Equal(LevelProgress.EmptyFraction, fraction);
        }

        [Fact]
        public void GetFraction_ExperienceAboveTheNextLevel_ReturnsFull()
        {
            double fraction = LevelProgress.GetFraction(AboveNextLevelExperience, LevelStart, NextLevelStart);

            Assert.Equal(LevelProgress.FullFraction, fraction);
        }

        [Fact]
        public void GetFraction_LastLevel_ReturnsFull()
        {
            double fraction = LevelProgress.GetFraction(LastLevelExperience, LastLevelExperience, LastLevelExperience);

            Assert.Equal(LevelProgress.FullFraction, fraction);
        }

        [Fact]
        public void IsNewLevel_GainCrossesTheLevelStart_ReturnsTrue()
        {
            bool isNewLevel = LevelProgress.IsNewLevel(HalfwayExperience, BigGain, LevelStart);

            Assert.True(isNewLevel);
        }

        [Fact]
        public void IsNewLevel_GainInsideTheLevel_ReturnsFalse()
        {
            bool isNewLevel = LevelProgress.IsNewLevel(HalfwayExperience, SmallGain, LevelStart);

            Assert.False(isNewLevel);
        }
    }
}
