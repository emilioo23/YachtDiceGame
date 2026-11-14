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
            List<MatchParticipant> players = CreatePlayers(FirstId, SecondId);

            Assert.Empty(PresenceComparer.FindDeparted(players, players));
        }

        [Fact]
        public void FindDeparted_PlayerMissingNow_ReturnsThatPlayer()
        {
            List<MatchParticipant> before = CreatePlayers(FirstId, SecondId);
            List<MatchParticipant> after = CreatePlayers(FirstId);

            MatchParticipant[] expectedDeparted = { new MatchParticipant(SecondId, SecondName) };

            List<MatchParticipant> departed = PresenceComparer.FindDeparted(before, after);

            Assert.Equivalent(expectedDeparted, departed, strict: true);
        }

        [Fact]
        public void FindDeparted_TwoPlayersMissing_ReturnsBothInOrder()
        {
            List<MatchParticipant> before = CreatePlayers(FirstId, SecondId, ThirdId);
            List<MatchParticipant> after = CreatePlayers(FirstId);

            MatchParticipant[] expectedDeparted =
            {
                new MatchParticipant(SecondId, SecondName),
                new MatchParticipant(ThirdId, ThirdName)
            };

            List<MatchParticipant> departed = PresenceComparer.FindDeparted(before, after);

            Assert.Equivalent(expectedDeparted, departed, strict: true);
        }

        [Fact]
        public void FindDeparted_PlayerArrived_ReturnsNobody()
        {
            List<MatchParticipant> before = CreatePlayers(FirstId);
            List<MatchParticipant> after = CreatePlayers(FirstId, SecondId);

            Assert.Empty(PresenceComparer.FindDeparted(before, after));
        }

        [Fact]
        public void FindArrived_PlayerNewInTheRoom_ReturnsThatPlayer()
        {
            List<MatchParticipant> before = CreatePlayers(FirstId);
            List<MatchParticipant> after = CreatePlayers(FirstId, SecondId);

            MatchParticipant[] expectedArrived = { new MatchParticipant(SecondId, SecondName) };

            List<MatchParticipant> arrived = PresenceComparer.FindArrived(before, after);

            Assert.Equivalent(expectedArrived, arrived, strict: true);
        }

        [Fact]
        public void FindArrived_PlayerLeft_ReturnsNobody()
        {
            List<MatchParticipant> before = CreatePlayers(FirstId, SecondId);
            List<MatchParticipant> after = CreatePlayers(FirstId);

            Assert.Empty(PresenceComparer.FindArrived(before, after));
        }

        [Fact]
        public void FindArrived_NobodyBefore_ReturnsEveryone()
        {
            List<MatchParticipant> after = CreatePlayers(FirstId, SecondId);

            MatchParticipant[] expectedArrived =
            {
                new MatchParticipant(FirstId, FirstName),
                new MatchParticipant(SecondId, SecondName)
            };

            List<MatchParticipant> arrived = PresenceComparer.FindArrived(new List<MatchParticipant>(), after);

            Assert.Equivalent(expectedArrived, arrived, strict: true);
        }

        [Fact]
        public void FindDeparted_SameNameDifferentIdentifier_CountsAsDifferentPlayers()
        {
            var before = new List<MatchParticipant> { new MatchParticipant(FirstId, FirstName) };
            var after = new List<MatchParticipant> { new MatchParticipant(SecondId, FirstName) };

            MatchParticipant[] expectedDeparted = { new MatchParticipant(FirstId, FirstName) };

            List<MatchParticipant> departed = PresenceComparer.FindDeparted(before, after);

            Assert.Equivalent(expectedDeparted, departed, strict: true);
        }

        private static List<MatchParticipant> CreatePlayers(params string[] ids)
        {
            var names = new Dictionary<string, string>
            {
                { FirstId, FirstName },
                { SecondId, SecondName },
                { ThirdId, ThirdName }
            };

            return ids.Select(id => new MatchParticipant(id, names[id])).ToList();
        }
    }
}
