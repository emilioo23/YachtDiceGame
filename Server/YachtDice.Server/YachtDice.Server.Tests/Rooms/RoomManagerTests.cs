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
            RoomResult result = _manager.CreateRoom(HostName);

            Assert.Equal(RoomStatus.Success, result.Status);
            Assert.False(string.IsNullOrEmpty(result.PlayerId));
            Assert.Equal(result.PlayerId, result.Room.HostId);
            Assert.Equal(HostName, result.Room.Players.Single().DisplayName);
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

            RoomResult player = _manager.Join(host.Room.Code, PlayerName);

            Assert.Equal(RoomStatus.Success, player.Status);
            Assert.Equal(PlayerName, player.Room.Players.Last().DisplayName);
            Assert.NotEqual(host.PlayerId, player.PlayerId);
        }

        [Fact]
        public void Join_NameUsedByAnotherPlayer_AddsANumberToTheName()
        {
            RoomResult host = _manager.CreateRoom(GuestName);

            RoomResult second = _manager.Join(host.Room.Code, GuestName);

            Assert.Equal(RoomStatus.Success, second.Status);
            Assert.Equal(SecondGuestName, second.Room.Players.Last().DisplayName);
            Assert.NotEqual(host.PlayerId, second.PlayerId);
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
            RoomResult player = _manager.Join(UnknownRoomCode, PlayerName);

            Assert.Equal(RoomStatus.RoomNotFound, player.Status);
            Assert.Equal(string.Empty, player.PlayerId);
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
        public void Leave_PlayerWithTheSameNameAsAnother_RemovesOnlyThatPlayer()
        {
            RoomResult first = _manager.CreateRoom(GuestName);
            RoomResult second = _manager.Join(first.Room.Code, GuestName);

            RoomResult afterLeaving = _manager.Leave(first.Room.Code, second.PlayerId);

            Assert.Equal(RoomStatus.Success, afterLeaving.Status);
            Assert.Equal(first.PlayerId, afterLeaving.Room.Players.Single().Id);
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
        public void Leave_LastPlayer_DeletesTheRoom()
        {
            RoomResult host = _manager.CreateRoom(HostName);

            RoomResult afterLeaving = _manager.Leave(host.Room.Code, host.PlayerId);
            RoomResult afterDeleting = _manager.Join(host.Room.Code, PlayerName);

            Assert.True(afterLeaving.Room.IsEmpty);
            Assert.Equal(RoomStatus.RoomNotFound, afterDeleting.Status);
        }

        [Fact]
        public void Leave_PlayerNameInsteadOfIdentifier_ReturnsRoomNotFound()
        {
            RoomResult host = _manager.CreateRoom(HostName);

            RoomResult afterLeaving = _manager.Leave(host.Room.Code, HostName);

            Assert.Equal(RoomStatus.RoomNotFound, afterLeaving.Status);
            Assert.True(_manager.TryGetPlayer(host.Room.Code, host.PlayerId, out _));
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

            RoomResult result = _manager.UpdateSettings(host.Room.Code, host.PlayerId, CreateSettings(ThreePlayers));

            Assert.Equal(RoomStatus.Success, result.Status);
            Assert.Equal(ThreePlayers, result.Room.Settings.MaxPlayers);
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

            bool isFound = _manager.TryGetPlayer(host.Room.Code, host.PlayerId, out RoomPlayer player);

            Assert.True(isFound);
            Assert.Equal(HostName, player.DisplayName);
        }

        [Fact]
        public void TryGetPlayer_UnknownIdentifier_ReturnsFalse()
        {
            RoomResult host = _manager.CreateRoom(HostName);

            bool isFound = _manager.TryGetPlayer(host.Room.Code, UnknownPlayerId, out RoomPlayer player);

            Assert.False(isFound);
            Assert.Null(player);
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

            RoomSummary summary = _manager.GetOpenRooms().Single();

            Assert.Equal(HostName, summary.HostName);
            Assert.Equal(TwoPlayers, summary.PlayerCount);
            Assert.Equal(FourPlayers, summary.MaxPlayers);
            Assert.False(summary.IsPrivate);
        }

        [Fact]
        public void GetOpenRooms_PrivateRoom_ListsItWithoutRevealingItsCode()
        {
            RoomResult host = _manager.CreateRoom(HostName);

            RoomSummary summary = _manager.GetOpenRooms().Single();

            Assert.True(summary.IsPrivate);
            Assert.NotEqual(host.Room.Code, summary.RoomId);
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

            RoomResult player = _manager.JoinListed(roomId, PlayerName, string.Empty);

            Assert.Equal(RoomStatus.Success, player.Status);
            Assert.False(string.IsNullOrEmpty(player.PlayerId));
            Assert.Equal(TwoPlayers, player.Room.Players.Count);
        }

        [Fact]
        public void JoinListed_PrivateRoomWithTheRightCode_PutsThePlayerInside()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            string roomId = _manager.GetOpenRooms().Single().RoomId;

            RoomResult player = _manager.JoinListed(roomId, PlayerName, host.Room.Code.ToLowerInvariant());

            Assert.Equal(RoomStatus.Success, player.Status);
            Assert.Equal(host.Room.Code, player.Room.Code);
        }

        [Theory]
        [InlineData(UnknownRoomCode)]
        [InlineData("")]
        [InlineData(null)]
        public void JoinListed_PrivateRoomWithAWrongCode_ReturnsInvalidAccessCode(string accessCode)
        {
            _manager.CreateRoom(HostName);
            string roomId = _manager.GetOpenRooms().Single().RoomId;

            RoomResult player = _manager.JoinListed(roomId, PlayerName, accessCode);

            Assert.Equal(RoomStatus.InvalidAccessCode, player.Status);
            Assert.Empty(player.PlayerId);
            Assert.Equal(SingleSlot, _manager.GetOpenRooms().Single().PlayerCount);
        }

        [Fact]
        public void JoinListed_UnknownRoom_ReturnsRoomNotFound()
        {
            RoomResult player = _manager.JoinListed(UnknownPlayerId, PlayerName, string.Empty);

            Assert.Equal(RoomStatus.RoomNotFound, player.Status);
        }

        [Fact]
        public void JoinListed_PublicRoomWithoutSpace_ReturnsRoomFull()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            _manager.UpdateSettings(host.Room.Code, host.PlayerId, CreateSettings(TwoPlayers, false));
            _manager.Join(host.Room.Code, PlayerName);
            string roomId = _manager.GetOpenRooms().Single().RoomId;

            RoomResult player = _manager.JoinListed(roomId, GuestName, string.Empty);

            Assert.Equal(RoomStatus.RoomFull, player.Status);
        }

        [Fact]
        public void JoinListed_BlankName_ThrowsArgumentException()
        {
            CreatePublicRoom(HostName);
            string roomId = _manager.GetOpenRooms().Single().RoomId;

            Assert.Throws<ArgumentException>(() => _manager.JoinListed(roomId, " ", string.Empty));
        }

        [Fact]
        public void KickPlayer_HostExpelsAnotherPlayer_RemovesOnlyThatPlayer()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            RoomResult player = _manager.Join(host.Room.Code, PlayerName);

            RoomResult afterKicking = _manager.KickPlayer(host.Room.Code, host.PlayerId, player.PlayerId);

            Assert.Equal(RoomStatus.Success, afterKicking.Status);
            Assert.Equal(host.PlayerId, afterKicking.Room.Players.Single().Id);
            Assert.False(_manager.TryGetPlayer(host.Room.Code, player.PlayerId, out _));
        }

        [Fact]
        public void KickPlayer_PlayerWhoIsNotTheHost_ReturnsNotHostAndKeepsEveryone()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            RoomResult player = _manager.Join(host.Room.Code, PlayerName);

            RoomResult afterKicking = _manager.KickPlayer(host.Room.Code, player.PlayerId, host.PlayerId);

            Assert.Equal(RoomStatus.NotHost, afterKicking.Status);
            Assert.True(_manager.TryGetPlayer(host.Room.Code, host.PlayerId, out _));
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
        public void KickPlayer_GameAlreadyStarted_ReturnsGameAlreadyStarted()
        {
            RoomResult host = _manager.CreateRoom(HostName);
            RoomResult player = _manager.Join(host.Room.Code, PlayerName);
            _manager.StartGame(host.Room.Code, host.PlayerId);

            RoomResult afterKicking = _manager.KickPlayer(host.Room.Code, host.PlayerId, player.PlayerId);

            Assert.Equal(RoomStatus.GameAlreadyStarted, afterKicking.Status);
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

            RoomResult afterTransfer = _manager.TransferHost(host.Room.Code, host.PlayerId, player.PlayerId);

            Assert.Equal(RoomStatus.Success, afterTransfer.Status);
            Assert.Equal(player.PlayerId, afterTransfer.Room.HostId);
            Assert.Equal(TwoPlayers, afterTransfer.Room.Players.Count);
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
            Assert.Equal(host.PlayerId, _manager.Join(host.Room.Code, GuestName).Room.HostId);
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

            Assert.Equal(HostAvatarId, player.Room.Players.First().AvatarId);
            Assert.Equal(PlayerAvatarId, player.Room.Players.Last().AvatarId);
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
