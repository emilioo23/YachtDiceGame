using System;
using System.Collections.Generic;
using Xunit;
using YachtDice.Client.Logic.Rooms;

namespace YachtDice.Client.Tests.Rooms
{
    /// <summary>
    /// Rules to choose which rooms of the list the player sees.
    /// </summary>
    public class RoomListItemFilterTests
    {
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

        private readonly List<RoomListItem> _rooms = new List<RoomListItem>
        {
            CreateRoom(FirstRoomId, FirstHost, false),
            CreateRoom(SecondRoomId, SecondHost, true),
            CreateRoom(ThirdRoomId, ThirdHost, false)
        };

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
                RoomListItemFilter.Apply(rooms, RoomPrivacyFilter.All, RoomNameSearch);

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
            List<RoomListItem> roomsExpected = _rooms;

            List<RoomListItem> roomsResult = RoomListItemFilter.Apply(_rooms, RoomPrivacyFilter.All, null);

            Assert.Equal(roomsExpected, roomsResult);
        }

        [Fact]
        public void Apply_NoRooms_ReturnsAnEmptyList()
        {
            List<RoomListItem> filteredRooms =
                RoomListItemFilter.Apply(new List<RoomListItem>(), RoomPrivacyFilter.All, NoSearch);

            Assert.Empty(filteredRooms);
        }

        private static RoomListItem CreateRoom(string roomId, string hostName, bool isPrivate)
        {
            return new RoomListItem { RoomId = roomId, HostName = hostName, IsPrivate = isPrivate };
        }

        private List<RoomListItem> ApplyFilter(RoomPrivacyFilter privacy, string searchText)
        {
            return RoomListItemFilter.Apply(_rooms, privacy, searchText);
        }
    }
}
