using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using YachtDice.Server.Logic.Game;
using YachtDice.Server.Logic.Moderation;
using YachtDice.Server.Logic.Rooms;
using YachtDice.Server.Tests.InMemory;
using Player = YachtDice.Server.Data.Player;

namespace YachtDice.Server.Tests.Game
{
    /// <summary>
    /// Games in progress of registered players that are saved after every move, so they can go on after a restart
    /// of the server.
    /// </summary>
    public class GameCheckpointServiceTests
    {
        private const string HostName = "Emilio";
        private const string FriendName = "Gabriel";
        private const string GuestName = "Invitado";
        private const int HostAccountId = 1;
        private const int FriendAccountId = 2;
        private const int TurnSeconds = 60;
        private const int OneRollMade = 1;
        private const int OneSave = 1;
        private const int FirstDiePosition = 1;

        private static readonly int[] LittleStraightDice = { 1, 2, 3, 4, 5 };
        private static readonly bool[] NoHeldDice = { false, false, false, false, false };

        private readonly InMemoryPlayerRepository _players = new InMemoryPlayerRepository();
        private readonly InMemoryGameCheckpointRepository _checkpoints = new InMemoryGameCheckpointRepository();
        private readonly RoomManager _roomManager = new RoomManager(
            new InMemoryAvatarLookup(),
            new OffensiveWordFilter(new string[0]));
        private readonly GameManager _gameManager;
        private readonly GameCheckpointService _service;

        /// <summary>
        /// Prepares two registered players, a game that always rolls a little straight and the service.
        /// </summary>
        public GameCheckpointServiceTests()
        {
            var secondsBySpeed = new Dictionary<GameSpeed, int>
            {
                { GameSpeed.Normal, TurnSeconds },
                { GameSpeed.Fast, TurnSeconds }
            };

            _players.Add(new Player { Id = HostAccountId, DisplayName = HostName });
            _players.Add(new Player { Id = FriendAccountId, DisplayName = FriendName });
            _gameManager = new GameManager(new InMemoryDieRoller(LittleStraightDice), new TurnDuration(secondsBySpeed));
            _service = new GameCheckpointService(_players, _checkpoints);
        }

        [Fact]
        public void Track_EveryPlayerHasAnAccount_ReturnsTrue()
        {
            bool isTracked = _service.Track(CreateRoom(FriendName));

            Assert.True(isTracked);
        }

        [Fact]
        public void Track_RoomWithAGuest_ReturnsFalse()
        {
            bool isTracked = _service.Track(CreateRoom(GuestName));

            Assert.False(isTracked);
        }

        [Fact]
        public void Track_RoomWithAComputerPlayer_ReturnsFalse()
        {
            RoomResult created = _roomManager.CreateRoom(HostName);
            RoomResult withComputer = _roomManager.AddComputer(
                new RoomMember(created.Room.Code, created.PlayerId),
                ComputerDifficulty.Normal);

            bool isTracked = _service.Track(withComputer.Room);

            Assert.False(isTracked);
        }

        [Fact]
        public void Save_TrackedGameAfterARoll_SavesTheDiceAndThePlayerWithTheTurn()
        {
            RoomDetails room = CreateRoom(FriendName);
            _service.Track(room);
            GameSnapshot snapshot = StartAndRoll(room);
            var checkpointExpected = new
            {
                CurrentPlayerName = HostName,
                RollsMade = OneRollMade,
                Dice = LittleStraightDice
                    .Select((value, index) => new { Position = index + FirstDiePosition, Value = value })
                    .ToArray()
            };

            _service.Save(room.Code, snapshot);

            Assert.Equivalent(checkpointExpected, _checkpoints.Find(room.Code));
        }

        [Fact]
        public void Save_GameThatIsNotTracked_SavesNothing()
        {
            RoomDetails room = CreateRoom(FriendName);
            GameSnapshot snapshot = StartAndRoll(room);

            _service.Save(room.Code, snapshot);

            Assert.Null(_checkpoints.Find(room.Code));
        }

        [Fact]
        public void Save_OlderState_DoesNotReplaceTheNewerOne()
        {
            RoomDetails room = CreateRoom(FriendName);
            _service.Track(room);
            GameSnapshot olderSnapshot = StartGame(room);
            GameSnapshot newerSnapshot = Roll(room);

            _service.Save(room.Code, newerSnapshot);
            _service.Save(room.Code, olderSnapshot);

            Assert.Equal(OneSave, _checkpoints.SaveCount);
        }

        [Fact]
        public void Forget_TrackedGame_RemovesTheSavedGame()
        {
            RoomDetails room = CreateRoom(FriendName);
            _service.Track(room);
            _service.Save(room.Code, StartAndRoll(room));

            _service.Forget(room.Code);

            Assert.Null(_checkpoints.Find(room.Code));
        }

        [Fact]
        public void LoadAll_SavedGame_KeepsThePlayersInTheOrderOfTheTurnsAndTheDice()
        {
            RoomDetails room = CreateRoom(FriendName);
            _service.Track(room);
            _service.Save(room.Code, StartAndRoll(room));
            var gamesExpected = new[]
            {
                new
                {
                    RoomCode = room.Code,
                    CurrentPlayerName = HostName,
                    RollsMade = OneRollMade,
                    Dice = LittleStraightDice,
                    HeldDice = NoHeldDice,
                    Players = new[] { new { Name = HostName }, new { Name = FriendName } }
                }
            };

            List<SavedGame> games = new GameCheckpointService(_players, _checkpoints).LoadAll();

            Assert.Equivalent(gamesExpected, games);
        }

        [Fact]
        public void Track_NullRoom_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.Track(null));
        }

        [Fact]
        public void Constructor_NullPlayerRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new GameCheckpointService(null, _checkpoints));
        }

        [Fact]
        public void Constructor_NullCheckpointRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new GameCheckpointService(_players, null));
        }

        private RoomDetails CreateRoom(string secondPlayerName)
        {
            RoomResult created = _roomManager.CreateRoom(HostName);
            RoomResult joined = _roomManager.Join(created.Room.Code, secondPlayerName);

            return joined.Room;
        }

        private GameSnapshot StartGame(RoomDetails room)
        {
            IEnumerable<GameParticipant> participants =
                room.Players.Select(player => new GameParticipant(player.Id, player.DisplayName));

            return _gameManager.Start(room.Code, participants, GameSpeed.Normal);
        }

        private GameSnapshot Roll(RoomDetails room)
        {
            var member = new RoomMember(room.Code, room.Players.First().Id);
            GameActionResult rollResult = _gameManager.Roll(member, NoHeldDice);

            return rollResult.Snapshot;
        }

        private GameSnapshot StartAndRoll(RoomDetails room)
        {
            StartGame(room);

            return Roll(room);
        }
    }
}
