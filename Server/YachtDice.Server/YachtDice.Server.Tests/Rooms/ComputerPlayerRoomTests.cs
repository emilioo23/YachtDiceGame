using System;
using System.Linq;
using Xunit;
using YachtDice.Server.Logic.Moderation;
using YachtDice.Server.Logic.Rooms;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Rooms
{
    /// <summary>
    /// Computer players inside a room: only the host adds them, they are always ready, they never become
    /// the host and a room with only computer players is closed.
    /// </summary>
    public class ComputerPlayerRoomTests
    {
        private const string HostName = "Emilio";
        private const string PlayerName = "Gabriel";
        private const string ComputerName = "CPU";
        private const string OffensiveWord = "pendejo";

        private readonly RoomManager _manager =
            new RoomManager(new InMemoryAvatarLookup(), new OffensiveWordFilter(new[] { OffensiveWord }));

        [Fact]
        public void AddComputer_Host_AddsAComputerPlayerWhoIsReady()
        {
            RoomResult createdRoom = _manager.CreateRoom(HostName);
            var computerExpected = new { DisplayName = ComputerName, IsComputer = true, IsReady = true };

            RoomResult addResult = _manager.AddComputer(GetMember(createdRoom), ComputerDifficulty.Normal);

            Assert.Equivalent(computerExpected, FindComputer(addResult.Room));
        }

        [Fact]
        public void AddComputer_PlayerWhoIsNotTheHost_ReturnsNotHost()
        {
            RoomResult createdRoom = _manager.CreateRoom(HostName);
            RoomResult joinedRoom = _manager.Join(createdRoom.Room.Code, PlayerName);

            RoomResult addResult = _manager.AddComputer(GetMember(joinedRoom), ComputerDifficulty.Normal);

            Assert.Equal(RoomStatus.NotHost, addResult.Status);
        }

        [Fact]
        public void AddComputer_FullRoom_ReturnsRoomFull()
        {
            RoomResult createdRoom = _manager.CreateRoom(HostName);
            RoomMember host = GetMember(createdRoom);
            int freePlaces = createdRoom.Room.Settings.MaxPlayers - createdRoom.Room.Players.Count;

            for (int place = 0; place < freePlaces; place++)
            {
                _manager.AddComputer(host, ComputerDifficulty.Normal);
            }

            RoomResult addResult = _manager.AddComputer(host, ComputerDifficulty.Normal);

            Assert.Equal(RoomStatus.RoomFull, addResult.Status);
        }

        [Fact]
        public void StartGame_HostAndComputer_StartsTheGame()
        {
            RoomResult createdRoom = _manager.CreateRoom(HostName);
            RoomMember host = GetMember(createdRoom);
            _manager.AddComputer(host, ComputerDifficulty.Normal);

            RoomResult startResult = _manager.StartGame(host);

            Assert.Equal(RoomStatus.Success, startResult.Status);
        }

        [Fact]
        public void Leave_HostLeavesOnlyComputers_LeavesTheRoomEmpty()
        {
            RoomResult createdRoom = _manager.CreateRoom(HostName);
            RoomMember host = GetMember(createdRoom);
            _manager.AddComputer(host, ComputerDifficulty.Normal);

            RoomResult leaveResult = _manager.Leave(host);

            Assert.True(leaveResult.Room.IsEmpty);
        }

        [Fact]
        public void Leave_HostLeavesOnlyComputers_ClosesTheRoom()
        {
            RoomResult createdRoom = _manager.CreateRoom(HostName);
            RoomMember host = GetMember(createdRoom);
            _manager.AddComputer(host, ComputerDifficulty.Normal);
            _manager.Leave(host);

            RoomResult joinResult = _manager.Join(createdRoom.Room.Code, PlayerName);

            Assert.Equal(RoomStatus.RoomNotFound, joinResult.Status);
        }

        [Fact]
        public void Leave_HostBeforeAPersonAndAfterAComputer_GivesTheHostToThePerson()
        {
            RoomResult createdRoom = _manager.CreateRoom(HostName);
            RoomMember host = GetMember(createdRoom);
            _manager.AddComputer(host, ComputerDifficulty.Normal);
            RoomResult joinedRoom = _manager.Join(createdRoom.Room.Code, PlayerName);

            RoomResult leaveResult = _manager.Leave(host);

            Assert.Equal(joinedRoom.PlayerId, leaveResult.Room.HostId);
        }

        [Fact]
        public void TransferHost_ToAComputer_ReturnsInvalidTarget()
        {
            RoomResult createdRoom = _manager.CreateRoom(HostName);
            RoomMember host = GetMember(createdRoom);
            RoomPlayer computer = FindComputer(_manager.AddComputer(host, ComputerDifficulty.Normal).Room);

            RoomResult transferResult = _manager.TransferHost(host, computer.Id);

            Assert.Equal(RoomStatus.InvalidTarget, transferResult.Status);
        }

        private static RoomMember GetMember(RoomResult roomResult)
        {
            return new RoomMember(roomResult.Room.Code, roomResult.PlayerId);
        }

        private static RoomPlayer FindComputer(RoomDetails room)
        {
            Func<RoomPlayer, bool> isComputer = player => player.IsComputer;

            return room.Players.Single(isComputer);
        }
    }
}
