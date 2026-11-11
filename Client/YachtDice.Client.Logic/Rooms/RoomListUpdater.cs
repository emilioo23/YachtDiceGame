using System;
using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Client.Logic.Rooms
{
    /// <summary>
    /// Rules to replace the list of rooms that the player sees with the new list that the server sends.
    /// </summary>
    public static class RoomListUpdater
    {
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
            Func<bool, bool> isTrue = isSame => isSame;

            return oldRooms.Count == newRooms.Count
                && oldRooms
                    .Zip(newRooms, (oldRoom, newRoom) => oldRoom.HasSameContentAs(newRoom))
                    .All(isTrue);
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
    }
}
