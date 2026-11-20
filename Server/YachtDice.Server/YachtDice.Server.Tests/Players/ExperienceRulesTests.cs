using System;
using Xunit;
using YachtDice.Server.Logic.Players;

namespace YachtDice.Server.Tests.Players
{
    /// <summary>
    /// Experience that a player earns for completing a game, according to the rules of the team.
    /// </summary>
    public class ExperienceRulesTests
    {
        private const int GameTotal = 180;
        private const int NoPoints = 0;
        private const int NegativeTotal = -1;
        private const int WinnerExperience = 280;
        private const int LoserExperience = 210;

        [Fact]
        public void CalculateGain_Winner_AddsTheWinnerBaseToTheTotal()
        {
            int experienceGained = ExperienceRules.CalculateGain(GameTotal, true);

            Assert.Equal(WinnerExperience, experienceGained);
        }

        [Fact]
        public void CalculateGain_Loser_AddsTheLoserBaseToTheTotal()
        {
            int experienceGained = ExperienceRules.CalculateGain(GameTotal, false);

            Assert.Equal(LoserExperience, experienceGained);
        }

        [Fact]
        public void CalculateGain_LoserWithoutPoints_GetsOnlyTheLoserBase()
        {
            int experienceGained = ExperienceRules.CalculateGain(NoPoints, false);

            Assert.Equal(ExperienceRules.LoserBaseExperience, experienceGained);
        }

        [Fact]
        public void CalculateGain_NegativeTotal_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ExperienceRules.CalculateGain(NegativeTotal, true));
        }
    }
}
