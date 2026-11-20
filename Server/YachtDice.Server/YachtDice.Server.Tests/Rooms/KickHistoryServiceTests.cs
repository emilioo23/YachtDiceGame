using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using YachtDice.Server.Data;
using YachtDice.Server.Logic.Rooms;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Rooms
{
    /// <summary>
    /// History of expulsions: only expulsions between two accounts are saved, and each player sees the ones where
    /// they were the host or the expelled player.
    /// </summary>
    public class KickHistoryServiceTests
    {
        private const int HostId = 1;
        private const int TargetId = 2;
        private const int OtherId = 3;
        private const string HostName = "gabriel";
        private const string TargetName = "emilio";
        private const string OtherName = "ana";
        private const string GuestName = "Invitado123";
        private const string RoomCode = "ABC123";

        private readonly InMemoryPlayerRepository _players = new InMemoryPlayerRepository();
        private readonly InMemoryKickLogRepository _kicks;
        private readonly KickHistoryService _service;

        /// <summary>
        /// Prepares three registered players.
        /// </summary>
        public KickHistoryServiceTests()
        {
            _players.Add(new Player { Id = HostId, Username = HostName, DisplayName = HostName });
            _players.Add(new Player { Id = TargetId, Username = TargetName, DisplayName = TargetName });
            _players.Add(new Player { Id = OtherId, Username = OtherName, DisplayName = OtherName });
            _kicks = new InMemoryKickLogRepository(_players);
            _service = new KickHistoryService(_players, _kicks);
        }

        [Fact]
        public void Constructor_NullKickLogRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new KickHistoryService(_players, null));
        }

        [Fact]
        public void Record_TwoAccounts_SavesTheExpulsion()
        {
            var kickExpected = new { HostId, TargetId, RoomCode };

            _service.Record(RoomCode, HostName, TargetName);

            Assert.Equivalent(kickExpected, _kicks.Kicks.Single());
        }

        [Fact]
        public void Record_ExpelledGuest_DoesNotSaveAnything()
        {
            bool isSaved = _service.Record(RoomCode, HostName, GuestName);

            Assert.False(isSaved);
        }

        [Fact]
        public void GetHistory_ExpelledPlayer_SeesWhoExpelledThem()
        {
            _service.Record(RoomCode, HostName, TargetName);
            var historyExpected = new[] { new { HostName, TargetName, RoomCode } };

            List<KickHistoryEntry> history = _service.GetHistory(TargetName);

            Assert.Equivalent(historyExpected, history);
        }

        [Fact]
        public void GetHistory_PlayerWhoTookNoPart_ReturnsAnEmptyList()
        {
            _service.Record(RoomCode, HostName, TargetName);

            List<KickHistoryEntry> history = _service.GetHistory(OtherName);

            Assert.Empty(history);
        }
    }
}
