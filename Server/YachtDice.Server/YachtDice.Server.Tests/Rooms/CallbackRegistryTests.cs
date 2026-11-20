using System;
using Xunit;
using YachtDice.Server.Logic.Rooms;
using YachtDice.Server.Services;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Rooms
{
    /// <summary>
    /// The channels of the players of each room, used by the server to send them the events of the room.
    /// A channel that fails is removed so the server stops sending to a client that left.
    /// </summary>
    public class CallbackRegistryTests
    {
        private const string RoomCode = "ABC123";
        private const string OtherRoomCode = "XYZ789";
        private const string PlayerId = "player-1";
        private const string OtherPlayerId = "player-2";
        private const int TwoDeliveries = 2;

        private static readonly Action<FakeCallbackChannel> Deliver = channel => channel.Receive();

        private readonly CallbackRegistry<FakeCallbackChannel> _registry = new CallbackRegistry<FakeCallbackChannel>();
        private readonly FakeCallbackChannel _channel = new FakeCallbackChannel();
        private readonly FakeCallbackChannel _otherChannel = new FakeCallbackChannel();
        private readonly RoomMember _member = new RoomMember(RoomCode, PlayerId);

        [Fact]
        public void Notify_TwoPlayersOfTheRoom_DeliversToBoth()
        {
            _registry.Register(_member, _channel);
            _registry.Register(new RoomMember(RoomCode, OtherPlayerId), _channel);

            _registry.Notify(RoomCode, Deliver);

            Assert.Equal(TwoDeliveries, _channel.ReceivedCount);
        }

        [Fact]
        public void Notify_PlayerOfAnotherRoom_DoesNotDeliver()
        {
            _registry.Register(new RoomMember(OtherRoomCode, OtherPlayerId), _otherChannel);

            _registry.Notify(RoomCode, Deliver);

            Assert.Equal(0, _otherChannel.ReceivedCount);
        }

        [Fact]
        public void Remove_RegisteredPlayer_StopsTheNotifications()
        {
            _registry.Register(_member, _channel);
            _registry.Remove(_member);

            _registry.Notify(RoomCode, Deliver);

            Assert.Equal(0, _channel.ReceivedCount);
        }

        [Fact]
        public void RemoveIfCurrent_OldChannelOfAPlayerWhoCameBack_ReturnsFalse()
        {
            _registry.Register(_member, _channel);
            _registry.Register(_member, _otherChannel);

            bool isRemoved = _registry.RemoveIfCurrent(_member, _channel);

            Assert.False(isRemoved);
        }

        [Fact]
        public void Notify_BrokenChannel_RemovesThePlayer()
        {
            _channel.IsBroken = true;
            _registry.Register(_member, _channel);
            _registry.Notify(RoomCode, Deliver);
            _channel.IsBroken = false;

            _registry.Notify(RoomCode, Deliver);

            Assert.Equal(0, _channel.ReceivedCount);
        }

        [Fact]
        public void NotifyPlayer_OnlyThatPlayer_DoesNotDeliverToTheOthers()
        {
            _registry.Register(_member, _channel);
            _registry.Register(new RoomMember(RoomCode, OtherPlayerId), _otherChannel);

            _registry.NotifyPlayer(_member, Deliver);

            Assert.Equal(0, _otherChannel.ReceivedCount);
        }
    }
}
