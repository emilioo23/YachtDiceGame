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
            RoomSummary summary = CreateSummary(TwoPlayers, FourPlayers, false);
            RoomListItem expected = CreateExpectedItem(summary, true);

            RoomListItem item = CreateItem(summary);

            Assert.Equivalent(expected, item);
        }

        [Fact]
        public void Create_FullRoom_ReturnsTheRowWithoutSpace()
        {
            RoomSummary summary = CreateSummary(FourPlayers, FourPlayers, false);
            RoomListItem expected = CreateExpectedItem(summary, false);

            RoomListItem item = CreateItem(summary);

            Assert.Equivalent(expected, item);
        }

        [Fact]
        public void Create_PrivateRoom_ReturnsTheRowWithThePrivateText()
        {
            RoomSummary summary = CreateSummary(TwoPlayers, FourPlayers, true);
            RoomListItem expected = CreateExpectedItem(summary, true);

            RoomListItem item = CreateItem(summary);

            Assert.Equivalent(expected, item);
        }

        private static RoomListItem CreateExpectedItem(RoomSummary summary, bool hasSpace)
        {
            var item = new RoomListItem
            {
                RoomId = RoomId,
                HostName = HostName,
                Initial = HostInitial,
                Title = string.Format(Strings.Rooms_TitleFormat, HostName),
                Details = string.Format(Strings.Rooms_DetailsFormat, summary.PlayerCount, summary.MaxPlayers, Strings.Lobby_Mode1MinuteTitle),
                PrivacyText = summary.IsPrivate ? Strings.Lobby_OptionPrivate : Strings.Lobby_OptionPublic,
                IsPrivate = summary.IsPrivate,
                HasSpace = hasSpace
            };
            return item;
        }

        private static RoomSummary CreateSummary(int playerCount, int maxPlayers, bool isPrivate)
        {
            return new RoomSummary
            {
                RoomId = RoomId,
                HostName = HostName,
                PlayerCount = playerCount,
                MaxPlayers = maxPlayers,
                IsPrivate = isPrivate,
                Speed = GameSpeed.Normal
            };
        }

        private static RoomListItem CreateItem(RoomSummary summary)
        {
            RoomSummary[] summaries = { summary };

            List<RoomListItem> items = RoomListItemFactory.Create(summaries);

            return items.Single();
        }
    }
}
