using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using YachtDice.Client.Logic.Rooms;

namespace YachtDice.Client.Tests.Rooms
{
    /// <summary>
    /// Rules of the list of rooms.
    /// </summary>
    public class RoomListRulesTests
    {
        private const string FirstId = "id-1";
        private const string SecondId = "id-2";
        private const string ThirdId = "id-3";
        private const string FirstName = "Emilio";
        private const string SecondName = "Gabriel";
        private const string ThirdName = "Ana";
        private const string NoSearch = "";
        private const string FirstHost = "Emilio";
        private const string SecondHost = "Gabriel";
        private const string ThirdHost = "Carlos";
        private const string RoomName = "Los del salon 4";
        private const string RoomNameSearch = "salon";
        private const string FirstRoomId = "1";
        private const string SecondRoomId = "2";
        private const string ThirdRoomId = "3";
        private const string FourthRoomId = "4";
        private const string PartialHostSearch = "gab";
        private const string UppercaseHostSearch = "CARLOS";
        private const string SpacedHostSearch = "  emilio  ";
        private const string ExactHostSearch = "emilio";
        private const string NoMatchSearch = "zzz";
        private const string TypedCode = "ABC123";
        private const string NoCode = "";
        private const int OnePlayer = 1;
        private const int TwoPlayers = 2;
        private readonly List<RoomListItem> _rooms = new List<RoomListItem>
        {
            CreateRoom(FirstRoomId, FirstHost, false),
            CreateRoom(SecondRoomId, SecondHost, true),
            CreateRoom(ThirdRoomId, ThirdHost, false)
        };

        [Fact]
        public void FindDeparted_SamePlayers_ReturnsNobody()
        {
            List<GameParticipant> players = CreatePlayers(FirstId, SecondId);

            Assert.Empty(RoomListRules.FindDeparted(players, players));
        }

        [Fact]
        public void FindDeparted_PlayerMissingNow_ReturnsThatPlayer()
        {
            List<GameParticipant> before = CreatePlayers(FirstId, SecondId);
            List<GameParticipant> after = CreatePlayers(FirstId);

            GameParticipant[] departedExpected = { new GameParticipant(SecondId, SecondName) };

            List<GameParticipant> departed = RoomListRules.FindDeparted(before, after);

            Assert.Equivalent(departedExpected, departed, strict: true);
        }

        [Fact]
        public void FindDeparted_TwoPlayersMissing_ReturnsBothInOrder()
        {
            List<GameParticipant> before = CreatePlayers(FirstId, SecondId, ThirdId);
            List<GameParticipant> after = CreatePlayers(FirstId);

            GameParticipant[] departedExpected =
            {
                new GameParticipant(SecondId, SecondName),
                new GameParticipant(ThirdId, ThirdName)
            };

            List<GameParticipant> departed = RoomListRules.FindDeparted(before, after);

            Assert.Equivalent(departedExpected, departed, strict: true);
        }

        [Fact]
        public void FindDeparted_PlayerArrived_ReturnsNobody()
        {
            List<GameParticipant> before = CreatePlayers(FirstId);
            List<GameParticipant> after = CreatePlayers(FirstId, SecondId);

            Assert.Empty(RoomListRules.FindDeparted(before, after));
        }

        [Fact]
        public void FindArrived_PlayerNewInTheRoom_ReturnsThatPlayer()
        {
            List<GameParticipant> before = CreatePlayers(FirstId);
            List<GameParticipant> after = CreatePlayers(FirstId, SecondId);

            GameParticipant[] arrivedExpected = { new GameParticipant(SecondId, SecondName) };

            List<GameParticipant> arrived = RoomListRules.FindArrived(before, after);

            Assert.Equivalent(arrivedExpected, arrived, strict: true);
        }

        [Fact]
        public void FindArrived_PlayerLeft_ReturnsNobody()
        {
            List<GameParticipant> before = CreatePlayers(FirstId, SecondId);
            List<GameParticipant> after = CreatePlayers(FirstId);

            Assert.Empty(RoomListRules.FindArrived(before, after));
        }

        [Fact]
        public void FindArrived_NobodyBefore_ReturnsEveryone()
        {
            List<GameParticipant> after = CreatePlayers(FirstId, SecondId);

            GameParticipant[] arrivedExpected =
            {
                new GameParticipant(FirstId, FirstName),
                new GameParticipant(SecondId, SecondName)
            };

            List<GameParticipant> arrived = RoomListRules.FindArrived(new List<GameParticipant>(), after);

            Assert.Equivalent(arrivedExpected, arrived, strict: true);
        }

        [Fact]
        public void FindDeparted_SameNameDifferentIdentifier_CountsAsDifferentPlayers()
        {
            var before = new List<GameParticipant> { new GameParticipant(FirstId, FirstName) };
            var after = new List<GameParticipant> { new GameParticipant(SecondId, FirstName) };

            GameParticipant[] departedExpected = { new GameParticipant(FirstId, FirstName) };

            List<GameParticipant> departed = RoomListRules.FindDeparted(before, after);

            Assert.Equivalent(departedExpected, departed, strict: true);
        }

        [Fact]
        public void Apply_AllRoomsAndNoSearch_KeepsEveryRoomInTheSameOrder()
        {
            RoomListItem[] filteredRoomsExpected =
            {
                CreateRoom(FirstRoomId, FirstHost, false),
                CreateRoom(SecondRoomId, SecondHost, true),
                CreateRoom(ThirdRoomId, ThirdHost, false)
            };

            List<RoomListItem> filteredRooms = ApplyFilter(RoomPrivacyFilter.All, NoSearch);

            Assert.Equivalent(filteredRoomsExpected, filteredRooms, strict: true);
        }

        [Fact]
        public void Apply_OnlyPublicRooms_HidesThePrivateOnes()
        {
            RoomListItem[] filteredRoomsExpected =
            {
                CreateRoom(FirstRoomId, FirstHost, false),
                CreateRoom(ThirdRoomId, ThirdHost, false)
            };

            List<RoomListItem> filteredRooms = ApplyFilter(RoomPrivacyFilter.Public, NoSearch);

            Assert.Equivalent(filteredRoomsExpected, filteredRooms, strict: true);
        }

        [Fact]
        public void Apply_OnlyPrivateRooms_HidesThePublicOnes()
        {
            RoomListItem[] filteredRoomsExpected =
            {
                CreateRoom(SecondRoomId, SecondHost, true)
            };

            List<RoomListItem> filteredRooms = ApplyFilter(RoomPrivacyFilter.Private, NoSearch);

            Assert.Equivalent(filteredRoomsExpected, filteredRooms, strict: true);
        }

        [Fact]
        public void Apply_TextInTheNameOfTheHost_KeepsOnlyThoseRooms()
        {
            RoomListItem[] filteredRoomsExpected =
            {
                CreateRoom(SecondRoomId, SecondHost, true)
            };

            List<RoomListItem> filteredRooms = ApplyFilter(RoomPrivacyFilter.All, PartialHostSearch);

            Assert.Equivalent(filteredRoomsExpected, filteredRooms, strict: true);
        }

        [Fact]
        public void Apply_TextInTheNameOfTheRoom_KeepsThatRoom()
        {
            RoomListItem namedRoom = CreateRoom(FourthRoomId, ThirdHost, false);
            namedRoom.Title = RoomName;
            var rooms = new List<RoomListItem> { namedRoom };

            List<RoomListItem> filteredRooms =
                RoomListRules.Apply(rooms, RoomPrivacyFilter.All, RoomNameSearch);

            Assert.Single(filteredRooms);
        }

        [Fact]
        public void Apply_TextWithDifferentCase_StillMatches()
        {
            RoomListItem[] filteredRoomsExpected =
            {
                CreateRoom(ThirdRoomId, ThirdHost, false)
            };

            List<RoomListItem> filteredRooms = ApplyFilter(RoomPrivacyFilter.All, UppercaseHostSearch);

            Assert.Equivalent(filteredRoomsExpected, filteredRooms, strict: true);
        }

        [Fact]
        public void Apply_TextWithSpacesAround_IgnoresTheSpaces()
        {
            RoomListItem[] filteredRoomsExpected =
            {
                CreateRoom(FirstRoomId, FirstHost, false)
            };

            List<RoomListItem> filteredRooms = ApplyFilter(RoomPrivacyFilter.All, SpacedHostSearch);

            Assert.Equivalent(filteredRoomsExpected, filteredRooms, strict: true);
        }

        [Fact]
        public void Apply_PrivateFilterWithTextOfAPublicRoomHost_ReturnsAnEmptyList()
        {
            List<RoomListItem> filteredRooms = ApplyFilter(RoomPrivacyFilter.Private, ExactHostSearch);

            Assert.Empty(filteredRooms);
        }

        [Fact]
        public void Apply_TextThatNobodyHas_ReturnsAnEmptyList()
        {
            List<RoomListItem> filteredRooms = ApplyFilter(RoomPrivacyFilter.All, NoMatchSearch);

            Assert.Empty(filteredRooms);
        }

        [Fact]
        public void Apply_NullSearchText_KeepsEveryRoom()
        {
            List<RoomListItem> filteredRoomsResult = RoomListRules.Apply(_rooms, RoomPrivacyFilter.All, null);

            Assert.Equal(_rooms, filteredRoomsResult);
        }

        [Fact]
        public void Apply_NoRooms_ReturnsAnEmptyList()
        {
            List<RoomListItem> filteredRooms =
                RoomListRules.Apply(new List<RoomListItem>(), RoomPrivacyFilter.All, NoSearch);

            Assert.Empty(filteredRooms);
        }

        [Fact]
        public void HasSameContent_SameRooms_ReturnsTrue()
        {
            var oldRooms = new List<RoomListItem>
            {
                CreateRoom(FirstRoomId, OnePlayer),
                CreateRoom(SecondRoomId, TwoPlayers)
            };
            var newRooms = new List<RoomListItem>
            {
                CreateRoom(FirstRoomId, OnePlayer),
                CreateRoom(SecondRoomId, TwoPlayers)
            };

            Assert.True(RoomListRules.HasSameContent(oldRooms, newRooms));
        }

        [Fact]
        public void HasSameContent_PlayerTypedACodeInAnOldRow_IgnoresTheCode()
        {
            var oldRooms = new List<RoomListItem> { CreateRoom(FirstRoomId, OnePlayer) };
            var newRooms = new List<RoomListItem> { CreateRoom(FirstRoomId, OnePlayer) };
            oldRooms[0].AccessCode = TypedCode;

            Assert.True(RoomListRules.HasSameContent(oldRooms, newRooms));
        }

        [Fact]
        public void HasSameContent_ARoomChangedItsPlayers_ReturnsFalse()
        {
            var oldRooms = new List<RoomListItem> { CreateRoom(FirstRoomId, OnePlayer) };
            var newRooms = new List<RoomListItem> { CreateRoom(FirstRoomId, TwoPlayers) };

            Assert.False(RoomListRules.HasSameContent(oldRooms, newRooms));
        }

        [Fact]
        public void HasSameContent_DifferentNumberOfRooms_ReturnsFalse()
        {
            var oldRooms = new List<RoomListItem> { CreateRoom(FirstRoomId, OnePlayer) };
            var newRooms = new List<RoomListItem>
            {
                CreateRoom(FirstRoomId, OnePlayer),
                CreateRoom(SecondRoomId, OnePlayer)
            };

            Assert.False(RoomListRules.HasSameContent(oldRooms, newRooms));
        }

        [Fact]
        public void HasSameContent_SameRoomsInAnotherOrder_ReturnsFalse()
        {
            var oldRooms = new List<RoomListItem>
            {
                CreateRoom(FirstRoomId, OnePlayer),
                CreateRoom(SecondRoomId, OnePlayer)
            };
            var newRooms = new List<RoomListItem>
            {
                CreateRoom(SecondRoomId, OnePlayer),
                CreateRoom(FirstRoomId, OnePlayer)
            };

            Assert.False(RoomListRules.HasSameContent(oldRooms, newRooms));
        }

        [Fact]
        public void HasSameContent_BothListsEmpty_ReturnsTrue()
        {
            Assert.True(RoomListRules.HasSameContent(new List<RoomListItem>(), new List<RoomListItem>()));
        }

        [Fact]
        public void KeepTypedCodes_RoomStillInTheList_CopiesTheTypedCode()
        {
            var oldRooms = new List<RoomListItem> { CreateRoom(FirstRoomId, OnePlayer) };
            var newRooms = new List<RoomListItem> { CreateRoom(FirstRoomId, TwoPlayers) };
            oldRooms[0].AccessCode = TypedCode;

            RoomListRules.KeepTypedCodes(oldRooms, newRooms);

            Assert.Equal(TypedCode, newRooms[0].AccessCode);
        }

        [Fact]
        public void KeepTypedCodes_NewRoomInTheList_LeavesItsCodeEmpty()
        {
            var oldRooms = new List<RoomListItem> { CreateRoom(FirstRoomId, OnePlayer) };
            var newRooms = new List<RoomListItem> { CreateRoom(SecondRoomId, OnePlayer) };
            oldRooms[0].AccessCode = TypedCode;

            RoomListRules.KeepTypedCodes(oldRooms, newRooms);

            Assert.Equal(NoCode, newRooms[0].AccessCode);
        }

        private static List<GameParticipant> CreatePlayers(params string[] ids)
        {
            var names = new Dictionary<string, string>
            {
                { FirstId, FirstName },
                { SecondId, SecondName },
                { ThirdId, ThirdName }
            };

            return ids.Select(id => new GameParticipant(id, names[id])).ToList();
        }

        private static RoomListItem CreateRoom(string roomId, string hostName, bool isPrivate)
        {
            return new RoomListItem { RoomId = roomId, HostName = hostName, IsPrivate = isPrivate };
        }

        private List<RoomListItem> ApplyFilter(RoomPrivacyFilter privacy, string searchText)
        {
            return RoomListRules.Apply(_rooms, privacy, searchText);
        }

        private static RoomListItem CreateRoom(string roomId, int playerCount)
        {
            return new RoomListItem
            {
                RoomId = roomId,
                Title = roomId,
                Details = playerCount.ToString(),
                PrivacyText = NoCode,
                HasSpace = true
            };
        }
    }
}
