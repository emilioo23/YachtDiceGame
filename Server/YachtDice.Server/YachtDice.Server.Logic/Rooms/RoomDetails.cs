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
        /// Gets the name of the player who controls the room.
        /// </summary>
        public string HostName { get; internal set; }

        /// <summary>
        /// Gets the names of the players inside the room, in the order in which they entered.
        /// </summary>
        public List<string> Players { get; internal set; }

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
                HostName = string.Empty,
                Players = new List<string>(),
                Settings = new RoomSettings()
            };

            return details;
        }
    }
}
