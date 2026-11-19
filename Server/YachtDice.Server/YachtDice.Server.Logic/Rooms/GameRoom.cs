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
        private const string NullCodeMessage = "The code of the room cannot be null.";
        private const string NullSettingsMessage = "The settings of the room cannot be null.";

        private readonly List<RoomPlayer> _players = new List<RoomPlayer>();
        private readonly List<RoomPlayer> _absentPlayers = new List<RoomPlayer>();
        private readonly Dictionary<string, int> _chatInfractions = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly HashSet<string> _expelledNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new room without players. The first player who enters becomes the host.
        /// </summary>
        /// <param name="code">Code of the room.</param>
        /// <param name="settings">Options of the room.</param>
        /// <param name="createdAt">Moment, in UTC, when the room was created.</param>
        public GameRoom(string code, RoomSettings settings, DateTime createdAt)
        {
            Id = Guid.NewGuid().ToString(IdFormat, CultureInfo.InvariantCulture);
            Code = code ?? throw new ArgumentNullException(nameof(code), NullCodeMessage);
            HostId = string.Empty;
            Settings = settings ?? throw new ArgumentNullException(nameof(settings), NullSettingsMessage);
            CreatedAt = createdAt;
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
        /// Gets the moment, in UTC, when the room was created.
        /// </summary>
        public DateTime CreatedAt { get; }

        /// <summary>
        /// Gets the players inside the room, in the order in which they entered.
        /// </summary>
        public IReadOnlyList<RoomPlayer> Players => _players;

        /// <summary>
        /// Gets a value indicating whether a person is inside the room.
        /// </summary>
        public bool HasPeople => _players.Any(player => !player.IsComputer);

        /// <summary>
        /// Gets a value indicating whether a person left the game in progress and still has time to come back.
        /// </summary>
        public bool HasAbsentPeople => _absentPlayers.Any(player => !player.IsComputer);

        /// <summary>
        /// Puts a new player inside the room. The server gives the player a new identifier, and if another
        /// player already uses the name, a number is added to it so that the names stay different.
        /// </summary>
        /// <param name="applicant">Name, avatar and level of the player who enters.</param>
        /// <returns>The player that entered.</returns>
        public RoomPlayer AddPlayer(RoomApplicant applicant)
        {
            var player = new RoomPlayer(
                CreatePlayerId(),
                CreateUniqueName(applicant.Name.Trim()),
                applicant.AvatarId)
            {
                Level = applicant.Level,
                JoinedAt = DateTime.UtcNow
            };

            _players.Add(player);

            if (string.IsNullOrEmpty(HostId))
            {
                HostId = player.Id;
            }

            return player;
        }

        /// <summary>
        /// Creates a new identifier for a player of a room.
        /// </summary>
        /// <returns>The identifier.</returns>
        public static string CreatePlayerId()
        {
            return Guid.NewGuid().ToString(IdFormat, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Adds a player of a game saved before the server stopped. The player starts absent and can come back
        /// with the same name. The first one added becomes the host.
        /// </summary>
        /// <param name="applicant">Name, avatar and level of the player.</param>
        /// <param name="playerId">Identifier that the game also uses for the player.</param>
        public void AddAbsentPlayer(RoomApplicant applicant, string playerId)
        {
            var player = new RoomPlayer(playerId, applicant.Name.Trim(), applicant.AvatarId)
            {
                Level = applicant.Level,
                JoinedAt = DateTime.UtcNow,
                IsReady = true
            };

            _absentPlayers.Add(player);

            if (string.IsNullOrEmpty(HostId))
            {
                HostId = player.Id;
            }
        }

        /// <summary>
        /// Counts one more chat message with offensive words written by a player in this room.
        /// </summary>
        /// <param name="playerId">Identifier of the player who wrote the message.</param>
        /// <returns>The infractions that the player has in this room, counting this one.</returns>
        public int RegisterChatInfraction(string playerId)
        {
            _chatInfractions.TryGetValue(playerId, out int infractions);
            infractions++;
            _chatInfractions[playerId] = infractions;

            return infractions;
        }

        /// <summary>
        /// Saves whether a player is ready to start the game.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="isReady">True if the player is ready.</param>
        public void SetReady(string playerId, bool isReady)
        {
            if (TryGetPlayer(playerId, out RoomPlayer player))
            {
                player.IsReady = isReady;
            }
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
            bool isHostLeaving = IsHost(playerId);

            _players.RemoveAll(player => IsSameId(player.Id, playerId));

            RoomPlayer nextHost = _players.FirstOrDefault(player => !player.IsComputer);

            if (isHostLeaving && nextHost != null)
            {
                HostId = nextHost.Id;
            }
        }

        /// <summary>
        /// Takes a player out of the room and remembers their name, so they cannot enter this room again.
        /// </summary>
        /// <param name="playerId">Identifier of the player to expel.</param>
        public void Expel(string playerId)
        {
            if (TryGetPlayer(playerId, out RoomPlayer player) && !player.IsComputer)
            {
                _expelledNames.Add(player.DisplayName);
            }

            RemovePlayer(playerId);
        }

        /// <summary>
        /// Finds a person inside the room by name. A person who lost the network can still look present for the
        /// server for a while, so the same person asks to come back before the server notices.
        /// </summary>
        /// <param name="displayName">Name that the player who wants to come back uses.</param>
        /// <param name="player">The person with that name, or null if there is none.</param>
        /// <returns>True if a person with that name is inside the room.</returns>
        public bool TryFindPerson(string displayName, out RoomPlayer player)
        {
            string name = displayName.Trim();

            player = _players.FirstOrDefault(
                candidate => !candidate.IsComputer
                    && string.Equals(candidate.DisplayName, name, StringComparison.OrdinalIgnoreCase));

            return player != null;
        }

        /// <summary>
        /// Tells whether a name belongs to a player who was expelled from this room.
        /// </summary>
        /// <param name="displayName">Name that the player who wants to enter uses.</param>
        /// <returns>True if the host expelled a player with that name.</returns>
        public bool IsExpelled(string displayName)
        {
            return _expelledNames.Contains(displayName.Trim());
        }

        /// <summary>
        /// Moves the player out of the room but keeps the place, so the player can return to a game in
        /// progress. If the player was the host, the next player becomes the host.
        /// </summary>
        /// <param name="playerId">Identifier of the player who left.</param>
        public void MarkAbsent(string playerId)
        {
            if (TryGetPlayer(playerId, out RoomPlayer player))
            {
                RemovePlayer(playerId);
                _absentPlayers.Add(player);
            }
        }

        /// <summary>
        /// Forgets an absent player for good, so the player cannot come back to the game.
        /// </summary>
        /// <param name="playerId">Identifier of the absent player.</param>
        public void ForgetAbsentPlayer(string playerId)
        {
            _absentPlayers.RemoveAll(player => IsSameId(player.Id, playerId));
        }

        /// <summary>
        /// Brings back the absent player who has the name. The player keeps the identifier and the avatar.
        /// </summary>
        /// <param name="displayName">Name that the returning player shows.</param>
        /// <param name="player">The player who returned, if there was an absent one with that name.</param>
        /// <returns><c>true</c> if an absent player had the name; otherwise, <c>false</c>.</returns>
        public bool TryReturnPlayer(string displayName, out RoomPlayer player)
        {
            string name = displayName.Trim();

            player = _absentPlayers.FirstOrDefault(
                candidate => string.Equals(candidate.DisplayName, name, StringComparison.OrdinalIgnoreCase));

            if (player != null)
            {
                _absentPlayers.Remove(player);
                _players.Add(player);
                TakeHostIfMissing(player);
            }

            return player != null;
        }

        /// <summary>
        /// Checks whether an absent player has the name.
        /// </summary>
        /// <param name="displayName">Name to look for.</param>
        /// <returns><c>true</c> if the player left the game and can return; otherwise, <c>false</c>.</returns>
        public bool HasAbsentPlayer(string displayName)
        {
            string name = displayName.Trim();

            return _absentPlayers.Any(
                player => string.Equals(player.DisplayName, name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Makes another player of the room the host.
        /// </summary>
        /// <param name="playerId">Identifier of the player who becomes the host. It must be inside the room.</param>
        public void SetHost(string playerId)
        {
            HostId = playerId;
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
                Speed = Settings.Speed,
                Name = Settings.Name
            };

            var details = new RoomDetails
            {
                Code = Code,
                HostId = HostId,
                Players = _players.ToList(),
                AbsentPlayers = _absentPlayers.ToList(),
                IsStarted = IsStarted,
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
                Speed = Settings.Speed,
                Name = Settings.Name
            };

            return summary;
        }

        private void TakeHostIfMissing(RoomPlayer player)
        {
            if (!HasPlayer(HostId))
            {
                HostId = player.Id;
            }
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
