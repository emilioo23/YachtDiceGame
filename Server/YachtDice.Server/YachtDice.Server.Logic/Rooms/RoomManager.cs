using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Logic.Avatars;
using YachtDice.Server.Logic.Security;

namespace YachtDice.Server.Logic.Rooms
{
    /// <summary>
    /// Keeps the rooms that are alive in the server. The room service, the chat service and the events of the
    /// channels use it at the same time. WCF only serializes the calls of each service, not those of different
    /// services, so every operation takes the lock before it reads or changes a room. A concurrent dictionary
    /// would only protect the list of rooms, not a step such as checking for space and adding the player.
    /// The private steps that receive a room only run while the lock is held. The codes of the rooms come from a
    /// secure generator, so a private room cannot be entered by guessing its code.
    /// </summary>
    public class RoomManager : IRoomManager
    {
        private const int CodeLength = 6;
        private const int FirstAlphabetIndex = 0;
        private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        private const int MinCapacity = 2;
        private const int MaxCapacity = 4;
        private const int MinPlayersToStart = 2;
        private const string BlankNameMessage = "The name of the player cannot be empty.";

        private readonly IAvatarLookup _avatarLookup;
        private readonly object _syncRoot = new object();
        private readonly Dictionary<string, GameRoom> _rooms =
            new Dictionary<string, GameRoom>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes the manager without any room.
        /// </summary>
        /// <param name="avatarLookup">Tells the avatar of each player who enters a room.</param>
        public RoomManager(IAvatarLookup avatarLookup)
        {
            _avatarLookup = avatarLookup
                ?? throw new ArgumentNullException(nameof(avatarLookup));
        }

        /// <inheritdoc />
        public RoomResult CreateRoom(string hostName)
        {
            ValidateName(hostName);
            RoomApplicant applicant = CreateApplicant(hostName);
            RoomResult result;

            lock (_syncRoot)
            {
                var room = new GameRoom(CreateUniqueCode(), CreateDefaultSettings());
                RoomPlayer host = room.AddPlayer(applicant.Name, applicant.AvatarId);

                _rooms[room.Code] = room;
                result = RoomResult.CreateEntered(room.ToDetails(), host.Id);
            }

            return result;
        }

        /// <inheritdoc />
        public RoomResult Join(string roomCode, string playerName)
        {
            ValidateName(playerName);
            RoomApplicant applicant = CreateApplicant(playerName);

            RoomResult result = ExecuteInRoom(roomCode, room => AddPlayer(room, applicant));

            return result;
        }

        /// <inheritdoc />
        public RoomResult JoinListed(string playerName, Room room)
        {
            ValidateName(playerName);
            RoomApplicant applicant = CreateApplicant(playerName);
            RoomResult result = RoomResult.CreateFailed(RoomStatus.RoomNotFound);

            lock (_syncRoot)
            {
                if (TryGetRoomById(room.RoomId, out GameRoom gameRoom))
                {
                    result = JoinWithAccessCode(gameRoom, applicant, NormalizeCode(room.AccessCode));
                }
            }

            return result;
        }

        /// <inheritdoc />
        public List<RoomSummary> GetOpenRooms()
        {
            List<RoomSummary> rooms;

            lock (_syncRoot)
            {
                Func<GameRoom, bool> isNotStarted = room => !room.IsStarted;
                rooms = _rooms.Values
                    .Where(isNotStarted)
                    .Select(room => room.ToSummary())
                    .OrderBy(summary => summary.HostName, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            return rooms;
        }

        /// <inheritdoc />
        public string FindPendingRoomCode(string playerName)
        {
            ValidateName(playerName);
            string code = string.Empty;

            lock (_syncRoot)
            {
                Func<GameRoom, bool> isPendingForPlayer = room => room.IsStarted && room.HasAbsentPlayer(playerName);
                GameRoom pending = _rooms.Values.FirstOrDefault(isPendingForPlayer);

                if (pending != null)
                {
                    code = pending.Code;
                }
            }

            return code;
        }

        /// <inheritdoc />
        public RoomResult Leave(RoomMember member)
        {
            RoomResult result = ExecuteInRoom(member.RoomCode, room => TakeOutPlayer(room, member.PlayerId));

            return result;
        }

        /// <inheritdoc />
        public RoomResult UpdateSettings(RoomMember member, RoomSettings settings)
        {
            RoomResult result =
                ExecuteInRoom(member.RoomCode, room => ChangeSettings(new RoomActor(room, member.PlayerId), settings));

            return result;
        }

        /// <inheritdoc />
        public RoomResult StartGame(RoomMember member)
        {
            RoomResult result =
                ExecuteInRoom(member.RoomCode, room => StartRoomGame(new RoomActor(room, member.PlayerId)));

            return result;
        }

        /// <inheritdoc />
        public RoomResult KickPlayer(RoomMember member, string targetPlayerId)
        {
            RoomResult result = ExecuteInRoom(
                member.RoomCode,
                room => ExpelPlayer(new RoomActor(room, member.PlayerId), targetPlayerId));

            return result;
        }

        /// <inheritdoc />
        public RoomResult TransferHost(RoomMember member, string newHostId)
        {
            RoomResult result =
                ExecuteInRoom(member.RoomCode, room => ChangeHost(new RoomActor(room, member.PlayerId), newHostId));

            return result;
        }

        /// <inheritdoc />
        public bool TryGetPlayer(RoomMember member, out RoomPlayer player)
        {
            var isFound = false;

            player = null;

            lock (_syncRoot)
            {
                if (TryGetRoom(member.RoomCode, out GameRoom room))
                {
                    isFound = room.TryGetPlayer(member.PlayerId, out player);
                }
            }

            return isFound;
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
                result = RoomResult.CreateSucceeded(room.ToDetails());
            }
            else
            {
                result = RoomResult.CreateFailed(status);
            }

            return result;
        }

        private static RoomResult AddPlayer(GameRoom room, RoomApplicant applicant)
        {
            RoomResult result;

            if (room.TryReturnPlayer(applicant.Name, out RoomPlayer returned))
            {
                result = RoomResult.CreateReturned(room.ToDetails(), returned.Id);
            }
            else
            {
                result = EnterNewPlayer(room, applicant);
            }

            return result;
        }

        private static RoomResult EnterNewPlayer(GameRoom room, RoomApplicant applicant)
        {
            RoomStatus status = GetJoinStatus(room);
            RoomResult result;

            if (status == RoomStatus.Success)
            {
                RoomPlayer player = room.AddPlayer(applicant.Name, applicant.AvatarId);

                result = RoomResult.CreateEntered(room.ToDetails(), player.Id);
            }
            else
            {
                result = RoomResult.CreateFailed(status);
            }

            return result;
        }

        private static RoomResult JoinWithAccessCode(GameRoom room, RoomApplicant applicant, string accessCode)
        {
            RoomResult result;

            if (room.Settings.IsPrivate && !room.HasCode(accessCode))
            {
                result = RoomResult.CreateFailed(RoomStatus.InvalidAccessCode);
            }
            else
            {
                result = AddPlayer(room, applicant);
            }

            return result;
        }

        private static RoomStatus GetJoinStatus(GameRoom room)
        {
            RoomStatus status = RoomStatus.Success;

            if (room.IsStarted)
            {
                status = RoomStatus.GameAlreadyStarted;
            }
            else if (room.Players.Count >= room.Settings.MaxPlayers)
            {
                status = RoomStatus.RoomFull;
            }

            return status;
        }

        private static RoomResult ChangeSettings(RoomActor actor, RoomSettings settings)
        {
            RoomStatus status = GetSettingsStatus(actor, settings);

            if (status == RoomStatus.Success)
            {
                actor.Room.Settings = new RoomSettings
                {
                    MaxPlayers = settings.MaxPlayers,
                    IsPrivate = settings.IsPrivate,
                    Speed = settings.Speed
                };
            }

            return CreateResult(status, actor.Room);
        }

        private static RoomStatus GetSettingsStatus(RoomActor actor, RoomSettings settings)
        {
            RoomStatus status = RoomStatus.Success;

            if (!actor.Room.IsHost(actor.PlayerId))
            {
                status = RoomStatus.NotHost;
            }
            else if (actor.Room.IsStarted)
            {
                status = RoomStatus.GameAlreadyStarted;
            }
            else if (!AreValidSettings(actor.Room, settings))
            {
                status = RoomStatus.InvalidSettings;
            }

            return status;
        }

        private static bool AreValidSettings(GameRoom room, RoomSettings settings)
        {
            bool isWithinCapacity = settings != null
                && settings.MaxPlayers >= MinCapacity
                && settings.MaxPlayers <= MaxCapacity;
            bool hasPlaceForPlayers = isWithinCapacity && settings.MaxPlayers >= room.Players.Count;

            return hasPlaceForPlayers && Enum.IsDefined(typeof(GameSpeed), settings.Speed);
        }

        private static RoomResult StartRoomGame(RoomActor actor)
        {
            RoomStatus status = GetStartStatus(actor);

            if (status == RoomStatus.Success)
            {
                actor.Room.IsStarted = true;
            }

            return CreateResult(status, actor.Room);
        }

        private static RoomStatus GetStartStatus(RoomActor actor)
        {
            RoomStatus status = RoomStatus.Success;

            if (!actor.Room.IsHost(actor.PlayerId))
            {
                status = RoomStatus.NotHost;
            }
            else if (actor.Room.IsStarted)
            {
                status = RoomStatus.GameAlreadyStarted;
            }
            else if (actor.Room.Players.Count < MinPlayersToStart)
            {
                status = RoomStatus.NotEnoughPlayers;
            }

            return status;
        }

        private static RoomStatus GetHostActionStatus(RoomActor actor, string targetPlayerId)
        {
            RoomStatus status = RoomStatus.Success;

            if (!actor.Room.IsHost(actor.PlayerId))
            {
                status = RoomStatus.NotHost;
            }
            else if (string.Equals(actor.PlayerId, targetPlayerId, StringComparison.Ordinal))
            {
                status = RoomStatus.InvalidTarget;
            }
            else if (!actor.Room.HasPlayer(targetPlayerId))
            {
                status = RoomStatus.PlayerNotFound;
            }

            return status;
        }

        private static RoomResult ExpelPlayer(RoomActor actor, string targetPlayerId)
        {
            RoomStatus status = GetHostActionStatus(actor, targetPlayerId);

            if (status == RoomStatus.Success)
            {
                actor.Room.RemovePlayer(targetPlayerId);
            }

            return CreateResult(status, actor.Room);
        }

        private static RoomResult ChangeHost(RoomActor actor, string newHostId)
        {
            RoomStatus status = GetHostActionStatus(actor, newHostId);

            if (status == RoomStatus.Success)
            {
                actor.Room.SetHost(newHostId);
            }

            return CreateResult(status, actor.Room);
        }

        private static void RemoveOrMarkAbsent(GameRoom room, string playerId)
        {
            if (room.IsStarted)
            {
                room.MarkAbsent(playerId);
            }
            else
            {
                room.RemovePlayer(playerId);
            }
        }

        private RoomApplicant CreateApplicant(string playerName)
        {
            return new RoomApplicant(playerName, _avatarLookup.GetAvatarId(playerName.Trim()));
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

        private RoomResult TakeOutPlayer(GameRoom room, string playerId)
        {
            RoomResult result = RoomResult.CreateFailed(RoomStatus.RoomNotFound);

            if (room.HasPlayer(playerId))
            {
                bool isHostLeaving = room.IsHost(playerId);

                RemoveOrMarkAbsent(room, playerId);
                DeleteIfEmpty(room);

                RoomDetails details = room.ToDetails();

                result = RoomResult.CreateLeft(details, isHostLeaving && !details.IsEmpty);
            }

            return result;
        }

        private bool TryGetRoomById(string roomId, out GameRoom room)
        {
            room = _rooms.Values.FirstOrDefault(
                candidate => string.Equals(candidate.Id, roomId, StringComparison.Ordinal));

            return room != null;
        }

        private bool TryGetRoom(string roomCode, out GameRoom room)
        {
            return _rooms.TryGetValue(NormalizeCode(roomCode), out room);
        }

        private void DeleteIfEmpty(GameRoom room)
        {
            if (room.Players.Count == 0)
            {
                _rooms.Remove(room.Code);
            }
        }

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

        private string CreateRandomCode()
        {
            var characters = new char[CodeLength];

            for (int characterIndex = 0; characterIndex < CodeLength; characterIndex++)
            {
                int alphabetIndex = SecureRandom.Next(FirstAlphabetIndex, CodeAlphabet.Length);

                characters[characterIndex] = CodeAlphabet[alphabetIndex];
            }

            return new string(characters);
        }
    }
}
