using Xunit;
using YachtDice.RoomServiceReference;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Client.Tests.Utils
{
    /// <summary>
    /// Notices written in the chat when a player leaves or comes back to a match.
    /// </summary>
    public class PresenceMessagesTests
    {
        private const string PlayerName = "Beto";
        private const int UnknownEvent = 99;
        private const string NameWithBraces = "{0}";

        [Fact]
        public void GetDepartureMessage_PlayerKicked_ReturnsTheKickedNotice()
        {
            string message = PresenceMessages.GetDepartureMessage(RoomEventType.PlayerKicked, PlayerName);

            Assert.Equal(string.Format(Strings.Game_Chat_PlayerKicked, PlayerName), message);
        }

        [Fact]
        public void GetDepartureMessage_PlayerDisconnected_ReturnsTheLeftNotice()
        {
            string message = PresenceMessages.GetDepartureMessage(RoomEventType.PlayerDisconnected, PlayerName);

            Assert.Equal(string.Format(Strings.Game_Chat_PlayerLeft, PlayerName), message);
        }

        [Fact]
        public void GetReturnMessage_PlayerName_ReturnsTheReturnNotice()
        {
            string message = PresenceMessages.GetReturnMessage(PlayerName);

            Assert.Equal(string.Format(Strings.Game_Chat_PlayerReturned, PlayerName), message);
        }

        [Fact]
        public void GetDepartureMessage_EventOutsideTheList_ReturnsTheLeftNotice()
        {
            var unknownEvent = (RoomEventType)UnknownEvent;

            string message = PresenceMessages.GetDepartureMessage(unknownEvent, PlayerName);

            Assert.Equal(string.Format(Strings.Game_Chat_PlayerLeft, PlayerName), message);
        }

        [Fact]
        public void GetDepartureMessage_KickedWithoutName_UsesAnEmptyName()
        {
            string message = PresenceMessages.GetDepartureMessage(RoomEventType.PlayerKicked, string.Empty);

            Assert.Equal(string.Format(Strings.Game_Chat_PlayerKicked, string.Empty), message);
        }

        [Fact]
        public void GetDepartureMessage_NullName_UsesAnEmptyName()
        {
            string message = PresenceMessages.GetDepartureMessage(RoomEventType.PlayerDisconnected, null);

            Assert.Equal(string.Format(Strings.Game_Chat_PlayerLeft, string.Empty), message);
        }

        [Fact]
        public void GetReturnMessage_EmptyName_UsesAnEmptyName()
        {
            string message = PresenceMessages.GetReturnMessage(string.Empty);

            Assert.Equal(string.Format(Strings.Game_Chat_PlayerReturned, string.Empty), message);
        }

        [Fact]
        public void GetReturnMessage_NameWithBraces_KeepsTheNameAsWritten()
        {
            string message = PresenceMessages.GetReturnMessage(NameWithBraces);

            Assert.Equal(string.Format(Strings.Game_Chat_PlayerReturned, NameWithBraces), message);
        }
    }
}
