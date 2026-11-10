using YachtDice.Resources;
using YachtDice.RoomServiceReference;

namespace YachtDice.Utils
{
    /// <summary>
    /// Chooses the notice that the chat of a match shows when a player leaves it or comes back.
    /// </summary>
    public static class PresenceMessages
    {
        /// <summary>
        /// Gets the notice for a player who is no longer inside the match.
        /// </summary>
        /// <param name="eventType">Event of the room that removed the player.</param>
        /// <param name="playerName">Name of the player.</param>
        /// <returns>The text to show in the chat.</returns>
        public static string GetDepartureMessage(RoomEventType eventType, string playerName)
        {
            string format;

            switch (eventType)
            {
                case RoomEventType.PlayerKicked:
                    format = Strings.Game_Chat_PlayerKicked;
                    break;
                default:
                    format = Strings.Game_Chat_PlayerLeft;
                    break;
            }

            return string.Format(format, playerName);
        }

        /// <summary>
        /// Gets the notice for a player who came back to the match.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        /// <returns>The text to show in the chat.</returns>
        public static string GetReturnMessage(string playerName)
        {
            return string.Format(Strings.Game_Chat_PlayerReturned, playerName);
        }
    }
}
