using System.Collections.Generic;
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
        private const int FirstPlace = StandingsBuilder.FirstRank;
        private const int SecondPlace = 2;
        private const int ThirdPlace = 3;

        [Fact]
        public void Build_UnsortedPlayers_ReturnsThemFromTheHighestTotal()
        {
            StandingEntry[] entries =
            {
                new StandingEntry(AnaId, AnaName, LowTotal),
                new StandingEntry(BetoId, BetoName, HighTotal),
                new StandingEntry(CesarId, CesarName, MiddleTotal)
            };
            RankedStanding[] expected =
            {
                new RankedStanding(FirstPlace, new StandingEntry(BetoId, BetoName, HighTotal)),
                new RankedStanding(SecondPlace, new StandingEntry(CesarId, CesarName, MiddleTotal)),
                new RankedStanding(ThirdPlace, new StandingEntry(AnaId, AnaName, LowTotal))
            };

            IReadOnlyList<RankedStanding> standings = StandingsBuilder.Build(entries);

            Assert.Equivalent(expected, standings, strict: true);
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
            RankedStanding[] expected =
            {
                new RankedStanding(FirstPlace, new StandingEntry(AnaId, AnaName, HighTotal)),
                new RankedStanding(FirstPlace, new StandingEntry(BetoId, BetoName, HighTotal)),
                new RankedStanding(ThirdPlace, new StandingEntry(CesarId, CesarName, MiddleTotal))
            };

            IReadOnlyList<RankedStanding> standings = StandingsBuilder.Build(entries);

            Assert.Equivalent(expected, standings, strict: true);
        }

        [Fact]
        public void Build_PlayersWithTheSameTotal_KeepTheirOriginalOrder()
        {
            StandingEntry[] entries =
            {
                new StandingEntry(AnaId, AnaName, TiedTotal),
                new StandingEntry(BetoId, BetoName, TiedTotal)
            };
            RankedStanding[] expected =
            {
                new RankedStanding(FirstPlace, new StandingEntry(AnaId, AnaName, TiedTotal)),
                new RankedStanding(FirstPlace, new StandingEntry(BetoId, BetoName, TiedTotal))
            };

            IReadOnlyList<RankedStanding> standings = StandingsBuilder.Build(entries);

            Assert.Equivalent(expected, standings, strict: true);
        }

        [Fact]
        public void Build_NoPlayers_ReturnsAnEmptyList()
        {
            IReadOnlyList<RankedStanding> standings = StandingsBuilder.Build(new StandingEntry[0]);

            Assert.Empty(standings);
        }
    }
}
