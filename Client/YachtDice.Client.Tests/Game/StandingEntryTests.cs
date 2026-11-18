using Xunit;
using YachtDice.Client.Logic.Game;

namespace YachtDice.Client.Tests.Game
{
    /// <summary>
    /// Place and points of a player in the final scoreboard.
    /// </summary>
    public class StandingEntryTests
    {
        private const string PlayerId = "a";
        private const string PlayerName = "Ana";
        private const int Total = 150;
        private const int SecondRank = 2;
        private const int UpperSubtotal = 70;
        private const int UpperBonus = 35;
        private const int YachtBonus = 100;
        private const int FullTotal = 380;
        private const int LowerTotal = 175;

        [Fact]
        public void IsWinner_FirstRank_ReturnsTrue()
        {
            var entry = new StandingEntry(PlayerId, PlayerName, Total) { Rank = StandingsBuilder.FirstRank };

            Assert.True(entry.IsWinner);
        }

        [Fact]
        public void IsWinner_SecondRank_ReturnsFalse()
        {
            var entry = new StandingEntry(PlayerId, PlayerName, Total) { Rank = SecondRank };

            Assert.False(entry.IsWinner);
        }

        [Fact]
        public void LowerTotal_EntryWithBreakdown_LeavesOutTheUpperSectionAndTheBonuses()
        {
            var entry = new StandingEntry(PlayerId, PlayerName, FullTotal)
            {
                UpperSubtotal = UpperSubtotal,
                UpperBonus = UpperBonus,
                YachtBonus = YachtBonus
            };

            Assert.Equal(LowerTotal, entry.LowerTotal);
        }

        [Fact]
        public void LowerTotal_EntryWithoutBreakdown_IsTheWholeTotal()
        {
            var entry = new StandingEntry(PlayerId, PlayerName, Total);

            Assert.Equal(Total, entry.LowerTotal);
        }
    }
}
