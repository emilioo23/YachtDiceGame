using System.Collections.Generic;
using Xunit;
using YachtDice.Models;
using YachtDice.RoomServiceReference;
using YachtDice.Utils;

namespace YachtDice.Client.Tests.Utils
{
    /// <summary>
    /// Players shown at the top of the scorecard of a match.
    /// </summary>
    public class MatchPlayerFactoryTests
    {
        private const string LocalId = "local";
        private const string OpponentId = "opponent";
        private const string LocalName = "Ana";
        private const string OpponentName = "Beto";

        private readonly RoomPlayer _localPlayer = new RoomPlayer { Id = LocalId, DisplayName = LocalName };
        private readonly RoomPlayer _opponent = new RoomPlayer { Id = OpponentId, DisplayName = OpponentName };

        [Fact]
        public void Create_LocalPlayerAfterOpponent_PutsTheLocalPlayerFirstAndMarksIt()
        {
            RoomPlayer[] roomPlayers = { _opponent, _localPlayer };
            MatchPlayer[] expectedPlayers =
            {
                CreateMatchPlayer(_localPlayer, true, false),
                CreateMatchPlayer(_opponent, false, false)
            };

            List<MatchPlayer> players = MatchPlayerFactory.Create(roomPlayers, LocalId);

            Assert.Equivalent(expectedPlayers, players, strict: true);
        }

        [Fact]
        public void CreateForRoom_AbsentOpponent_MarksTheOpponentAsLeft()
        {
            var room = new RoomState
            {
                Players = new[] { _localPlayer },
                AbsentPlayers = new[] { _opponent }
            };
            MatchPlayer[] expectedPlayers =
            {
                CreateMatchPlayer(_localPlayer, true, false),
                CreateMatchPlayer(_opponent, false, true)
            };

            List<MatchPlayer> players = MatchPlayerFactory.CreateForRoom(room, LocalId);

            Assert.Equivalent(expectedPlayers, players, strict: true);
        }

        [Fact]
        public void UpdatePresence_BothPlayersInside_KeepsEveryonePresent()
        {
            RoomPlayer[] roomPlayers = { _localPlayer, _opponent };
            List<MatchPlayer> players = MatchPlayerFactory.Create(roomPlayers, LocalId);
            MatchPlayer[] expectedPlayers =
            {
                CreateMatchPlayer(_localPlayer, true, false),
                CreateMatchPlayer(_opponent, false, false)
            };

            List<MatchPlayer> updated = MatchPlayerFactory.UpdatePresence(players, roomPlayers);

            Assert.Equivalent(expectedPlayers, updated, strict: true);
        }

        private static MatchPlayer CreateMatchPlayer(RoomPlayer roomPlayer, bool isLocalPlayer, bool hasLeft)
        {
            var player = new MatchPlayer
            {
                Id = roomPlayer.Id,
                Name = roomPlayer.DisplayName,
                IsLocalPlayer = isLocalPlayer,
                HasLeft = hasLeft
            };
            return player;
        }
    }
}
