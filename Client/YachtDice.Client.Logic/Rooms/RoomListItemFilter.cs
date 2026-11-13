using System;
using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Client.Logic.Rooms
{
    /// <summary>
    /// Chooses which rows of the list of rooms the player sees, according to the kind of room and to the text
    /// that the player searches.
    /// </summary>
    public static class RoomListItemFilter
    {
        private const int EmptyLength = 0;
        private const int FirstCharacterIndex = 0;
        private const string NullSearchMessage = "The search cannot be null.";

        /// <summary>
        /// Keeps the rows that match the kind of room and contain the text in the name of the host. The
        /// comparison ignores the case and the spaces around the text. The order of the rows does not change.
        /// </summary>
        /// <param name="rooms">Rows of the list of rooms.</param>
        /// <param name="roomSearch">Kind of rooms to keep and text to look for in the name of the host.</param>
        /// <returns>The rows that the player must see.</returns>
        /// <exception cref="ArgumentNullException">The search is null.</exception>
        public static List<RoomListItem> Apply(IEnumerable<RoomListItem> rooms, RoomSearch roomSearch)
        {
            if (roomSearch == null)
            {
                throw new ArgumentNullException(nameof(roomSearch), NullSearchMessage);
            }

            RoomPrivacyFilter privacy = roomSearch.Privacy;
            string search = roomSearch.SearchText.Trim();
            Func<RoomListItem, bool> isVisible = room => MatchesPrivacy(room, privacy) && MatchesName(room, search);

            return rooms
                .Where(isVisible)
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
