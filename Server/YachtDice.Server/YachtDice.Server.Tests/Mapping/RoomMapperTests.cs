using System.Linq;
using Xunit;
using YachtDice.Server.Logic.Moderation;
using YachtDice.Server.Logic.Rooms;
using YachtDice.Server.Services.Mapping;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Mapping
{
    /// <summary>
    /// Conversion of a room of the logic layer into the data contract that the clients receive.
    /// </summary>
    public class RoomMapperTests
    {
        private const string HostName = "Emilio";
        private const string GuestName = "Invitado";
        private const int RegisteredLevel = 1;

        private readonly InMemoryAvatarLookup _avatars = new InMemoryAvatarLookup();
        private readonly RoomManager _manager;

        public RoomMapperTests()
        {
            _manager = new RoomManager(_avatars, new OffensiveWordFilter(new string[0]));
        }

        [Fact]
        public void ToContract_PlayerWithAnAccount_IsRegistered()
        {
            _avatars.SetLevel(HostName, RegisteredLevel);
            RoomResult createdRoom = _manager.CreateRoom(HostName);

            YachtDice.Server.Contracts.Rooms.RoomState room = RoomMapper.ToContract(createdRoom.Room);

            Assert.True(room.Players.Single().IsRegistered);
        }

        [Fact]
        public void ToContract_GuestWithoutLevel_IsNotRegistered()
        {
            RoomResult createdRoom = _manager.CreateRoom(GuestName);

            YachtDice.Server.Contracts.Rooms.RoomState room = RoomMapper.ToContract(createdRoom.Room);

            Assert.False(room.Players.Single().IsRegistered);
        }

        [Fact]
        public void ToContract_ComputerPlayer_IsNotRegistered()
        {
            _avatars.SetLevel(HostName, RegisteredLevel);
            RoomResult createdRoom = _manager.CreateRoom(HostName);
            var host = new RoomMember(createdRoom.Room.Code, createdRoom.PlayerId);
            RoomResult withComputer = _manager.AddComputer(host, ComputerDifficulty.Normal);

            YachtDice.Server.Contracts.Rooms.RoomState room = RoomMapper.ToContract(withComputer.Room);

            Assert.False(room.Players.Single(player => player.IsComputer).IsRegistered);
        }
    }
}
