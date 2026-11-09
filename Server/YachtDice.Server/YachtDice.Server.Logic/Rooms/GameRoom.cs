using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace YachtDice.Server.Logic.Rooms
{
    /// <summary>
    /// A room that is alive in the server. It is not thread-safe: <see cref="RoomManager"/> only uses it
    /// while it holds its lock.
    /// </summary>
    internal sealed class GameRoom
    {
        private const string IdFormat = "N";
        private const string RepeatedNameFormat = "{0} {1}";
        private const int FirstRepeatedNumber = 2;

        private readonly List<RoomPlayer> _players = new List<RoomPlayer>();

        /// <summary>
        /// Initializes a new room without players. The first player who enters becomes the host.
        /// </summary>
        /// <param name="code">Code of the room.</param>
        /// <param name="settings">Options of the room.</param>
        public GameRoom(string code, RoomSettings settings)
        {
            Id = Guid.NewGuid().ToString(IdFormat, CultureInfo.InvariantCulture);
            Code = code;
            HostId = string.Empty;
            Settings = settings;
        }

        /// <summary>
        /// Gets the identifier of the room inside the list of rooms. It is not the code of the room, which
        /// private rooms keep secret.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the code of the room.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Gets the identifier of the host.
        /// </summary>
        public string HostId { get; private set; }

        /// <summary>
        /// Gets or sets the options of the room.
        /// </summary>
        public RoomSettings Settings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the game already started.
        /// </summary>
        public bool IsStarted { get; set; }

        /// <summary>
        /// Gets the players inside the room, in the order in which they entered.
        /// </summary>
        public IReadOnlyList<RoomPlayer> Players => _players;

        /// <summary>
        /// Puts a new player inside the room. The server gives the player a new identifier, and if another
        /// player already uses the name, a number is added to it so that the names stay different.
        /// </summary>
        /// <param name="displayName">Name that the player asked for.</param>
        /// <returns>The player that entered.</returns>
        public RoomPlayer AddPlayer(string displayName)
        {
            var player = new RoomPlayer(
                Guid.NewGuid().ToString(IdFormat, CultureInfo.InvariantCulture),
                CreateUniqueName(displayName.Trim()));

            _players.Add(player);

            if (string.IsNullOrEmpty(HostId))
            {
                HostId = player.Id;
            }

            return player;
        }

        /// <summary>
        /// Indicates whether the text is the code of the room. The case does not matter.
        /// </summary>
        /// <param name="code">Code to compare, without spaces around it.</param>
        /// <returns>True if it is the code of the room.</returns>
        public bool HasCode(string code)
        {
            return string.Equals(Code, code, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Indicates whether the player is inside the room.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>True if a player with that identifier is inside.</returns>
        public bool HasPlayer(string playerId)
        {
            return _players.Any(player => IsSameId(player.Id, playerId));
        }

        /// <summary>
        /// Looks for a player of the room.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="player">The player, or null if nobody inside has that identifier.</param>
        /// <returns>True if the player is inside the room.</returns>
        public bool TryGetPlayer(string playerId, out RoomPlayer player)
        {
            player = _players.FirstOrDefault(candidate => IsSameId(candidate.Id, playerId));

            return player != null;
        }

        /// <summary>
        /// Indicates whether the player is the host of the room.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>True if the player is the host.</returns>
        public bool IsHost(string playerId)
        {
            return IsSameId(HostId, playerId);
        }

        /// <summary>
        /// Takes the player out of the room. If the player was the host, the next player becomes the host.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        public void RemovePlayer(string playerId)
        {
            bool wasHost = IsHost(playerId);

            _players.RemoveAll(player => IsSameId(player.Id, playerId));

            if (wasHost && _players.Any())
            {
                HostId = _players.First().Id;
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

            var details = new RoomDetails
            {
                Code = Code,
                HostId = HostId,
                Players = _players.ToList(),
                Settings = settings
            };

            return details;
        }

        /// <summary>
        /// Creates the short description of the room that is shown in the list of rooms.
        /// </summary>
        /// <returns>A description that does not include the code of the room.</returns>
        public RoomSummary ToSummary()
        {
            RoomPlayer host = _players.First(player => IsSameId(player.Id, HostId));

            var summary = new RoomSummary
            {
                RoomId = Id,
                HostName = host.DisplayName,
                PlayerCount = _players.Count,
                MaxPlayers = Settings.MaxPlayers,
                IsPrivate = Settings.IsPrivate,
                Speed = Settings.Speed
            };

            return summary;
        }

        private static bool IsSameId(string firstId, string secondId)
        {
            return string.Equals(firstId, secondId, StringComparison.Ordinal);
        }

        private string CreateUniqueName(string displayName)
        {
            string uniqueName = displayName;
            int repeatedNumber = FirstRepeatedNumber;

            while (IsNameTaken(uniqueName))
            {
                uniqueName = string.Format(
                    CultureInfo.InvariantCulture,
                    RepeatedNameFormat,
                    displayName,
                    repeatedNumber);
                repeatedNumber++;
            }

            return uniqueName;
        }

        private bool IsNameTaken(string displayName)
        {
            return _players.Any(
                player => string.Equals(player.DisplayName, displayName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
