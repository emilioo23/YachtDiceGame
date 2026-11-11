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
    }
}
