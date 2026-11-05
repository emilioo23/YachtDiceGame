using System;
using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Server.Logic.Rooms
{
    /// <summary>
    /// A room that is alive in the server. It is not thread-safe: <see cref="RoomManager"/> only uses it
    /// while it holds its lock.
    /// </summary>
    internal sealed class GameRoom
    {
        /// <summary>
        /// Initializes a new room with its host as the only player.
        /// </summary>
        /// <param name="code">Code of the room.</param>
        /// <param name="hostName">Name of the host.</param>
        /// <param name="settings">Options of the room.</param>
        public GameRoom(string code, string hostName, RoomSettings settings)
        {
            Code = code;
            HostName = hostName;
            Settings = settings;
            Players = new List<string> { hostName };
        }

        /// <summary>
        /// Gets the code of the room.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Gets or sets the name of the host.
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// Gets or sets the options of the room.
        /// </summary>
        public RoomSettings Settings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the game already started.
        /// </summary>
        public bool IsStarted { get; set; }

        /// <summary>
        /// Gets the names of the players inside the room.
        /// </summary>
        public List<string> Players { get; }

        /// <summary>
        /// Indicates whether the player is inside the room.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        /// <returns>True if a player with that name is inside.</returns>
        public bool HasPlayer(string playerName)
        {
            return Players.Contains(playerName, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Indicates whether the player is the host of the room.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        /// <returns>True if the player is the host.</returns>
        public bool IsHost(string playerName)
        {
            return string.Equals(HostName, playerName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Takes the player out of the room. If the player was the host, the next player becomes the host.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        public void RemovePlayer(string playerName)
        {
            Players.RemoveAll(name => string.Equals(name, playerName, StringComparison.OrdinalIgnoreCase));

            if (IsHost(playerName) && Players.Any())
            {
                HostName = Players.First();
            }
        }

        /// <summary>
        /// Creates a copy of the current state of the room.
        /// </summary>
        /// <returns>Details that do not change when the room changes.</returns>
        public RoomDetails ToDetails()
        {
            var settings = new RoomSettings
            {
                MaxPlayers = Settings.MaxPlayers,
                IsPrivate = Settings.IsPrivate,
                Speed = Settings.Speed
            };

            return new RoomDetails(Code, HostName, Players.ToList(), settings);
        }
    }
}
