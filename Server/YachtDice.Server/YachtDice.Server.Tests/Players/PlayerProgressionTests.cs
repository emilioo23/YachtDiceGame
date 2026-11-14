using System;
using Xunit;
using YachtDice.Server.Logic.Players;

namespace YachtDice.Server.Tests.Players
{
    /// <summary>
    /// Rules of the progression of a player: the winner earns 100 points and the other players 30, every player
    /// adds one point for each point of the game, and every level needs more experience than the previous one.
    /// </summary>
    public class PlayerProgressionTests
    {
        private const int WinnerScore = 250;
        private const int WinnerExperience = 350;
        private const int ParticipantScore = 180;
        private const int ParticipantExperience = 210;
        private const int NoScore = 0;
        private const int WinnerBaseExperience = 100;
        private const int ParticipantBaseExperience = 30;
        private const int HighestScore = 1575;
        private const int HighestScoreExperience = 1675;
        private const int NegativeScore = -1;
        private const int NegativeExperience = -50;
        private const int ExperienceBetweenLevels = 750;
        private const int LevelOfExperienceBetweenLevels = 3;
        private const int SecondLevel = 2;
        private const int SecondLevelExperience = 200;
        private const int LastAvatarLevel = 20;
        private const int LastAvatarLevelExperience = 20900;
        private const int HighestLevelOfAnInteger = 6553;
        private const long NoExperienceRequired = 0;

        [Fact]
        public void CalculateExperience_Winner_AddsTheWinnerBaseToTheScore()
        {
            int expectedExperience = WinnerExperience;

            int experience = PlayerProgression.CalculateExperience(WinnerScore, true);

            Assert.Equal(expectedExperience, experience);
        }

        [Fact]
        public void CalculateExperience_Participant_AddsTheParticipantBaseToTheScore()
        {
            int expectedExperience = ParticipantExperience;

            int experience = PlayerProgression.CalculateExperience(ParticipantScore, false);

            Assert.Equal(expectedExperience, experience);
        }

        [Fact]
        public void CalculateExperience_WinnerWithoutPoints_ReturnsTheWinnerBase()
        {
            int expectedExperience = WinnerBaseExperience;

            int experience = PlayerProgression.CalculateExperience(NoScore, true);

            Assert.Equal(expectedExperience, experience);
        }

        [Fact]
        public void CalculateExperience_ParticipantWithoutPoints_ReturnsTheParticipantBase()
        {
            int expectedExperience = ParticipantBaseExperience;

            int experience = PlayerProgression.CalculateExperience(NoScore, false);

            Assert.Equal(expectedExperience, experience);
        }

        [Fact]
        public void CalculateExperience_HighestPossibleScore_AddsEveryPoint()
        {
            int expectedExperience = HighestScoreExperience;

            int experience = PlayerProgression.CalculateExperience(HighestScore, true);

            Assert.Equal(expectedExperience, experience);
        }

        [Fact]
        public void CalculateExperience_NegativeScore_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => PlayerProgression.CalculateExperience(NegativeScore, false));
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(199, 1)]
        [InlineData(200, 2)]
        [InlineData(499, 2)]
        [InlineData(500, 3)]
        [InlineData(900, 4)]
        public void GetLevel_ExperienceAroundTheLevels_ReturnsTheLevelThatWasReached(int experience, int expectedLevel)
        {
            int level = PlayerProgression.GetLevel(experience);

            Assert.Equal(expectedLevel, level);
        }

        [Fact]
        public void GetLevel_ExperienceBetweenTwoLevels_ReturnsTheLowerLevel()
        {
            int expectedLevel = LevelOfExperienceBetweenLevels;

            int level = PlayerProgression.GetLevel(ExperienceBetweenLevels);

            Assert.Equal(expectedLevel, level);
        }

        [Fact]
        public void GetLevel_NegativeExperience_ReturnsTheFirstLevel()
        {
            int expectedLevel = PlayerProgression.FirstLevel;

            int level = PlayerProgression.GetLevel(NegativeExperience);

            Assert.Equal(expectedLevel, level);
        }

        [Fact]
        public void GetLevel_HighestInteger_ReturnsALevelWithoutOverflowing()
        {
            int expectedLevel = HighestLevelOfAnInteger;

            int level = PlayerProgression.GetLevel(int.MaxValue);

            Assert.Equal(expectedLevel, level);
        }

        [Fact]
        public void GetRequiredExperience_FirstLevel_ReturnsZero()
        {
            long expectedExperience = NoExperienceRequired;

            long requiredExperience = PlayerProgression.GetRequiredExperience(PlayerProgression.FirstLevel);

            Assert.Equal(expectedExperience, requiredExperience);
        }

        [Fact]
        public void GetRequiredExperience_SecondLevel_ReturnsTwoHundred()
        {
            long expectedExperience = SecondLevelExperience;

            long requiredExperience = PlayerProgression.GetRequiredExperience(SecondLevel);

            Assert.Equal(expectedExperience, requiredExperience);
        }

        [Fact]
        public void GetRequiredExperience_LastAvatarLevel_ReturnsTheExperienceOfThatLevel()
        {
            long expectedExperience = LastAvatarLevelExperience;

            long requiredExperience = PlayerProgression.GetRequiredExperience(LastAvatarLevel);

            Assert.Equal(expectedExperience, requiredExperience);
        }
    }
}
