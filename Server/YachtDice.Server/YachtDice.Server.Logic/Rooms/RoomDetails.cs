using System.Collections.Generic;

namespace YachtDice.Server.Logic.Rooms
{
    /// <summary>
    /// Copy of the state of a room at one moment. Changing it does not change the room.
    /// </summary>
    public class RoomDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoomDetails"/> class.
        /// </summary>
        /// <param name="code">Code of the room.</param>
        /// <param name="hostName">Name of the host.</param>
        /// <param name="players">Names of the players inside the room.</param>
        /// <param name="settings">Options of the room.</param>
        public RoomDetails(string code, string hostName, List<string> players, RoomSettings settings)
        {
            Code = code;
            HostName = hostName;
            Players = players;
            Settings = settings;
        }

        /// <summary>
        /// Gets the code that the players type to enter the room.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Gets the name of the player who controls the room.
        /// </summary>
        public string HostName { get; }

        /// <summary>
        /// Gets the names of the players inside the room, in the order in which they entered.
        /// </summary>
        public List<string> Players { get; }

        /// <summary>
        /// Gets the options of the room.
        /// </summary>
        public RoomSettings Settings { get; }

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
            return new RoomDetails(string.Empty, string.Empty, new List<string>(), new RoomSettings());
        }
    }
}
