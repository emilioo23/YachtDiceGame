using System;
using System.Collections.Generic;
using YachtDice.Resources;
using YachtDice.RoomServiceReference;

namespace YachtDice.Utils
{
    /// <summary>
    /// Chooses the notice that the chat of a match shows when a player leaves it or comes back.
    /// </summary>
    public static class PresenceMessages
    {
        private static readonly Dictionary<RoomEventType, Func<string>> DepartureFormatByEvent =
            new Dictionary<RoomEventType, Func<string>>
            {
                { RoomEventType.PlayerKicked, () => Strings.Game_Chat_PlayerKicked }
            };

        /// <summary>
        /// Gets the notice for a player who is no longer inside the match.
        /// </summary>
        /// <param name="eventType">Event of the room that removed the player.</param>
        /// <param name="playerName">Name of the player.</param>
        /// <returns>The text to show in the chat.</returns>
        public static string GetDepartureMessage(RoomEventType eventType, string playerName)
        {
            Func<string> getFormat;

            if (!DepartureFormatByEvent.TryGetValue(eventType, out getFormat))
            {
                getFormat = () => Strings.Game_Chat_PlayerLeft;
            }

            return string.Format(getFormat(), playerName);
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
