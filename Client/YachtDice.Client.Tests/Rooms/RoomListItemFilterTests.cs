using System.Collections.Generic;
using System.Linq;
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
            List<RoomListItem> result = RoomListItemFilter.Apply(_rooms, RoomPrivacyFilter.All, NoSearch);

            Assert.Equal(new[] { "1", "2", "3" }, result.Select(room => room.RoomId));
        }

        [Fact]
        public void Apply_OnlyPublicRooms_HidesThePrivateOnes()
        {
            List<RoomListItem> result = RoomListItemFilter.Apply(_rooms, RoomPrivacyFilter.Public, NoSearch);

            Assert.Equal(new[] { "1", "3" }, result.Select(room => room.RoomId));
        }

        [Fact]
        public void Apply_OnlyPrivateRooms_HidesThePublicOnes()
        {
            List<RoomListItem> result = RoomListItemFilter.Apply(_rooms, RoomPrivacyFilter.Private, NoSearch);

            Assert.Equal(new[] { "2" }, result.Select(room => room.RoomId));
        }

        [Fact]
        public void Apply_TextInTheNameOfTheHost_KeepsOnlyThoseRooms()
        {
            List<RoomListItem> result = RoomListItemFilter.Apply(_rooms, RoomPrivacyFilter.All, "gab");

            Assert.Equal(new[] { "2" }, result.Select(room => room.RoomId));
        }

        [Fact]
        public void Apply_TextWithDifferentCase_StillMatches()
        {
            List<RoomListItem> result = RoomListItemFilter.Apply(_rooms, RoomPrivacyFilter.All, "CARLOS");

            Assert.Equal(new[] { "3" }, result.Select(room => room.RoomId));
        }

        [Fact]
        public void Apply_TextWithSpacesAround_IgnoresTheSpaces()
        {
            List<RoomListItem> result = RoomListItemFilter.Apply(_rooms, RoomPrivacyFilter.All, "  emilio  ");

            Assert.Equal(new[] { "1" }, result.Select(room => room.RoomId));
        }

        [Fact]
        public void Apply_PrivacyAndTextTogether_KeepsOnlyRoomsThatMatchBoth()
        {
            List<RoomListItem> result = RoomListItemFilter.Apply(_rooms, RoomPrivacyFilter.Private, "emilio");

            Assert.Empty(result);
        }

        [Fact]
        public void Apply_TextThatNobodyHas_ReturnsAnEmptyList()
        {
            List<RoomListItem> result = RoomListItemFilter.Apply(_rooms, RoomPrivacyFilter.All, "zzz");

            Assert.Empty(result);
        }

        [Fact]
        public void Apply_NoRooms_ReturnsAnEmptyList()
        {
            List<RoomListItem> result =
                RoomListItemFilter.Apply(new List<RoomListItem>(), RoomPrivacyFilter.All, NoSearch);

            Assert.Empty(result);
        }

        private static RoomListItem CreateRoom(string roomId, string hostName, bool isPrivate)
        {
            return new RoomListItem { RoomId = roomId, HostName = hostName, IsPrivate = isPrivate };
        }
    }
}
