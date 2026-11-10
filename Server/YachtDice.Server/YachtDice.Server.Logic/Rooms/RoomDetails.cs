using System.Collections.Generic;

namespace YachtDice.Server.Logic.Rooms
{
    /// <summary>
    /// Copy of the state of a room at one moment. Changing it does not change the room.
    /// </summary>
    public class RoomDetails
    {
        /// <summary>
        /// Gets the code that the players type to enter the room.
        /// </summary>
        public string Code { get; internal set; }

        /// <summary>
        /// Gets the identifier of the player who controls the room.
        /// </summary>
        public string HostId { get; internal set; }

        /// <summary>
        /// Gets the players inside the room, in the order in which they entered.
        /// </summary>
        public List<RoomPlayer> Players { get; internal set; }

        /// <summary>
        /// Gets the players who left a game in progress and can still come back.
        /// </summary>
        public List<RoomPlayer> AbsentPlayers { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the game of the room already started.
        /// </summary>
        public bool IsStarted { get; internal set; }

        /// <summary>
        /// Gets the options of the room.
        /// </summary>
        public RoomSettings Settings { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether nobody is inside the room.
        /// </summary>
        public bool IsEmpty => Players.Count == 0;

        /// <summary>
        /// Creates the details that stand for "no room": no code, no host and no players.
        /// </summary>
        /// <returns>Details of an empty room.</returns>
        public static RoomDetails CreateEmpty()
        {
            var details = new RoomDetails
            {
                Code = string.Empty,
                HostId = string.Empty,
                Players = new List<RoomPlayer>(),
                AbsentPlayers = new List<RoomPlayer>(),
                Settings = new RoomSettings()
            };

            return details;
        }
    }
}
