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
            FinalPlace[] entries =
            {
                new FinalPlace(AnaId, AnaName, LowTotal),
                new FinalPlace(BetoId, BetoName, HighTotal),
                new FinalPlace(CesarId, CesarName, MiddleTotal)
            };
            FinalPlace[] standingsExpected =
            {
                new FinalPlace(BetoId, BetoName, HighTotal) { Rank = FirstPlace },
                new FinalPlace(CesarId, CesarName, MiddleTotal) { Rank = SecondPlace },
                new FinalPlace(AnaId, AnaName, LowTotal) { Rank = ThirdPlace }
            };

            IReadOnlyList<FinalPlace> standings = StandingsBuilder.Build(entries);

            Assert.Equivalent(standingsExpected, standings, strict: true);
        }

        [Fact]
        public void Build_PlayersWithTheSameTotal_ShareThePlaceAndSkipTheNext()
        {
            FinalPlace[] entries =
            {
                new FinalPlace(AnaId, AnaName, HighTotal),
                new FinalPlace(BetoId, BetoName, HighTotal),
                new FinalPlace(CesarId, CesarName, MiddleTotal)
            };
            FinalPlace[] standingsExpected =
            {
                new FinalPlace(AnaId, AnaName, HighTotal) { Rank = FirstPlace },
                new FinalPlace(BetoId, BetoName, HighTotal) { Rank = FirstPlace },
                new FinalPlace(CesarId, CesarName, MiddleTotal) { Rank = ThirdPlace }
            };

            IReadOnlyList<FinalPlace> standings = StandingsBuilder.Build(entries);

            Assert.Equivalent(standingsExpected, standings, strict: true);
        }

        [Fact]
        public void Build_PlayersWithTheSameTotal_KeepTheirOriginalOrder()
        {
            FinalPlace[] entries =
            {
                new FinalPlace(AnaId, AnaName, TiedTotal),
                new FinalPlace(BetoId, BetoName, TiedTotal)
            };
            FinalPlace[] standingsExpected =
            {
                new FinalPlace(AnaId, AnaName, TiedTotal) { Rank = FirstPlace },
                new FinalPlace(BetoId, BetoName, TiedTotal) { Rank = FirstPlace }
            };

            IReadOnlyList<FinalPlace> standings = StandingsBuilder.Build(entries);

            Assert.Equivalent(standingsExpected, standings, strict: true);
        }

        [Fact]
        public void Build_NoPlayers_ReturnsAnEmptyList()
        {
            IReadOnlyList<FinalPlace> standings = StandingsBuilder.Build(new FinalPlace[0]);

            Assert.Empty(standings);
        }
    }
}
