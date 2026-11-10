using System.Linq;
using Xunit;
using YachtDice.Client.Logic.Game;

namespace YachtDice.Client.Tests.Game
{
    /// <summary>
    /// Final scoreboard of a game: order and places.
    /// </summary>
    public class StandingsBuilderTests
    {
        [Fact]
        public void Build_UnsortedPlayers_ReturnsThemFromTheHighestTotal()
        {
            var entries = new[]
            {
                new StandingEntry("a", "Ana", 120),
                new StandingEntry("b", "Beto", 200),
                new StandingEntry("c", "Cesar", 150)
            };

            var standings = StandingsBuilder.Build(entries);

            Assert.Equal(new[] { "b", "c", "a" }, standings.Select(standing => standing.PlayerId).ToArray());
            Assert.Equal(new[] { 1, 2, 3 }, standings.Select(standing => standing.Rank).ToArray());
        }

        [Fact]
        public void Build_PlayersWithTheSameTotal_ShareThePlaceAndSkipTheNext()
        {
            var entries = new[]
            {
                new StandingEntry("a", "Ana", 200),
                new StandingEntry("b", "Beto", 200),
                new StandingEntry("c", "Cesar", 150)
            };

            var standings = StandingsBuilder.Build(entries);

            Assert.Equal(new[] { 1, 1, 3 }, standings.Select(standing => standing.Rank).ToArray());
            Assert.Equal(new[] { true, true, false }, standings.Select(standing => standing.IsWinner).ToArray());
        }

        [Fact]
        public void Build_PlayersWithTheSameTotal_KeepTheirOriginalOrder()
        {
            var entries = new[] { new StandingEntry("a", "Ana", 90), new StandingEntry("b", "Beto", 90) };

            var standings = StandingsBuilder.Build(entries);

            Assert.Equal(new[] { "a", "b" }, standings.Select(standing => standing.PlayerId).ToArray());
        }

        [Fact]
        public void Build_NoPlayers_ReturnsAnEmptyList()
        {
            var standings = StandingsBuilder.Build(new StandingEntry[0]);

            Assert.Empty(standings);
        }
    }
}
