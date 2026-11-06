using System.Linq;

namespace YachtDice.Server.Services.Mapping
{
    using Contracts = YachtDice.Server.Contracts;
    using Logic = YachtDice.Server.Logic;

    /// <summary>
    /// Converts the rooms of the logic layer into data contracts and the other way around.
    /// </summary>
    internal static class RoomMapper
    {
        /// <summary>
        /// Creates the data contract that is sent to the clients.
        /// </summary>
        /// <param name="room">State of the room in the logic layer.</param>
        /// <returns>The state of the room for the clients.</returns>
        public static Contracts.RoomState ToContract(Logic.Rooms.RoomDetails room)
        {
            return new Contracts.RoomState
            {
                Code = room.Code,
                HostName = room.HostName,
                Players = room.Players.ToList(),
                Settings = ToContract(room.Settings)
            };
        }

        /// <summary>
        /// Creates the data contract of the options of a room.
        /// </summary>
        /// <param name="settings">Options in the logic layer.</param>
        /// <returns>The options for the clients.</returns>
        public static Contracts.RoomSettings ToContract(Logic.Rooms.RoomSettings settings)
        {
            return new Contracts.RoomSettings
            {
                MaxPlayers = settings.MaxPlayers,
                IsPrivate = settings.IsPrivate,
                Speed = EnumMapper.Map<Logic.Rooms.GameSpeed, Contracts.GameSpeed>(settings.Speed)
            };
        }

        /// <summary>
        /// Creates the options of the logic layer from the ones sent by a client.
        /// </summary>
        /// <param name="settings">Options received from the client.</param>
        /// <returns>The options for the logic layer.</returns>
        public static Logic.Rooms.RoomSettings ToLogic(Contracts.RoomSettings settings)
        {
            return new Logic.Rooms.RoomSettings
            {
                MaxPlayers = settings.MaxPlayers,
                IsPrivate = settings.IsPrivate,
                Speed = EnumMapper.Map<Contracts.GameSpeed, Logic.Rooms.GameSpeed>(settings.Speed)
            };
        }

        /// <summary>
        /// Creates the answer to the player who tried to enter a room.
        /// </summary>
        /// <param name="result">Result of the logic layer.</param>
        /// <returns>The answer for the client.</returns>
        public static Contracts.JoinRoomResult ToJoinResult(Logic.Rooms.RoomResult result)
        {
            return new Contracts.JoinRoomResult
            {
                Code = ToResultCode(result.Status),
                Room = ToContract(result.Room)
            };
        }

        /// <summary>
        /// Converts the outcome of the logic layer into the code that is sent to the client.
        /// </summary>
        /// <param name="status">Outcome in the logic layer.</param>
        /// <returns>The code for the client.</returns>
        public static Contracts.RoomResultCode ToResultCode(Logic.Rooms.RoomStatus status)
        {
            return EnumMapper.Map<Logic.Rooms.RoomStatus, Contracts.RoomResultCode>(status);
        }
    }
}
