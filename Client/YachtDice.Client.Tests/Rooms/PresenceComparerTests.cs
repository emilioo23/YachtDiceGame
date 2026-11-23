using System.Collections.Generic;
using System.Linq;
using Xunit;
using YachtDice.Client.Logic.Rooms;

namespace YachtDice.Client.Tests.Rooms
{
    /// <summary>
    /// Rules to notice who left a match and who came back.
    /// </summary>
    public class PresenceComparerTests
    {
        private const string FirstId = "id-1";
        private const string SecondId = "id-2";
        private const string ThirdId = "id-3";
        private const string FirstName = "Emilio";
        private const string SecondName = "Gabriel";
        private const string ThirdName = "Ana";

        [Fact]
        public void FindDeparted_SamePlayers_ReturnsNobody()
        {
            List<GameParticipant> players = CreatePlayers(FirstId, SecondId);

            Assert.Empty(PresenceComparer.FindDeparted(players, players));
        }

        [Fact]
        public void FindDeparted_PlayerMissingNow_ReturnsThatPlayer()
        {
            List<GameParticipant> before = CreatePlayers(FirstId, SecondId);
            List<GameParticipant> after = CreatePlayers(FirstId);

            GameParticipant[] departedExpected = { new GameParticipant(SecondId, SecondName) };

            List<GameParticipant> departed = PresenceComparer.FindDeparted(before, after);

            Assert.Equivalent(departedExpected, departed, strict: true);
        }

        [Fact]
        public void FindDeparted_TwoPlayersMissing_ReturnsBothInOrder()
        {
            List<GameParticipant> before = CreatePlayers(FirstId, SecondId, ThirdId);
            List<GameParticipant> after = CreatePlayers(FirstId);

            GameParticipant[] departedExpected =
            {
                new GameParticipant(SecondId, SecondName),
                new GameParticipant(ThirdId, ThirdName)
            };

            List<GameParticipant> departed = PresenceComparer.FindDeparted(before, after);

            Assert.Equivalent(departedExpected, departed, strict: true);
        }

        [Fact]
        public void FindDeparted_PlayerArrived_ReturnsNobody()
        {
            List<GameParticipant> before = CreatePlayers(FirstId);
            List<GameParticipant> after = CreatePlayers(FirstId, SecondId);

            Assert.Empty(PresenceComparer.FindDeparted(before, after));
        }

        [Fact]
        public void FindArrived_PlayerNewInTheRoom_ReturnsThatPlayer()
        {
            List<GameParticipant> before = CreatePlayers(FirstId);
            List<GameParticipant> after = CreatePlayers(FirstId, SecondId);

            GameParticipant[] arrivedExpected = { new GameParticipant(SecondId, SecondName) };

            List<GameParticipant> arrived = PresenceComparer.FindArrived(before, after);

            Assert.Equivalent(arrivedExpected, arrived, strict: true);
        }

        [Fact]
        public void FindArrived_PlayerLeft_ReturnsNobody()
        {
            List<GameParticipant> before = CreatePlayers(FirstId, SecondId);
            List<GameParticipant> after = CreatePlayers(FirstId);

            Assert.Empty(PresenceComparer.FindArrived(before, after));
        }

        [Fact]
        public void FindArrived_NobodyBefore_ReturnsEveryone()
        {
            List<GameParticipant> after = CreatePlayers(FirstId, SecondId);

            GameParticipant[] arrivedExpected =
            {
                new GameParticipant(FirstId, FirstName),
                new GameParticipant(SecondId, SecondName)
            };

            List<GameParticipant> arrived = PresenceComparer.FindArrived(new List<GameParticipant>(), after);

            Assert.Equivalent(arrivedExpected, arrived, strict: true);
        }

        [Fact]
        public void FindDeparted_SameNameDifferentIdentifier_CountsAsDifferentPlayers()
        {
            var before = new List<GameParticipant> { new GameParticipant(FirstId, FirstName) };
            var after = new List<GameParticipant> { new GameParticipant(SecondId, FirstName) };

            GameParticipant[] departedExpected = { new GameParticipant(FirstId, FirstName) };

            List<GameParticipant> departed = PresenceComparer.FindDeparted(before, after);

            Assert.Equivalent(departedExpected, departed, strict: true);
        }

        private static List<GameParticipant> CreatePlayers(params string[] ids)
        {
            var names = new Dictionary<string, string>
            {
                { FirstId, FirstName },
                { SecondId, SecondName },
                { ThirdId, ThirdName }
            };

            return ids.Select(id => new GameParticipant(id, names[id])).ToList();
        }
    }
}
