using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Server.Services.Mapping
{
    using Contracts = YachtDice.Server.Contracts.Rooms;
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
            var state = new Contracts.RoomState
            {
                Code = room.Code,
                HostId = room.HostId,
                Players = room.Players.Select(ToContract).ToList(),
                AbsentPlayers = room.AbsentPlayers.Select(ToContract).ToList(),
                IsStarted = room.IsStarted,
                Settings = ToContract(room.Settings)
            };

            return state;
        }

        /// <summary>
        /// Creates the data contracts of the rooms of the list of rooms.
        /// </summary>
        /// <param name="rooms">Descriptions of the rooms in the logic layer.</param>
        /// <returns>The descriptions for the clients.</returns>
        public static List<Contracts.RoomSummary> ToSummaries(List<Logic.Rooms.RoomSummary> rooms)
        {
            return rooms.Select(ToContract).ToList();
        }

        /// <summary>
        /// Creates the room of the list of the logic layer from the one sent by a client.
        /// </summary>
        /// <param name="room">Room of the list received from the client.</param>
        /// <returns>The room of the list for the logic layer.</returns>
        public static Logic.Rooms.Room ToLogic(Contracts.Room room)
        {
            var logicRoom = new Logic.Rooms.Room
            {
                RoomId = room.RoomId,
                AccessCode = room.AccessCode
            };

            return logicRoom;
        }

        /// <summary>
        /// Creates the options of the logic layer from the ones sent by a client.
        /// </summary>
        /// <param name="settings">Options received from the client.</param>
        /// <returns>The options for the logic layer.</returns>
        public static Logic.Rooms.RoomSettings ToLogic(Contracts.RoomSettings settings)
        {
            var logicSettings = new Logic.Rooms.RoomSettings
            {
                MaxPlayers = settings.MaxPlayers,
                IsPrivate = settings.IsPrivate,
                Speed = EnumMapper.Map<Contracts.GameSpeed, Logic.Rooms.GameSpeed>(settings.Speed)
            };

            return logicSettings;
        }

        /// <summary>
        /// Creates the answer to the player who tried to enter a room.
        /// </summary>
        /// <param name="result">Result of the logic layer.</param>
        /// <returns>The answer for the client.</returns>
        public static Contracts.JoinRoomResult ToJoinResult(Logic.Rooms.RoomResult result)
        {
            var joinResult = new Contracts.JoinRoomResult
            {
                Code = ToResultCode(result.Status),
                PlayerId = result.PlayerId,
                Room = ToContract(result.Room)
            };

            return joinResult;
        }

        /// <summary>
        /// Creates the answer to the player who created a room.
        /// </summary>
        /// <param name="result">Result of the logic layer.</param>
        /// <returns>The answer for the client.</returns>
        public static Contracts.CreateRoomResult ToCreateResult(Logic.Rooms.RoomResult result)
        {
            var createResult = new Contracts.CreateRoomResult
            {
                PlayerId = result.PlayerId,
                Room = ToContract(result.Room)
            };

            return createResult;
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

        /// <summary>
        /// Creates the data contract of the options of a room.
        /// </summary>
        /// <param name="settings">Options in the logic layer.</param>
        /// <returns>The options for the clients.</returns>
        private static Contracts.RoomSettings ToContract(Logic.Rooms.RoomSettings settings)
        {
            var contract = new Contracts.RoomSettings
            {
                MaxPlayers = settings.MaxPlayers,
                IsPrivate = settings.IsPrivate,
                Speed = EnumMapper.Map<Logic.Rooms.GameSpeed, Contracts.GameSpeed>(settings.Speed)
            };

            return contract;
        }

        /// <summary>
        /// Creates the data contract of a room of the list of rooms.
        /// </summary>
        /// <param name="room">Description of the room in the logic layer.</param>
        /// <returns>The description for the clients.</returns>
        private static Contracts.RoomSummary ToContract(Logic.Rooms.RoomSummary room)
        {
            var contract = new Contracts.RoomSummary
            {
                RoomId = room.RoomId,
                HostName = room.HostName,
                PlayerCount = room.PlayerCount,
                MaxPlayers = room.MaxPlayers,
                IsPrivate = room.IsPrivate,
                Speed = EnumMapper.Map<Logic.Rooms.GameSpeed, Contracts.GameSpeed>(room.Speed)
            };

            return contract;
        }

        /// <summary>
        /// Creates the data contract of a player inside a room.
        /// </summary>
        /// <param name="player">Player in the logic layer.</param>
        /// <returns>The player for the clients.</returns>
        private static Contracts.RoomPlayer ToContract(Logic.Rooms.RoomPlayer player)
        {
            var contract = new Contracts.RoomPlayer
            {
                Id = player.Id,
                DisplayName = player.DisplayName,
                AvatarId = player.AvatarId
            };

            return contract;
        }
    }
}
