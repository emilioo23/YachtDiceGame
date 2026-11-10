using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using YachtDice.Server.Logic.Avatars;
using YachtDice.Server.Logic.Rooms;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Rooms
{
    /// <summary>
    /// Rules of the rooms: the server recognizes each player by the identifier it gives, and the display name
    /// is only a label that can be repeated.
    /// </summary>
    public class RoomManagerTests
    {
        private const string GuestName = "Guest";
        private const string SecondGuestName = "Guest 2";
        private const string ThirdGuestName = "Guest 3";
        private const string LowercaseGuestName = "guest";
        private const string SecondLowercaseGuestName = "guest 2";
        private const string HostName = "Emilio";
        private const string PlayerName = "Gabriel";
        private const string UnknownRoomCode = "ZZZZZZ";
        private const string UnknownPlayerId = "not-a-player";
        private const int TwoPlayers = 2;
        private const int ThreePlayers = 3;
        private const int FourPlayers = 4;
        private const int SingleSlot = 1;
        private const int HostAvatarId = 3;
        private const int PlayerAvatarId = 7;

        private readonly InMemoryAvatarLookup _avatars = new InMemoryAvatarLookup();
        private readonly RoomManager _manager;

        /// <summary>
        /// Prepares a manager without rooms whose players have the avatars set in the test.
        /// </summary>
        public RoomManagerTests()
        {
            _manager = new RoomManager(_avatars);
        }

        [Fact]
        public void CreateRoom_ValidName_PutsTheHostInsideWithAnIdentifier()
        {
            var expected = new
            {
                Status = RoomStatus.Success,
                HasIdentifier = true,
                IsHost = true,
                DisplayName = HostName
            };

            RoomResult result = _manager.CreateRoom(HostName);

            var actual = new
            {
                result.Status,
                HasIdentifier = !string.IsNullOrEmpty(result.PlayerId),
                IsHost = result.PlayerId == result.Room.HostId,
                result.Room.Players.Single().DisplayName
            };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CreateRoom_BlankName_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => _manager.CreateRoom(" "));
        }

        [Fact]
        public void Join_NameFreeInTheRoom_KeepsTheNameAndGivesANewIdentifier()
        {
            RoomResult host = _manager.CreateRoom(HostName);

            var expected = new { Status = RoomStatus.Success, DisplayName = PlayerName, HasNewIdentifier = true };

            RoomResult player = _manager.Join(host.Room.Code, PlayerName);

            var actual = new
            {
                player.Status,
                player.Room.Players.Last().DisplayName,
                HasNewIdentifier = player.PlayerId != host.PlayerId
            };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Join_NameUsedByAnotherPlayer_AddsANumberToTheName()
        {
            RoomResult host = _manager.CreateRoom(GuestName);

            var expected = new { Status = RoomStatus.Success, DisplayName = SecondGuestName, HasNewIdentifier = true };

            RoomResult second = _manager.Join(host.Room.Code, GuestName);

            var actual = new
            {
                second.Status,
                second.Room.Players.Last().DisplayName,
                HasNewIdentifier = second.PlayerId != host.PlayerId
            };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Join_SameNameThreeTimes_NumbersEachPlayer()
        {
            RoomResult host = _manager.CreateRoom(GuestName);
            _manager.Join(host.Room.Code, GuestName);

            RoomResult third = _manager.Join(host.Room.Code, GuestName);

            string[] names = third.Room.Players.Select(player => player.DisplayName).ToArray();
            Assert.Equal(new[] { GuestName, SecondGuestName, ThirdGuestName }, names);
        }

        [Fact]
        public void Join_NameUsedWithDifferentCase_AddsANumberToTheName()
        {
            RoomResult host = _manager.CreateRoom(GuestName);

            RoomResult second = _manager.Join(host.Room.Code, LowercaseGuestName);

            Assert.Equal(SecondLowercaseGuestName, second.Room.Players.Last().DisplayName);
        }

        [Fact]
        public void Join_CodeInLowercaseWithSpaces_FindsTheRoom()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            string typedCode = " " + host.Room.Code.ToLowerInvariant() + " ";

            RoomResult player = _manager.Join(typedCode, PlayerName);

            Assert.Equal(RoomStatus.Success, player.Status);
        }

        [Fact]
        public void Join_UnknownRoom_ReturnsRoomNotFound()
        {
            var expected = new { Status = RoomStatus.RoomNotFound, PlayerId = string.Empty };

            RoomResult player = _manager.Join(UnknownRoomCode, PlayerName);

            var actual = new { player.Status, player.PlayerId };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Join_RoomWithoutSpace_ReturnsRoomFull()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            _manager.UpdateSettings(host.Room.Code, host.PlayerId, CreateSettings(TwoPlayers));
            _manager.Join(host.Room.Code, PlayerName);

            RoomResult player = _manager.Join(host.Room.Code, GuestName);

            Assert.Equal(RoomStatus.RoomFull, player.Status);
        }

        [Fact]
        public void Join_GameAlreadyStarted_ReturnsGameAlreadyStarted()
        {
            RoomResult host = CreateStartedRoom();

            RoomResult player = _manager.Join(host.Room.Code, GuestName);

            Assert.Equal(RoomStatus.GameAlreadyStarted, player.Status);
        }

        [Fact]
        public void Leave_GameAlreadyStarted_KeepsThePlayerAsAbsent()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            RoomResult player = _manager.Join(host.Room.Code, PlayerName);
            _manager.StartGame(host.Room.Code, host.PlayerId);

            var expected = new { IsStarted = true, IsPlaying = false, IsAbsent = true };

            RoomResult afterLeaving = _manager.Leave(host.Room.Code, player.PlayerId);

            var actual = new
            {
                afterLeaving.Room.IsStarted,
                IsPlaying = afterLeaving.Room.Players.Any(remaining => remaining.Id == player.PlayerId),
                IsAbsent = afterLeaving.Room.AbsentPlayers.Any(absent => absent.Id == player.PlayerId)
            };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Leave_HostOfAGameInProgress_PassesTheHostToThePlayerWhoStays()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            RoomResult player = _manager.Join(host.Room.Code, PlayerName);
            _manager.StartGame(host.Room.Code, host.PlayerId);

            RoomResult afterLeaving = _manager.Leave(host.Room.Code, host.PlayerId);

            Assert.Equal(player.PlayerId, afterLeaving.Room.HostId);
        }

        [Fact]
        public void Leave_LastPlayerOfAGameInProgress_DeletesTheRoom()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            RoomResult player = _manager.Join(host.Room.Code, PlayerName);
            _manager.StartGame(host.Room.Code, host.PlayerId);
            _manager.Leave(host.Room.Code, player.PlayerId);
            _manager.Leave(host.Room.Code, host.PlayerId);

            RoomResult attempt = _manager.Join(host.Room.Code, PlayerName);

            Assert.Equal(RoomStatus.RoomNotFound, attempt.Status);
        }

        [Fact]
        public void Join_PlayerWhoLeftTheGame_ReturnsWithTheSameIdentifier()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            RoomResult player = _manager.Join(host.Room.Code, PlayerName);
            _manager.StartGame(host.Room.Code, host.PlayerId);
            _manager.Leave(host.Room.Code, player.PlayerId);

            var expected = new { Status = RoomStatus.Success, HasReturned = true, player.PlayerId };

            RoomResult returned = _manager.Join(host.Room.Code, PlayerName);

            var actual = new { returned.Status, returned.HasReturned, returned.PlayerId };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Join_PlayerWhoLeftTheGame_MovesThePlayerBackToThePlayers()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            RoomResult player = _manager.Join(host.Room.Code, PlayerName);
            _manager.StartGame(host.Room.Code, host.PlayerId);
            _manager.Leave(host.Room.Code, player.PlayerId);

            var expected = new { IsPlaying = true, IsAbsent = false };

            RoomResult returned = _manager.Join(host.Room.Code, PlayerName.ToUpperInvariant());

            var actual = new
            {
                IsPlaying = returned.Room.Players.Any(back => back.Id == player.PlayerId),
                IsAbsent = returned.Room.AbsentPlayers.Any(absent => absent.Id == player.PlayerId)
            };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Join_StrangerToAGameInProgress_ReturnsGameAlreadyStarted()
        {
            RoomResult host = CreateStartedRoom();

            var expected = new { Status = RoomStatus.GameAlreadyStarted, HasReturned = false };

            RoomResult stranger = _manager.Join(host.Room.Code, GuestName);

            var actual = new { stranger.Status, stranger.HasReturned };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Join_PlayerBeforeTheGameStarts_DoesNotReportAReturn()
        {
            RoomResult host = _manager.CreateRoom(HostName);

            RoomResult player = _manager.Join(host.Room.Code, PlayerName);

            Assert.False(player.HasReturned);
        }

        [Fact]
        public void FindPendingRoomCode_PlayerWhoLeftTheGame_ReturnsTheCodeOfTheRoom()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            RoomResult player = _manager.Join(host.Room.Code, PlayerName);
            _manager.StartGame(host.Room.Code, host.PlayerId);
            _manager.Leave(host.Room.Code, player.PlayerId);

            string code = _manager.FindPendingRoomCode(PlayerName);

            Assert.Equal(host.Room.Code, code);
        }

        [Fact]
        public void FindPendingRoomCode_PlayerWhoReturned_ReturnsEmpty()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            RoomResult player = _manager.Join(host.Room.Code, PlayerName);
            _manager.StartGame(host.Room.Code, host.PlayerId);
            _manager.Leave(host.Room.Code, player.PlayerId);
            _manager.Join(host.Room.Code, PlayerName);

            string code = _manager.FindPendingRoomCode(PlayerName);

            Assert.Equal(string.Empty, code);
        }

        [Fact]
        public void FindPendingRoomCode_PlayerWhoLeftBeforeTheGame_ReturnsEmpty()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            RoomResult player = _manager.Join(host.Room.Code, PlayerName);
            _manager.Leave(host.Room.Code, player.PlayerId);

            string code = _manager.FindPendingRoomCode(PlayerName);

            Assert.Equal(string.Empty, code);
        }

        [Fact]
        public void FindPendingRoomCode_BlankName_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => _manager.FindPendingRoomCode(" "));
        }

        [Fact]
        public void Leave_PlayerWithTheSameNameAsAnother_RemovesOnlyThatPlayer()
        {
            RoomResult first = _manager.CreateRoom(GuestName);
            RoomResult second = _manager.Join(first.Room.Code, GuestName);

            var expected = new { Status = RoomStatus.Success, RemainingId = first.PlayerId };

            RoomResult afterLeaving = _manager.Leave(first.Room.Code, second.PlayerId);

            var actual = new { afterLeaving.Status, RemainingId = afterLeaving.Room.Players.Single().Id };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Leave_HostWithOtherPlayersInside_PassesTheHostToTheNextPlayer()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            RoomResult player = _manager.Join(host.Room.Code, PlayerName);

            RoomResult afterLeaving = _manager.Leave(host.Room.Code, host.PlayerId);

            Assert.Equal(player.PlayerId, afterLeaving.Room.HostId);
        }

        [Fact]
        public void Leave_HostWithOtherPlayersInside_ReportsThatTheHostChanged()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            _manager.Join(host.Room.Code, PlayerName);

            RoomResult afterLeaving = _manager.Leave(host.Room.Code, host.PlayerId);

            Assert.True(afterLeaving.HostChanged);
        }

        [Fact]
        public void Leave_PlayerWhoIsNotTheHost_DoesNotReportAHostChange()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            RoomResult player = _manager.Join(host.Room.Code, PlayerName);

            RoomResult afterLeaving = _manager.Leave(host.Room.Code, player.PlayerId);

            Assert.False(afterLeaving.HostChanged);
        }

        [Fact]
        public void Leave_LastPlayer_LeavesTheRoomEmpty()
        {
            RoomResult host = _manager.CreateRoom(HostName);

            RoomResult afterLeaving = _manager.Leave(host.Room.Code, host.PlayerId);

            Assert.True(afterLeaving.Room.IsEmpty);
        }

        [Fact]
        public void Leave_LastPlayer_DeletesTheRoom()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            _manager.Leave(host.Room.Code, host.PlayerId);

            RoomResult afterDeleting = _manager.Join(host.Room.Code, PlayerName);

            Assert.Equal(RoomStatus.RoomNotFound, afterDeleting.Status);
        }

        [Fact]
        public void Leave_PlayerNameInsteadOfIdentifier_ReturnsRoomNotFound()
        {
            RoomResult host = _manager.CreateRoom(HostName);

            RoomResult afterLeaving = _manager.Leave(host.Room.Code, HostName);

            Assert.Equal(RoomStatus.RoomNotFound, afterLeaving.Status);
        }

        [Fact]
        public void Leave_PlayerNameInsteadOfIdentifier_KeepsTheHostInside()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            _manager.Leave(host.Room.Code, HostName);

            bool isFound = _manager.TryGetPlayer(host.Room.Code, host.PlayerId, out _);

            Assert.True(isFound);
        }

        [Fact]
        public void UpdateSettings_PlayerWithTheHostName_ReturnsNotHost()
        {
            RoomResult host = _manager.CreateRoom(GuestName);
            RoomResult sameName = _manager.Join(host.Room.Code, GuestName);

            RoomResult result = _manager.UpdateSettings(host.Room.Code, sameName.PlayerId, CreateSettings(TwoPlayers));

            Assert.Equal(RoomStatus.NotHost, result.Status);
        }

        [Fact]
        public void UpdateSettings_HostWithValidSettings_ChangesTheSettings()
        {
            RoomResult host = _manager.CreateRoom(HostName);

            var expected = new { Status = RoomStatus.Success, MaxPlayers = ThreePlayers };

            RoomResult result = _manager.UpdateSettings(host.Room.Code, host.PlayerId, CreateSettings(ThreePlayers));

            var actual = new { result.Status, result.Room.Settings.MaxPlayers };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void UpdateSettings_FewerPlacesThanPlayers_ReturnsInvalidSettings()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            _manager.Join(host.Room.Code, PlayerName);

            RoomResult result = _manager.UpdateSettings(host.Room.Code, host.PlayerId, CreateSettings(SingleSlot));

            Assert.Equal(RoomStatus.InvalidSettings, result.Status);
        }

        [Fact]
        public void StartGame_PlayerWithTheHostName_ReturnsNotHost()
        {
            RoomResult host = _manager.CreateRoom(GuestName);
            RoomResult sameName = _manager.Join(host.Room.Code, GuestName);

            RoomResult result = _manager.StartGame(host.Room.Code, sameName.PlayerId);

            Assert.Equal(RoomStatus.NotHost, result.Status);
        }

        [Fact]
        public void StartGame_HostAlone_ReturnsNotEnoughPlayers()
        {
            RoomResult host = _manager.CreateRoom(HostName);

            RoomResult result = _manager.StartGame(host.Room.Code, host.PlayerId);

            Assert.Equal(RoomStatus.NotEnoughPlayers, result.Status);
        }

        [Fact]
        public void StartGame_HostWithEnoughPlayers_StartsTheGameOnlyOnce()
        {
            RoomResult host = CreateStartedRoom();

            RoomResult secondStart = _manager.StartGame(host.Room.Code, host.PlayerId);

            Assert.Equal(RoomStatus.GameAlreadyStarted, secondStart.Status);
        }

        [Fact]
        public void TryGetPlayer_PlayerInside_ReturnsThePlayerWithItsName()
        {
            RoomResult host = _manager.CreateRoom(HostName);

            var expected = new { IsFound = true, DisplayName = HostName };

            bool isFound = _manager.TryGetPlayer(host.Room.Code, host.PlayerId, out RoomPlayer player);

            var actual = new { IsFound = isFound, player.DisplayName };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TryGetPlayer_UnknownIdentifier_ReturnsFalse()
        {
            RoomResult host = _manager.CreateRoom(HostName);

            var expected = new { IsFound = false, HasPlayer = false };

            bool isFound = _manager.TryGetPlayer(host.Room.Code, UnknownPlayerId, out RoomPlayer player);

            var actual = new { IsFound = isFound, HasPlayer = player != null };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TryGetPlayer_UnknownRoom_ReturnsFalse()
        {
            bool isFound = _manager.TryGetPlayer(UnknownRoomCode, UnknownPlayerId, out _);

            Assert.False(isFound);
        }

        [Fact]
        public void GetOpenRooms_NoRooms_ReturnsAnEmptyList()
        {
            List<RoomSummary> rooms = _manager.GetOpenRooms();

            Assert.Empty(rooms);
        }

        [Fact]
        public void GetOpenRooms_RoomWithPlayers_ShowsTheHostAndHowManyAreInside()
        {
            RoomResult host = CreatePublicRoom(HostName);
            _manager.Join(host.Room.Code, PlayerName);

            var expected = new
            {
                HostName,
                PlayerCount = TwoPlayers,
                MaxPlayers = FourPlayers,
                IsPrivate = false
            };

            RoomSummary summary = _manager.GetOpenRooms().Single();

            var actual = new { summary.HostName, summary.PlayerCount, summary.MaxPlayers, summary.IsPrivate };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetOpenRooms_PrivateRoom_ListsItWithoutRevealingItsCode()
        {
            RoomResult host = _manager.CreateRoom(HostName);

            var expected = new { IsPrivate = true, RevealsCode = false };

            RoomSummary summary = _manager.GetOpenRooms().Single();

            var actual = new { summary.IsPrivate, RevealsCode = summary.RoomId == host.Room.Code };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetOpenRooms_GameAlreadyStarted_DoesNotListTheRoom()
        {
            CreateStartedRoom();
            _manager.CreateRoom(PlayerName);

            List<RoomSummary> rooms = _manager.GetOpenRooms();

            Assert.Equal(PlayerName, rooms.Single().HostName);
        }

        [Fact]
        public void GetOpenRooms_SeveralRooms_OrdersThemByHostName()
        {
            _manager.CreateRoom(PlayerName);
            _manager.CreateRoom(HostName);

            string[] hosts = _manager.GetOpenRooms().Select(room => room.HostName).ToArray();

            Assert.Equal(new[] { HostName, PlayerName }, hosts);
        }

        [Fact]
        public void JoinListed_PublicRoomWithoutCode_PutsThePlayerInside()
        {
            CreatePublicRoom(HostName);
            string roomId = _manager.GetOpenRooms().Single().RoomId;

            var expected = new { Status = RoomStatus.Success, HasIdentifier = true, PlayerCount = TwoPlayers };

            RoomResult player = _manager.JoinListed(PlayerName, CreateListedRoom(roomId, string.Empty));

            var actual = new
            {
                player.Status,
                HasIdentifier = !string.IsNullOrEmpty(player.PlayerId),
                PlayerCount = player.Room.Players.Count
            };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void JoinListed_PrivateRoomWithTheRightCode_PutsThePlayerInside()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            string roomId = _manager.GetOpenRooms().Single().RoomId;

            Room room = CreateListedRoom(roomId, host.Room.Code.ToLowerInvariant());
            var expected = new { Status = RoomStatus.Success, host.Room.Code };

            RoomResult player = _manager.JoinListed(PlayerName, room);

            var actual = new { player.Status, player.Room.Code };
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(UnknownRoomCode)]
        [InlineData("")]
        [InlineData(null)]
        public void JoinListed_PrivateRoomWithAWrongCode_ReturnsInvalidAccessCode(string accessCode)
        {
            _manager.CreateRoom(HostName);
            string roomId = _manager.GetOpenRooms().Single().RoomId;

            var expected = new { Status = RoomStatus.InvalidAccessCode, PlayerId = string.Empty };

            RoomResult player = _manager.JoinListed(PlayerName, CreateListedRoom(roomId, accessCode));

            var actual = new { player.Status, player.PlayerId };
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(UnknownRoomCode)]
        [InlineData("")]
        [InlineData(null)]
        public void JoinListed_PrivateRoomWithAWrongCode_DoesNotAddThePlayer(string accessCode)
        {
            _manager.CreateRoom(HostName);
            string roomId = _manager.GetOpenRooms().Single().RoomId;
            _manager.JoinListed(PlayerName, CreateListedRoom(roomId, accessCode));

            int playerCount = _manager.GetOpenRooms().Single().PlayerCount;

            Assert.Equal(SingleSlot, playerCount);
        }

        [Fact]
        public void JoinListed_UnknownRoom_ReturnsRoomNotFound()
        {
            RoomResult player = _manager.JoinListed(PlayerName, CreateListedRoom(UnknownPlayerId, string.Empty));

            Assert.Equal(RoomStatus.RoomNotFound, player.Status);
        }

        [Fact]
        public void JoinListed_PublicRoomWithoutSpace_ReturnsRoomFull()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            _manager.UpdateSettings(host.Room.Code, host.PlayerId, CreateSettings(TwoPlayers, false));
            _manager.Join(host.Room.Code, PlayerName);
            string roomId = _manager.GetOpenRooms().Single().RoomId;

            RoomResult player = _manager.JoinListed(GuestName, CreateListedRoom(roomId, string.Empty));

            Assert.Equal(RoomStatus.RoomFull, player.Status);
        }

        [Fact]
        public void JoinListed_BlankName_ThrowsArgumentException()
        {
            CreatePublicRoom(HostName);
            string roomId = _manager.GetOpenRooms().Single().RoomId;

            Assert.Throws<ArgumentException>(() => _manager.JoinListed(" ", CreateListedRoom(roomId, string.Empty)));
        }

        [Fact]
        public void KickPlayer_HostExpelsAnotherPlayer_RemovesOnlyThatPlayer()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            RoomResult player = _manager.Join(host.Room.Code, PlayerName);

            var expected = new { Status = RoomStatus.Success, RemainingId = host.PlayerId };

            RoomResult afterKicking = _manager.KickPlayer(host.Room.Code, host.PlayerId, player.PlayerId);

            var actual = new { afterKicking.Status, RemainingId = afterKicking.Room.Players.Single().Id };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void KickPlayer_HostExpelsAnotherPlayer_ThePlayerIsNoLongerFound()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            RoomResult player = _manager.Join(host.Room.Code, PlayerName);
            _manager.KickPlayer(host.Room.Code, host.PlayerId, player.PlayerId);

            bool isFound = _manager.TryGetPlayer(host.Room.Code, player.PlayerId, out _);

            Assert.False(isFound);
        }

        [Fact]
        public void KickPlayer_PlayerWhoIsNotTheHost_ReturnsNotHost()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            RoomResult player = _manager.Join(host.Room.Code, PlayerName);

            RoomResult afterKicking = _manager.KickPlayer(host.Room.Code, player.PlayerId, host.PlayerId);

            Assert.Equal(RoomStatus.NotHost, afterKicking.Status);
        }

        [Fact]
        public void KickPlayer_PlayerWhoIsNotTheHost_KeepsTheHostInside()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            RoomResult player = _manager.Join(host.Room.Code, PlayerName);
            _manager.KickPlayer(host.Room.Code, player.PlayerId, host.PlayerId);

            bool isFound = _manager.TryGetPlayer(host.Room.Code, host.PlayerId, out _);

            Assert.True(isFound);
        }

        [Fact]
        public void KickPlayer_HostChoosesThemselves_ReturnsInvalidTarget()
        {
            RoomResult host = _manager.CreateRoom(HostName);

            RoomResult afterKicking = _manager.KickPlayer(host.Room.Code, host.PlayerId, host.PlayerId);

            Assert.Equal(RoomStatus.InvalidTarget, afterKicking.Status);
        }

        [Fact]
        public void KickPlayer_PlayerNotInTheRoom_ReturnsPlayerNotFound()
        {
            RoomResult host = _manager.CreateRoom(HostName);

            RoomResult afterKicking = _manager.KickPlayer(host.Room.Code, host.PlayerId, UnknownPlayerId);

            Assert.Equal(RoomStatus.PlayerNotFound, afterKicking.Status);
        }

        [Fact]
        public void KickPlayer_GameAlreadyStarted_RemovesThePlayer()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            RoomResult player = _manager.Join(host.Room.Code, PlayerName);
            _manager.StartGame(host.Room.Code, host.PlayerId);

            var expected = new { Status = RoomStatus.Success, IsPlaying = false };

            RoomResult afterKicking = _manager.KickPlayer(host.Room.Code, host.PlayerId, player.PlayerId);

            var actual = new
            {
                afterKicking.Status,
                IsPlaying = afterKicking.Room.Players.Any(remaining => remaining.Id == player.PlayerId)
            };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void KickPlayer_GameAlreadyStarted_DoesNotLetThePlayerReturn()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            RoomResult player = _manager.Join(host.Room.Code, PlayerName);
            _manager.StartGame(host.Room.Code, host.PlayerId);
            _manager.KickPlayer(host.Room.Code, host.PlayerId, player.PlayerId);

            RoomResult attempt = _manager.Join(host.Room.Code, PlayerName);

            Assert.Equal(RoomStatus.GameAlreadyStarted, attempt.Status);
        }

        [Fact]
        public void KickPlayer_UnknownRoom_ReturnsRoomNotFound()
        {
            RoomResult afterKicking = _manager.KickPlayer(UnknownRoomCode, UnknownPlayerId, UnknownPlayerId);

            Assert.Equal(RoomStatus.RoomNotFound, afterKicking.Status);
        }

        [Fact]
        public void TransferHost_HostChoosesAnotherPlayer_MakesThatPlayerTheHost()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            RoomResult player = _manager.Join(host.Room.Code, PlayerName);

            var expected = new { Status = RoomStatus.Success, HostId = player.PlayerId, PlayerCount = TwoPlayers };

            RoomResult afterTransfer = _manager.TransferHost(host.Room.Code, host.PlayerId, player.PlayerId);

            var actual = new
            {
                afterTransfer.Status,
                afterTransfer.Room.HostId,
                PlayerCount = afterTransfer.Room.Players.Count
            };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TransferHost_FormerHost_CannotChangeTheSettingsAnymore()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            RoomResult player = _manager.Join(host.Room.Code, PlayerName);
            _manager.TransferHost(host.Room.Code, host.PlayerId, player.PlayerId);

            RoomResult afterChange = _manager.UpdateSettings(
                host.Room.Code,
                host.PlayerId,
                CreateSettings(FourPlayers));

            Assert.Equal(RoomStatus.NotHost, afterChange.Status);
        }

        [Fact]
        public void TransferHost_PlayerWhoIsNotTheHost_ReturnsNotHost()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            RoomResult player = _manager.Join(host.Room.Code, PlayerName);

            RoomResult afterTransfer = _manager.TransferHost(host.Room.Code, player.PlayerId, player.PlayerId);

            Assert.Equal(RoomStatus.NotHost, afterTransfer.Status);
        }

        [Fact]
        public void TransferHost_PlayerWhoIsNotTheHost_KeepsTheSameHost()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            RoomResult player = _manager.Join(host.Room.Code, PlayerName);
            _manager.TransferHost(host.Room.Code, player.PlayerId, player.PlayerId);

            RoomResult newcomer = _manager.Join(host.Room.Code, GuestName);

            Assert.Equal(host.PlayerId, newcomer.Room.HostId);
        }

        [Fact]
        public void TransferHost_PlayerNotInTheRoom_ReturnsPlayerNotFound()
        {
            RoomResult host = _manager.CreateRoom(HostName);

            RoomResult afterTransfer = _manager.TransferHost(host.Room.Code, host.PlayerId, UnknownPlayerId);

            Assert.Equal(RoomStatus.PlayerNotFound, afterTransfer.Status);
        }

        [Fact]
        public void TransferHost_HostChoosesThemselves_ReturnsInvalidTarget()
        {
            RoomResult host = _manager.CreateRoom(HostName);

            RoomResult afterTransfer = _manager.TransferHost(host.Room.Code, host.PlayerId, host.PlayerId);

            Assert.Equal(RoomStatus.InvalidTarget, afterTransfer.Status);
        }

        [Fact]
        public void CreateRoom_HostWithAvatar_ShowsTheAvatarOfTheHost()
        {
            _avatars.SetAvatar(HostName, HostAvatarId);

            RoomResult result = _manager.CreateRoom(HostName);

            Assert.Equal(HostAvatarId, result.Room.Players.Single().AvatarId);
        }

        [Fact]
        public void CreateRoom_HostNotRegistered_ShowsTheDefaultAvatar()
        {
            RoomResult result = _manager.CreateRoom(GuestName);

            Assert.Equal(AvatarCatalog.DefaultAvatarId, result.Room.Players.Single().AvatarId);
        }

        [Fact]
        public void Join_PlayerWithAvatar_KeepsTheAvatarOfEachPlayer()
        {
            _avatars.SetAvatar(HostName, HostAvatarId);
            _avatars.SetAvatar(PlayerName, PlayerAvatarId);
            RoomResult host = _manager.CreateRoom(HostName);

            RoomResult player = _manager.Join(host.Room.Code, PlayerName);

            int[] avatarIds = player.Room.Players.Select(member => member.AvatarId).ToArray();
            Assert.Equal(new[] { HostAvatarId, PlayerAvatarId }, avatarIds);
        }

        [Fact]
        public void Constructor_NullAvatarLookup_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RoomManager(null));
        }

        private static RoomSettings CreateSettings(int maxPlayers)
        {
            return CreateSettings(maxPlayers, true);
        }

        private static RoomSettings CreateSettings(int maxPlayers, bool isPrivate)
        {
            var settings = new RoomSettings
            {
                MaxPlayers = maxPlayers,
                IsPrivate = isPrivate,
                Speed = GameSpeed.Normal
            };

            return settings;
        }

        private static Room CreateListedRoom(string roomId, string accessCode)
        {
            var room = new Room
            {
                RoomId = roomId,
                AccessCode = accessCode
            };

            return room;
        }

        private RoomResult CreatePublicRoom(string hostName)
        {
            RoomResult host = _manager.CreateRoom(hostName);

            _manager.UpdateSettings(host.Room.Code, host.PlayerId, CreateSettings(FourPlayers, false));

            return host;
        }

        private RoomResult CreateStartedRoom()
        {
            RoomResult host = _manager.CreateRoom(HostName);

            _manager.Join(host.Room.Code, PlayerName);
            _manager.StartGame(host.Room.Code, host.PlayerId);

            return host;
        }
    }
}
