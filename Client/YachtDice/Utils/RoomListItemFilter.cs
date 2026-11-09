using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Models;

namespace YachtDice.Utils
{
    /// <summary>
    /// Chooses which rows of the list of rooms the player sees, according to the kind of room and to the text
    /// that the player searches.
    /// </summary>
    public static class RoomListItemFilter
    {
        private const int EmptyLength = 0;
        private const int FirstCharacterIndex = 0;

        /// <summary>
        /// Keeps the rows that match the kind of room and contain the text in the name of the host. The
        /// comparison ignores the case and the spaces around the text. The order of the rows does not change.
        /// </summary>
        /// <param name="rooms">Rows of the list of rooms.</param>
        /// <param name="privacy">Kind of rooms to keep.</param>
        /// <param name="searchText">Text to look for in the name of the host; empty keeps every row.</param>
        /// <returns>The rows that the player must see.</returns>
        public static List<RoomListItem> Apply(
            IEnumerable<RoomListItem> rooms,
            RoomPrivacyFilter privacy,
            string searchText)
        {
            string search = searchText.Trim();

            return rooms
                .Where(room => MatchesPrivacy(room, privacy) && MatchesName(room, search))
                .ToList();
        }

        private static bool MatchesPrivacy(RoomListItem room, RoomPrivacyFilter privacy)
        {
            return privacy == RoomPrivacyFilter.All || room.IsPrivate == (privacy == RoomPrivacyFilter.Private);
        }

        private static bool MatchesName(RoomListItem room, string search)
        {
            return search.Length == EmptyLength
                || room.HostName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= FirstCharacterIndex;
        }
    }
}
