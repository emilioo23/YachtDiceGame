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

        private readonly List<RoomListItem> _rooms = new List<RoomListItem>
        {
            CreateRoom("1", FirstHost, false),
            CreateRoom("2", SecondHost, true),
            CreateRoom("3", ThirdHost, false)
        };

        [Fact]
        public void Apply_AllRoomsAndNoSearch_KeepsEveryRoomInTheSameOrder()
        {
            RoomListItem[] expected =
            {
                CreateRoom("1", FirstHost, false),
                CreateRoom("2", SecondHost, true),
                CreateRoom("3", ThirdHost, false)
            };

            List<RoomListItem> result = ApplyFilter(RoomPrivacyFilter.All, NoSearch);

            Assert.Equivalent(expected, result, strict: true);
        }

        [Fact]
        public void Apply_OnlyPublicRooms_HidesThePrivateOnes()
        {
            RoomListItem[] expected =
            {
                CreateRoom("1", FirstHost, false),
                CreateRoom("3", ThirdHost, false)
            };

            List<RoomListItem> result = ApplyFilter(RoomPrivacyFilter.Public, NoSearch);

            Assert.Equivalent(expected, result, strict: true);
        }

        [Fact]
        public void Apply_OnlyPrivateRooms_HidesThePublicOnes()
        {
            RoomListItem[] expected =
            {
                CreateRoom("2", SecondHost, true)
            };

            List<RoomListItem> result = ApplyFilter(RoomPrivacyFilter.Private, NoSearch);

            Assert.Equivalent(expected, result, strict: true);
        }

        [Fact]
        public void Apply_TextInTheNameOfTheHost_KeepsOnlyThoseRooms()
        {
            RoomListItem[] expected =
            {
                CreateRoom("2", SecondHost, true)
            };

            List<RoomListItem> result = ApplyFilter(RoomPrivacyFilter.All, "gab");

            Assert.Equivalent(expected, result, strict: true);
        }

        [Fact]
        public void Apply_TextWithDifferentCase_StillMatches()
        {
            RoomListItem[] expected =
            {
                CreateRoom("3", ThirdHost, false)
            };

            List<RoomListItem> result = ApplyFilter(RoomPrivacyFilter.All, "CARLOS");

            Assert.Equivalent(expected, result, strict: true);
        }

        [Fact]
        public void Apply_TextWithSpacesAround_IgnoresTheSpaces()
        {
            RoomListItem[] expected =
            {
                CreateRoom("1", FirstHost, false)
            };

            List<RoomListItem> result = ApplyFilter(RoomPrivacyFilter.All, "  emilio  ");

            Assert.Equivalent(expected, result, strict: true);
        }

        [Fact]
        public void Apply_PrivacyAndTextTogether_KeepsOnlyRoomsThatMatchBoth()
        {
            List<RoomListItem> result = ApplyFilter(RoomPrivacyFilter.Private, "emilio");

            Assert.Empty(result);
        }

        [Fact]
        public void Apply_TextThatNobodyHas_ReturnsAnEmptyList()
        {
            List<RoomListItem> result = ApplyFilter(RoomPrivacyFilter.All, "zzz");

            Assert.Empty(result);
        }

        [Fact]
        public void Apply_NullSearch_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RoomListItemFilter.Apply(_rooms, null));
        }

        [Fact]
        public void Apply_NoRooms_ReturnsAnEmptyList()
        {
            List<RoomListItem> result =
                RoomListItemFilter.Apply(new List<RoomListItem>(), new RoomSearch(RoomPrivacyFilter.All, NoSearch));

            Assert.Empty(result);
        }

        private static RoomListItem CreateRoom(string roomId, string hostName, bool isPrivate)
        {
            return new RoomListItem { RoomId = roomId, HostName = hostName, IsPrivate = isPrivate };
        }

        private List<RoomListItem> ApplyFilter(RoomPrivacyFilter privacy, string searchText)
        {
            return RoomListItemFilter.Apply(_rooms, new RoomSearch(privacy, searchText));
        }
    }
}
