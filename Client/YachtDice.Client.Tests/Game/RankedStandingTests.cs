using Xunit;
using YachtDice.Client.Logic.Game;

namespace YachtDice.Client.Tests.Game
{
    /// <summary>
    /// Place of a player in the final scoreboard.
    /// </summary>
    public class RankedStandingTests
    {
        private const string PlayerId = "a";
        private const string PlayerName = "Ana";
        private const int Total = 150;
        private const int SecondRank = 2;
        private const int ExperienceGained = 250;
        private const int UpperSubtotal = 70;
        private const int UpperBonus = 35;
        private const int YachtBonus = 100;
        private const int FullTotal = 380;
        private const int LowerTotal = 175;

        private readonly StandingEntry _entry = new StandingEntry(PlayerId, PlayerName, Total);

        [Fact]
        public void IsWinner_FirstRank_ReturnsTrue()
        {
            var standing = new RankedStanding(StandingsBuilder.FirstRank, _entry);

            Assert.True(standing.IsWinner);
        }

        [Fact]
        public void IsWinner_SecondRank_ReturnsFalse()
        {
            var standing = new RankedStanding(SecondRank, _entry);

            Assert.False(standing.IsWinner);
        }

        [Fact]
        public void Constructor_Entry_KeepsTheTotalOfThePlayer()
        {
            var standing = new RankedStanding(SecondRank, _entry);

            Assert.Equal(Total, standing.Total);
        }

        [Fact]
        public void Constructor_EntryWithBreakdown_KeepsTheUpperSectionAndTheBonuses()
        {
            var standingEntry = new StandingEntry(PlayerId, PlayerName, FullTotal)
            {
                UpperSubtotal = UpperSubtotal,
                UpperBonus = UpperBonus,
                YachtBonus = YachtBonus
            };
            var standingExpected = new { UpperSubtotal, UpperBonus, YachtBonus };

            var standing = new RankedStanding(SecondRank, standingEntry);

            Assert.Equivalent(standingExpected, standing);
        }

        [Fact]
        public void LowerTotal_EntryWithBreakdown_LeavesOutTheUpperSectionAndTheBonuses()
        {
            var standingEntry = new StandingEntry(PlayerId, PlayerName, FullTotal)
            {
                UpperSubtotal = UpperSubtotal,
                UpperBonus = UpperBonus,
                YachtBonus = YachtBonus
            };

            var standing = new RankedStanding(SecondRank, standingEntry);

            Assert.Equal(LowerTotal, standing.LowerTotal);
        }

        [Fact]
        public void LowerTotal_EntryWithoutBreakdown_IsTheWholeTotal()
        {
            var standing = new RankedStanding(SecondRank, _entry);

            Assert.Equal(Total, standing.LowerTotal);
        }

        [Fact]
        public void Constructor_EntryWithExperience_KeepsTheExperienceGained()
        {
            var standingEntry = new StandingEntry(PlayerId, PlayerName, Total) { ExperienceGained = ExperienceGained };

            var standing = new RankedStanding(SecondRank, standingEntry);

            Assert.Equal(ExperienceGained, standing.ExperienceGained);
        }
    }
}
