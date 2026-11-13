using System.Collections.Generic;
using System.Linq;
using Xunit;
using YachtDice.Client.Logic.Rooms;
using YachtDice.Resources;
using YachtDice.RoomServiceReference;
using YachtDice.Utils;

namespace YachtDice.Client.Tests.Utils
{
    /// <summary>
    /// Rows of the list of rooms built from the descriptions sent by the server.
    /// </summary>
    public class RoomListItemFactoryTests
    {
        private const string RoomId = "room-1";
        private const string HostName = "Ana";
        private const int TwoPlayers = 2;
        private const int FourPlayers = 4;

        private const string HostInitial = "A";

        [Fact]
        public void Create_RoomWithFreeSeats_ReturnsTheRowWithTheDataOfTheRoom()
        {
            RoomListItem expected = CreateExpectedItem(TwoPlayers, FourPlayers, false, true);

            RoomListItem item = CreateItem(TwoPlayers, FourPlayers, false);

            Assert.Equivalent(expected, item);
        }

        [Fact]
        public void Create_FullRoom_ReturnsTheRowWithoutSpace()
        {
            RoomListItem expected = CreateExpectedItem(FourPlayers, FourPlayers, false, false);

            RoomListItem item = CreateItem(FourPlayers, FourPlayers, false);

            Assert.Equivalent(expected, item);
        }

        [Fact]
        public void Create_PrivateRoom_ReturnsTheRowWithThePrivateText()
        {
            RoomListItem expected = CreateExpectedItem(TwoPlayers, FourPlayers, true, true);

            RoomListItem item = CreateItem(TwoPlayers, FourPlayers, true);

            Assert.Equivalent(expected, item);
        }

        private static RoomListItem CreateExpectedItem(int playerCount, int maxPlayers, bool isPrivate, bool hasSpace)
        {
            var item = new RoomListItem
            {
                RoomId = RoomId,
                HostName = HostName,
                Initial = HostInitial,
                Title = string.Format(Strings.Rooms_TitleFormat, HostName),
                Details = string.Format(
                    Strings.Rooms_DetailsFormat,
                    playerCount,
                    maxPlayers,
                    Strings.Lobby_Mode1MinuteTitle),
                PrivacyText = isPrivate ? Strings.Lobby_OptionPrivate : Strings.Lobby_OptionPublic,
                IsPrivate = isPrivate,
                HasSpace = hasSpace
            };
            return item;
        }

        private static RoomListItem CreateItem(int playerCount, int maxPlayers, bool isPrivate)
        {
            var summary = new RoomSummary
            {
                RoomId = RoomId,
                HostName = HostName,
                PlayerCount = playerCount,
                MaxPlayers = maxPlayers,
                IsPrivate = isPrivate,
                Speed = GameSpeed.Normal
            };
            RoomSummary[] summaries = { summary };

            List<RoomListItem> items = RoomListItemFactory.Create(summaries);

            return items.Single();
        }
    }
}
