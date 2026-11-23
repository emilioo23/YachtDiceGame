using System;
using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Client.Logic.Rooms
{
    /// <summary>
    /// Rules of the list of rooms: which rooms are shown, how they update and how the presence is compared.
    /// </summary>
    public static class RoomListRules
    {
        private const int EmptyLength = 0;
        private const int NotFoundIndex = -1;

        /// <summary>
        /// Finds the players who were inside before and are not inside now.
        /// </summary>
        /// <param name="before">Players inside the room before the change.</param>
        /// <param name="after">Players inside the room after the change.</param>
        /// <returns>The players who left, in the order they had.</returns>
        public static List<GameParticipant> FindDeparted(
            IEnumerable<GameParticipant> before,
            IEnumerable<GameParticipant> after)
        {
            return FindMissing(before, after);
        }

        /// <summary>
        /// Finds the players who are inside now and were not inside before.
        /// </summary>
        /// <param name="before">Players inside the room before the change.</param>
        /// <param name="after">Players inside the room after the change.</param>
        /// <returns>The players who arrived, in the order they have.</returns>
        public static List<GameParticipant> FindArrived(
            IEnumerable<GameParticipant> before,
            IEnumerable<GameParticipant> after)
        {
            return FindMissing(after, before);
        }

        /// <summary>
        /// Keeps the rows that match the kind of room and contain the text in the name of the host. The
        /// comparison ignores the case and the spaces around the text. The order of the rows does not change.
        /// </summary>
        /// <param name="rooms">Rows of the list of rooms.</param>
        /// <param name="privacy">Kind of rooms to keep.</param>
        /// <param name="searchText">Text to look for in the name of the host. Null is taken as no text.</param>
        /// <returns>The rows that the player must see.</returns>
        public static List<RoomListItem> Apply(
            IEnumerable<RoomListItem> rooms,
            RoomPrivacyFilter privacy,
            string searchText)
        {
            string search = (searchText ?? string.Empty).Trim();
            Func<RoomListItem, bool> isVisible = room => MatchesPrivacy(room, privacy) && MatchesName(room, search);

            return rooms
                .Where(isVisible)
                .ToList();
        }

        /// <summary>
        /// Indicates whether the new list shows the same rooms, in the same order and state, as the old one.
        /// The access codes that the player typed do not count.
        /// </summary>
        /// <param name="oldRooms">Rows that the player sees now.</param>
        /// <param name="newRooms">Rows built from the latest answer of the server.</param>
        /// <returns>True if nothing changed, so the window can leave the list as it is.</returns>
        public static bool HasSameContent(
            IReadOnlyList<RoomListItem> oldRooms,
            IReadOnlyList<RoomListItem> newRooms)
        {
            return oldRooms.Count == newRooms.Count
                && oldRooms
                    .Zip(newRooms, (oldRoom, newRoom) => oldRoom.HasSameContentAs(newRoom))
                    .All(isSame => isSame);
        }

        /// <summary>
        /// Copies the access code that the player typed in a row to the row of the same room in the new list,
        /// so the code is not lost when the list updates itself.
        /// </summary>
        /// <param name="oldRooms">Rows that the player sees now.</param>
        /// <param name="newRooms">Rows built from the latest answer of the server; they are changed.</param>
        public static void KeepTypedCodes(IEnumerable<RoomListItem> oldRooms, IEnumerable<RoomListItem> newRooms)
        {
            Dictionary<string, string> codes = oldRooms.ToDictionary(room => room.RoomId, room => room.AccessCode);

            foreach (RoomListItem room in newRooms)
            {
                if (codes.TryGetValue(room.RoomId, out string code))
                {
                    room.AccessCode = code;
                }
            }
        }

        private static List<GameParticipant> FindMissing(
            IEnumerable<GameParticipant> source,
            IEnumerable<GameParticipant> reference)
        {
            var referenceIds = new HashSet<string>(reference.Select(player => player.Id), StringComparer.Ordinal);
            Func<GameParticipant, bool> isMissingFromReference = player => !referenceIds.Contains(player.Id);

            return source.Where(isMissingFromReference).ToList();
        }

        private static bool MatchesPrivacy(RoomListItem room, RoomPrivacyFilter privacy)
        {
            bool isPrivateWanted = privacy == RoomPrivacyFilter.Private;

            return privacy == RoomPrivacyFilter.All || room.IsPrivate == isPrivateWanted;
        }

        private static bool MatchesName(RoomListItem room, string search)
        {
            bool isInHostName = room.HostName.IndexOf(search, StringComparison.OrdinalIgnoreCase) > NotFoundIndex;
            bool isInTitle = (room.Title ?? string.Empty).IndexOf(search, StringComparison.OrdinalIgnoreCase)
                > NotFoundIndex;

            return search.Length == EmptyLength || isInHostName || isInTitle;
        }
    }
}
