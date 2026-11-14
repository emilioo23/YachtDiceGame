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
        public void Constructor_EntryWithExperience_KeepsTheExperienceGained()
        {
            var entry = new StandingEntry(PlayerId, PlayerName, Total) { ExperienceGained = ExperienceGained };

            var standing = new RankedStanding(SecondRank, entry);

            Assert.Equal(ExperienceGained, standing.ExperienceGained);
        }
    }
}
