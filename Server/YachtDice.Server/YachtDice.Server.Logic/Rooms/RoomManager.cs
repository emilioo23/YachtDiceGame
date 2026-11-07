using System;
using System.Collections.Generic;

namespace YachtDice.Server.Logic.Rooms
{
    /// <summary>
    /// Keeps the rooms that are alive in the server. The room service, the chat service and the events of the
    /// channels use it at the same time. WCF only serializes the calls of each service, not those of different
    /// services, so every operation takes the lock before it reads or changes a room. A concurrent dictionary
    /// would only protect the list of rooms, not a step such as checking for space and adding the player.
    /// </summary>
    public class RoomManager : IRoomManager
    {
        private const int CodeLength = 6;
        private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        private const int MinCapacity = 2;
        private const int MaxCapacity = 4;
        private const int MinPlayersToStart = 2;
        private const string BlankNameMessage = "The name of the player cannot be empty.";

        private readonly object _syncRoot = new object();
        private readonly Dictionary<string, GameRoom> _rooms =
            new Dictionary<string, GameRoom>(StringComparer.OrdinalIgnoreCase);

        // Only used while _syncRoot is held, so it needs no lock of its own.
        private readonly Random _random = new Random();

        /// <inheritdoc />
        public RoomDetails CreateRoom(string hostName)
        {
            ValidateName(hostName);
            RoomDetails details;

            lock (_syncRoot)
            {
                var room = new GameRoom(CreateUniqueCode(), hostName, CreateDefaultSettings());

                _rooms[room.Code] = room;
                details = room.ToDetails();
            }

            return details;
        }

        /// <inheritdoc />
        public RoomResult Join(string roomCode, string playerName)
        {
            ValidateName(playerName);

            RoomResult result = ExecuteInRoom(roomCode, room => AddPlayer(room, playerName));

            return result;
        }

        /// <inheritdoc />
        public RoomResult Leave(string roomCode, string playerName)
        {
            RoomResult result = ExecuteInRoom(roomCode, room => TakeOutPlayer(room, playerName));

            return result;
        }

        /// <inheritdoc />
        public RoomResult UpdateSettings(string roomCode, string playerName, RoomSettings settings)
        {
            RoomResult result = ExecuteInRoom(roomCode, room => ChangeSettings(room, playerName, settings));

            return result;
        }

        /// <inheritdoc />
        public RoomResult StartGame(string roomCode, string playerName)
        {
            RoomResult result = ExecuteInRoom(roomCode, room => StartRoomGame(room, playerName));

            return result;
        }

        /// <inheritdoc />
        public bool IsMember(string roomCode, string playerName)
        {
            var isMember = false;

            lock (_syncRoot)
            {
                if (TryGetRoom(roomCode, out GameRoom room))
                {
                    isMember = room.HasPlayer(playerName);
                }
            }

            return isMember;
        }

        private static void ValidateName(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
            {
                throw new ArgumentException(BlankNameMessage, nameof(playerName));
            }
        }

        private static string NormalizeCode(string roomCode)
        {
            return (roomCode ?? string.Empty).Trim();
        }

        private static RoomSettings CreateDefaultSettings()
        {
            var settings = new RoomSettings
            {
                MaxPlayers = MaxCapacity,
                IsPrivate = true,
                Speed = GameSpeed.Normal
            };

            return settings;
        }

        private static RoomResult CreateResult(RoomStatus status, GameRoom room)
        {
            RoomResult result;

            if (status == RoomStatus.Success)
            {
                result = new RoomResult(status, room.ToDetails());
            }
            else
            {
                result = RoomResult.CreateFailed(status);
            }

            return result;
        }

        private static RoomResult AddPlayer(GameRoom room, string playerName)
        {
            RoomStatus status = GetJoinStatus(room, playerName);

            if (status == RoomStatus.Success)
            {
                room.Players.Add(playerName);
            }

            return CreateResult(status, room);
        }

        private static RoomStatus GetJoinStatus(GameRoom room, string playerName)
        {
            RoomStatus status = RoomStatus.Success;

            if (room.IsStarted)
            {
                status = RoomStatus.GameAlreadyStarted;
            }
            else if (room.HasPlayer(playerName))
            {
                status = RoomStatus.PlayerAlreadyInRoom;
            }
            else if (room.Players.Count >= room.Settings.MaxPlayers)
            {
                status = RoomStatus.RoomFull;
            }

            return status;
        }

        private static RoomResult ChangeSettings(GameRoom room, string playerName, RoomSettings settings)
        {
            RoomStatus status = GetSettingsStatus(room, playerName, settings);

            if (status == RoomStatus.Success)
            {
                room.Settings = new RoomSettings
                {
                    MaxPlayers = settings.MaxPlayers,
                    IsPrivate = settings.IsPrivate,
                    Speed = settings.Speed
                };
            }

            return CreateResult(status, room);
        }

        private static RoomStatus GetSettingsStatus(GameRoom room, string playerName, RoomSettings settings)
        {
            RoomStatus status = RoomStatus.Success;

            if (!room.IsHost(playerName))
            {
                status = RoomStatus.NotHost;
            }
            else if (room.IsStarted)
            {
                status = RoomStatus.GameAlreadyStarted;
            }
            else if (!AreValidSettings(room, settings))
            {
                status = RoomStatus.InvalidSettings;
            }

            return status;
        }

        private static bool AreValidSettings(GameRoom room, RoomSettings settings)
        {
            return settings != null
                && settings.MaxPlayers >= MinCapacity
                && settings.MaxPlayers <= MaxCapacity
                && settings.MaxPlayers >= room.Players.Count
                && Enum.IsDefined(typeof(GameSpeed), settings.Speed);
        }

        private static RoomResult StartRoomGame(GameRoom room, string playerName)
        {
            RoomStatus status = GetStartStatus(room, playerName);

            if (status == RoomStatus.Success)
            {
                room.IsStarted = true;
            }

            return CreateResult(status, room);
        }

        private static RoomStatus GetStartStatus(GameRoom room, string playerName)
        {
            RoomStatus status = RoomStatus.Success;

            if (!room.IsHost(playerName))
            {
                status = RoomStatus.NotHost;
            }
            else if (room.IsStarted)
            {
                status = RoomStatus.GameAlreadyStarted;
            }
            else if (room.Players.Count < MinPlayersToStart)
            {
                status = RoomStatus.NotEnoughPlayers;
            }

            return status;
        }

        private RoomResult ExecuteInRoom(string roomCode, Func<GameRoom, RoomResult> operation)
        {
            RoomResult result = RoomResult.CreateFailed(RoomStatus.RoomNotFound);

            lock (_syncRoot)
            {
                if (TryGetRoom(roomCode, out GameRoom room))
                {
                    result = operation(room);
                }
            }

            return result;
        }

        // The caller must hold _syncRoot.
        private RoomResult TakeOutPlayer(GameRoom room, string playerName)
        {
            RoomResult result = RoomResult.CreateFailed(RoomStatus.RoomNotFound);

            if (room.HasPlayer(playerName))
            {
                room.RemovePlayer(playerName);
                DeleteIfEmpty(room);
                result = new RoomResult(RoomStatus.Success, room.ToDetails());
            }

            return result;
        }

        // The caller must hold _syncRoot.
        private bool TryGetRoom(string roomCode, out GameRoom room)
        {
            return _rooms.TryGetValue(NormalizeCode(roomCode), out room);
        }

        // The caller must hold _syncRoot.
        private void DeleteIfEmpty(GameRoom room)
        {
            if (room.Players.Count == 0)
            {
                _rooms.Remove(room.Code);
            }
        }

        // The caller must hold _syncRoot.
        private string CreateUniqueCode()
        {
            string code;

            do
            {
                code = CreateRandomCode();
            }
            while (_rooms.ContainsKey(code));

            return code;
        }

        // The caller must hold _syncRoot.
        private string CreateRandomCode()
        {
            var characters = new char[CodeLength];

            for (int characterIndex = 0; characterIndex < CodeLength; characterIndex++)
            {
                characters[characterIndex] = CodeAlphabet[_random.Next(CodeAlphabet.Length)];
            }

            return new string(characters);
        }
    }
}
