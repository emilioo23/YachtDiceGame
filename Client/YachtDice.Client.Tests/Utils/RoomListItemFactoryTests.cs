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

        [Fact]
        public void Create_RoomSummary_KeepsTheIdentifierOfTheRoom()
        {
            RoomListItem item = CreateItem(TwoPlayers, FourPlayers, false);

            Assert.Equal(RoomId, item.RoomId);
        }

        [Fact]
        public void Create_RoomSummary_ShowsTheHostInTheTitle()
        {
            RoomListItem item = CreateItem(TwoPlayers, FourPlayers, false);

            Assert.Equal(string.Format(Strings.Rooms_TitleFormat, HostName), item.Title);
        }

        [Fact]
        public void Create_RoomWithFreeSeats_HasSpace()
        {
            RoomListItem item = CreateItem(TwoPlayers, FourPlayers, false);

            Assert.True(item.HasSpace);
        }

        [Fact]
        public void Create_FullRoom_HasNoSpace()
        {
            RoomListItem item = CreateItem(FourPlayers, FourPlayers, false);

            Assert.False(item.HasSpace);
        }

        [Fact]
        public void Create_PrivateRoom_ShowsThePrivateText()
        {
            RoomListItem item = CreateItem(TwoPlayers, FourPlayers, true);

            Assert.Equal(Strings.Lobby_OptionPrivate, item.PrivacyText);
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
