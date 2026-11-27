using System;
using Xunit;
using YachtDice.Server.Contracts.Presence;
using YachtDice.Server.Logic.Security;
using YachtDice.Server.Services;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Services
{
    /// <summary>
    /// The list of players who are online: only the player of a valid session can appear online, so nobody
    /// can receive the notices of another player by sending a name that is not the own one.
    /// </summary>
    public class PresenceServiceTests
    {
        private const string PlayerName = "gabrielhernandez";
        private const string UnknownToken = "token-inventado";
        private const string NoToken = "";
        private const string SenderName = "emilioo23";
        private const string RoomCode = "ABC123";

        private readonly SessionRegistry _sessions = new SessionRegistry();
        private readonly PresenceService _service;

        /// <summary>
        /// Prepares the service with sessions kept in memory and no player online.
        /// </summary>
        public PresenceServiceTests()
        {
            _service = new PresenceService(new InMemoryFriendshipService(), _sessions);
        }

        [Fact]
        public void Constructor_NullPlayerIdFinder_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PresenceService(null, _sessions));
        }

        [Fact]
        public void Constructor_NullSessions_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PresenceService(new InMemoryFriendshipService(), null));
        }

        [Fact]
        public void Connect_UnknownToken_ReturnsNoOnlinePlayers()
        {
            string[] onlinePlayers = _service.Connect(UnknownToken);

            Assert.Empty(onlinePlayers);
        }

        [Fact]
        public void Connect_NullToken_ReturnsNoOnlinePlayers()
        {
            string[] onlinePlayers = _service.Connect(null);

            Assert.Empty(onlinePlayers);
        }

        [Fact]
        public void Connect_EmptyToken_ReturnsNoOnlinePlayers()
        {
            string[] onlinePlayers = _service.Connect(NoToken);

            Assert.Empty(onlinePlayers);
        }

        [Fact]
        public void Connect_TokenOfAnEndedSession_ReturnsNoOnlinePlayers()
        {
            string token = _sessions.Start(PlayerName);
            _sessions.EndFor(PlayerName);

            string[] onlinePlayers = _service.Connect(token);

            Assert.Empty(onlinePlayers);
        }

        [Fact]
        public void SendInvitation_PlayerWhoSentAnUnknownToken_DoesNotDeliverIt()
        {
            var invitation = new RoomInvitation { SenderName = SenderName, RoomCode = RoomCode };
            _service.Connect(UnknownToken);

            bool isDelivered = _service.SendInvitation(PlayerName, invitation);

            Assert.False(isDelivered);
        }
    }
}
