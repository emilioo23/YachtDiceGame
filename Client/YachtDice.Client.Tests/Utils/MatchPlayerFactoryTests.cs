using System.Collections.Generic;
using System.Linq;
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
        public void Create_LocalPlayerAfterOpponent_PutsTheLocalPlayerFirst()
        {
            RoomPlayer[] roomPlayers = { _opponent, _localPlayer };

            List<MatchPlayer> players = MatchPlayerFactory.Create(roomPlayers, LocalId);

            Assert.Equal(new[] { LocalId, OpponentId }, players.Select(player => player.Id).ToArray());
        }

        [Fact]
        public void Create_TwoPlayers_MarksOnlyTheLocalPlayer()
        {
            RoomPlayer[] roomPlayers = { _opponent, _localPlayer };

            List<MatchPlayer> players = MatchPlayerFactory.Create(roomPlayers, LocalId);

            Assert.Equal(new[] { true, false }, players.Select(player => player.IsLocalPlayer).ToArray());
        }

        [Fact]
        public void CreateForRoom_AbsentOpponent_MarksTheOpponentAsLeft()
        {
            var room = new RoomState
            {
                Players = new[] { _localPlayer },
                AbsentPlayers = new[] { _opponent }
            };

            List<MatchPlayer> players = MatchPlayerFactory.CreateForRoom(room, LocalId);

            Assert.Equal(new[] { false, true }, players.Select(player => player.HasLeft).ToArray());
        }

        [Fact]
        public void UpdatePresence_BothPlayersInside_KeepsEveryonePresent()
        {
            RoomPlayer[] roomPlayers = { _localPlayer, _opponent };
            List<MatchPlayer> players = MatchPlayerFactory.Create(roomPlayers, LocalId);

            List<MatchPlayer> updated = MatchPlayerFactory.UpdatePresence(players, roomPlayers);

            Assert.DoesNotContain(updated, player => player.HasLeft);
        }
    }
}
