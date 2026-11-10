using System.Collections.Generic;
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
        private const string AnaId = "a";
        private const string BetoId = "b";
        private const string CesarId = "c";
        private const string AnaName = "Ana";
        private const string BetoName = "Beto";
        private const string CesarName = "Cesar";
        private const int LowTotal = 120;
        private const int MiddleTotal = 150;
        private const int HighTotal = 200;
        private const int TiedTotal = 90;

        [Fact]
        public void Build_UnsortedPlayers_ReturnsThemFromTheHighestTotal()
        {
            StandingEntry[] entries =
            {
                new StandingEntry(AnaId, AnaName, LowTotal),
                new StandingEntry(BetoId, BetoName, HighTotal),
                new StandingEntry(CesarId, CesarName, MiddleTotal)
            };
            var expected = new { PlayerIds = new[] { BetoId, CesarId, AnaId }, Ranks = new[] { 1, 2, 3 } };

            IReadOnlyList<RankedStanding> standings = StandingsBuilder.Build(entries);

            var actual = new
            {
                PlayerIds = standings.Select(standing => standing.PlayerId).ToArray(),
                Ranks = standings.Select(standing => standing.Rank).ToArray()
            };
            Assert.Equivalent(expected, actual);
        }

        [Fact]
        public void Build_PlayersWithTheSameTotal_ShareThePlaceAndSkipTheNext()
        {
            StandingEntry[] entries =
            {
                new StandingEntry(AnaId, AnaName, HighTotal),
                new StandingEntry(BetoId, BetoName, HighTotal),
                new StandingEntry(CesarId, CesarName, MiddleTotal)
            };
            var expected = new { Ranks = new[] { 1, 1, 3 }, Winners = new[] { true, true, false } };

            IReadOnlyList<RankedStanding> standings = StandingsBuilder.Build(entries);

            var actual = new
            {
                Ranks = standings.Select(standing => standing.Rank).ToArray(),
                Winners = standings.Select(standing => standing.IsWinner).ToArray()
            };
            Assert.Equivalent(expected, actual);
        }

        [Fact]
        public void Build_PlayersWithTheSameTotal_KeepTheirOriginalOrder()
        {
            StandingEntry[] entries =
            {
                new StandingEntry(AnaId, AnaName, TiedTotal),
                new StandingEntry(BetoId, BetoName, TiedTotal)
            };

            IReadOnlyList<RankedStanding> standings = StandingsBuilder.Build(entries);

            Assert.Equal(new[] { AnaId, BetoId }, standings.Select(standing => standing.PlayerId).ToArray());
        }

        [Fact]
        public void Build_NoPlayers_ReturnsAnEmptyList()
        {
            IReadOnlyList<RankedStanding> standings = StandingsBuilder.Build(new StandingEntry[0]);

            Assert.Empty(standings);
        }
    }
}
