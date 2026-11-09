using System.Collections.Generic;
using System.Linq;
using YachtDice.Models;
using YachtDice.Resources;
using YachtDice.RoomServiceReference;

namespace YachtDice.Utils
{
    /// <summary>
    /// Builds the rows of the list of rooms from the descriptions that the server sends.
    /// </summary>
    public static class RoomListItemFactory
    {
        /// <summary>
        /// Creates one row for each room, in the same order.
        /// </summary>
        /// <param name="rooms">Descriptions of the rooms sent by the server.</param>
        /// <returns>The rows to show.</returns>
        public static List<RoomListItem> Create(IEnumerable<RoomSummary> rooms)
        {
            return rooms.Select(CreateItem).ToList();
        }

        private static RoomListItem CreateItem(RoomSummary room)
        {
            var item = new RoomListItem
            {
                RoomId = room.RoomId,
                HostName = room.HostName,
                Initial = PlayerInitials.GetInitial(room.HostName),
                Title = string.Format(Strings.Rooms_TitleFormat, room.HostName),
                Details = string.Format(
                    Strings.Rooms_DetailsFormat,
                    room.PlayerCount,
                    room.MaxPlayers,
                    GetSpeedText(room.Speed)),
                PrivacyText = room.IsPrivate ? Strings.Lobby_OptionPrivate : Strings.Lobby_OptionPublic,
                IsPrivate = room.IsPrivate,
                HasSpace = room.PlayerCount < room.MaxPlayers
            };

            return item;
        }

        private static string GetSpeedText(GameSpeed speed)
        {
            return speed == GameSpeed.Fast ? Strings.Lobby_Mode30SecondsTitle : Strings.Lobby_Mode1MinuteTitle;
        }
    }
}
