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
    public class GamePlayerFactoryTests
    {
        private const string LocalId = "local";
        private const string OpponentId = "opponent";
        private const string LocalName = "Ana";
        private const string OpponentName = "Beto";
        private const string ComputerId = "computer";
        private const string ComputerName = "CPU";

        private readonly RoomPlayer _localPlayer = new RoomPlayer { Id = LocalId, DisplayName = LocalName };
        private readonly RoomPlayer _opponent = new RoomPlayer { Id = OpponentId, DisplayName = OpponentName };

        [Fact]
        public void Create_LocalPlayerAfterOpponent_PutsTheLocalPlayerFirstAndMarksIt()
        {
            RoomPlayer[] roomPlayers = { _opponent, _localPlayer };
            GamePlayer[] playersExpected =
            {
                CreateGamePlayer(_localPlayer, true, false),
                CreateGamePlayer(_opponent, false, false)
            };

            List<GamePlayer> players = GamePlayerFactory.Create(roomPlayers, LocalId);

            Assert.Equivalent(playersExpected, players, strict: true);
        }

        [Fact]
        public void CreateForRoom_AbsentOpponent_MarksTheOpponentAsLeft()
        {
            var room = new RoomState
            {
                Players = new[] { _localPlayer },
                AbsentPlayers = new[] { _opponent }
            };
            GamePlayer[] playersExpected =
            {
                CreateGamePlayer(_localPlayer, true, false),
                CreateGamePlayer(_opponent, false, true)
            };

            List<GamePlayer> players = GamePlayerFactory.CreateForRoom(room, LocalId);

            Assert.Equivalent(playersExpected, players, strict: true);
        }

        [Fact]
        public void UpdatePresence_BothPlayersInside_KeepsEveryonePresent()
        {
            RoomPlayer[] roomPlayers = { _localPlayer, _opponent };
            List<GamePlayer> players = GamePlayerFactory.Create(roomPlayers, LocalId);
            GamePlayer[] playersExpected =
            {
                CreateGamePlayer(_localPlayer, true, false),
                CreateGamePlayer(_opponent, false, false)
            };

            List<GamePlayer> updated = GamePlayerFactory.UpdatePresence(players, roomPlayers);

            Assert.Equivalent(playersExpected, updated, strict: true);
        }

        [Fact]
        public void Create_ComputerOpponent_MarksThePlayerAsComputer()
        {
            var computer = new RoomPlayer { Id = ComputerId, DisplayName = ComputerName, IsComputer = true };
            RoomPlayer[] roomPlayers = { _localPlayer, computer };
            GamePlayer[] playersExpected =
            {
                CreateGamePlayer(_localPlayer, true, false),
                CreateGamePlayer(computer, false, false)
            };

            List<GamePlayer> players = GamePlayerFactory.Create(roomPlayers, LocalId);

            Assert.Equivalent(playersExpected, players, strict: true);
        }

        [Fact]
        public void UpdatePresence_ComputerOpponent_KeepsThePlayerAsComputer()
        {
            var computer = new RoomPlayer { Id = ComputerId, DisplayName = ComputerName, IsComputer = true };
            RoomPlayer[] roomPlayers = { _localPlayer, computer };
            List<GamePlayer> players = GamePlayerFactory.Create(roomPlayers, LocalId);
            GamePlayer[] playersExpected =
            {
                CreateGamePlayer(_localPlayer, true, false),
                CreateGamePlayer(computer, false, false)
            };

            List<GamePlayer> updated = GamePlayerFactory.UpdatePresence(players, roomPlayers);

            Assert.Equivalent(playersExpected, updated, strict: true);
        }

        private static GamePlayer CreateGamePlayer(RoomPlayer roomPlayer, bool isLocalPlayer, bool hasLeft)
        {
            var player = new GamePlayer
            {
                Id = roomPlayer.Id,
                Name = roomPlayer.DisplayName,
                IsLocalPlayer = isLocalPlayer,
                IsComputer = roomPlayer.IsComputer,
                HasLeft = hasLeft
            };
            return player;
        }
    }
}
