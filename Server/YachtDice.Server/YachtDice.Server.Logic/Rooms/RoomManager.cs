using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Logic.Avatars;
using YachtDice.Server.Logic.Moderation;
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
        /// <summary>
        /// Minutes that a room waits to start its game when the server does not configure another time.
        /// </summary>
        public const int DefaultWaitingMinutes = 15;

        private const int CodeLength = 6;
        private const int FirstAlphabetIndex = 0;
        private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        private const int MinCapacity = 2;
        private const int MaxCapacity = 4;
        private const int MinPlayersToStart = 2;
        private const int ReadyWaitMinutes = 2;
        private const int MaxRoomNameLength = 30;
        private const int NoInfractions = 0;
        private const int ComputerLevel = 0;
        private const string ComputerName = "CPU";
        private const string BlankNameMessage = "The name of the player cannot be empty.";
        private const string NullDependencyMessage = "A dependency of the room manager cannot be null.";

        private readonly IAvatarLookup _avatarLookup;
        private readonly OffensiveWordFilter _wordFilter;
        private readonly object _syncRoot = new object();
        private readonly Dictionary<string, GameRoom> _rooms =
            new Dictionary<string, GameRoom>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes the manager without any room.
        /// </summary>
        /// <param name="avatarLookup">Tells the avatar of each player who enters a room.</param>
        /// <param name="wordFilter">Finds the offensive words in the names of the rooms.</param>
        public RoomManager(IAvatarLookup avatarLookup, OffensiveWordFilter wordFilter)
        {
            _avatarLookup = avatarLookup
                ?? throw new ArgumentNullException(nameof(avatarLookup), NullDependencyMessage);
            _wordFilter = wordFilter
                ?? throw new ArgumentNullException(nameof(wordFilter), NullDependencyMessage);
        }

        /// <summary>
        /// Gets or sets the minutes that a room waits to start its game before it is closed. The server reads it
        /// from its configuration.
        /// </summary>
        public int WaitingMinutes { get; set; } = DefaultWaitingMinutes;

        /// <inheritdoc />
        public RoomResult CreateRoom(string hostName)
        {
            RejectBlankName(hostName);
            RoomApplicant applicant = CreateApplicant(hostName);
            RoomResult result;

            lock (_syncRoot)
            {
                var room = new GameRoom(CreateUniqueCode(), CreateDefaultSettings(), DateTime.UtcNow);
                RoomPlayer host = room.AddPlayer(applicant);

                _rooms[room.Code] = room;
                result = RoomResult.CreateEntered(room.ToDetails(), host.Id);
            }

            return result;
        }

        /// <inheritdoc />
        public RoomResult Join(string roomCode, string playerName)
        {
            RejectBlankName(playerName);
            RoomApplicant applicant = CreateApplicant(playerName);

            RoomResult result = ExecuteInRoom(roomCode, room => AddPlayer(room, applicant));

            return result;
        }

        /// <inheritdoc />
        public RoomResult JoinListed(string playerName, RoomAccess roomAccess)
        {
            RejectBlankName(playerName);
            RoomApplicant applicant = CreateApplicant(playerName);
            RoomResult result = RoomResult.CreateFailed(RoomStatus.RoomNotFound);

            lock (_syncRoot)
            {
                if (TryGetRoomById(roomAccess.RoomId, out GameRoom gameRoom))
                {
                    result = JoinWithAccessCode(gameRoom, applicant, NormalizeCode(roomAccess.AccessCode));
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
            RejectBlankName(playerName);
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
        public RoomResult ForgetAbsentPlayer(RoomMember member)
        {
            RoomResult result = ExecuteInRoom(member.RoomCode, room => ForgetAbsent(room, member.PlayerId));

            return result;
        }

        /// <inheritdoc />
        public RoomResult UpdateSettings(RoomMember member, RoomSettings settings)
        {
            bool hasOffensiveName = settings?.Name != null && _wordFilter.ContainsOffensiveWord(settings.Name);
            RoomResult result = RoomResult.CreateFailed(RoomStatus.OffensiveRoomName);

            if (!hasOffensiveName)
            {
                result = ExecuteInRoom(member.RoomCode, room => ChangeSettings(room, member.PlayerId, settings));
            }

            return result;
        }

        /// <inheritdoc />
        public RoomResult StartGame(RoomMember member)
        {
            RoomResult result =
                ExecuteInRoom(member.RoomCode, room => StartRoomGame(room, member.PlayerId));

            return result;
        }

        /// <inheritdoc />
        public RoomResult SetReady(RoomMember member, bool isReady)
        {
            RoomResult result = ExecuteInRoom(member.RoomCode, room => ChangeReady(room, member.PlayerId, isReady));

            return result;
        }

        /// <inheritdoc />
        public int RegisterChatInfraction(RoomMember member)
        {
            var infractions = NoInfractions;

            lock (_syncRoot)
            {
                bool isInside = TryGetRoom(member.RoomCode, out GameRoom room) && room.HasPlayer(member.PlayerId);

                if (isInside)
                {
                    infractions = room.RegisterChatInfraction(member.PlayerId);
                }
            }

            return infractions;
        }

        /// <inheritdoc />
        public bool TryGetHost(string roomCode, out RoomPlayer host)
        {
            var isFound = false;

            host = null;

            lock (_syncRoot)
            {
                if (TryGetRoom(roomCode, out GameRoom room))
                {
                    isFound = room.TryGetPlayer(room.HostId, out host);
                }
            }

            return isFound;
        }

        /// <inheritdoc />
        public List<RoomDetails> CloseExpiredRooms(DateTime now)
        {
            DateTime oldestAllowed = now.AddMinutes(-WaitingMinutes);
            Func<GameRoom, bool> isExpired = room => !room.IsStarted && room.CreatedAt <= oldestAllowed;
            List<RoomDetails> closedRooms;

            lock (_syncRoot)
            {
                List<GameRoom> expiredRooms = _rooms.Values.Where(isExpired).ToList();

                foreach (GameRoom room in expiredRooms)
                {
                    _rooms.Remove(room.Code);
                }

                closedRooms = expiredRooms.Select(room => room.ToDetails()).ToList();
            }

            return closedRooms;
        }

        /// <inheritdoc />
        public RoomResult KickPlayer(RoomMember member, string targetPlayerId)
        {
            RoomResult result = ExecuteInRoom(
                member.RoomCode,
                room => ExpelPlayer(room, member.PlayerId, targetPlayerId));

            return result;
        }

        /// <inheritdoc />
        public RoomResult TransferHost(RoomMember member, string newHostId)
        {
            RoomResult result =
                ExecuteInRoom(member.RoomCode, room => ChangeHost(room, member.PlayerId, newHostId));

            return result;
        }

        /// <inheritdoc />
        public RoomResult AddComputer(RoomMember member, ComputerDifficulty difficulty)
        {
            RoomResult result =
                ExecuteInRoom(member.RoomCode, room => EnterComputer(room, member.PlayerId, difficulty));

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

        private static void RejectBlankName(string playerName)
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

            if (room.IsExpelled(applicant.Name))
            {
                result = RoomResult.CreateFailed(RoomStatus.PlayerExpelled);
            }
            else if (room.TryReturnPlayer(applicant.Name, out RoomPlayer returned))
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
                RoomPlayer player = room.AddPlayer(applicant);

                result = RoomResult.CreateEntered(room.ToDetails(), player.Id);
            }
            else
            {
                result = RoomResult.CreateFailed(status);
            }

            return result;
        }

        private static RoomResult EnterComputer(GameRoom room, string playerId, ComputerDifficulty difficulty)
        {
            RoomStatus status = RoomStatus.NotHost;

            if (room.IsHost(playerId))
            {
                status = GetJoinStatus(room);
            }

            if (status == RoomStatus.Success)
            {
                RoomPlayer computer =
                    room.AddPlayer(new RoomApplicant(ComputerName, AvatarCatalog.DefaultAvatarId, ComputerLevel));

                computer.IsComputer = true;
                computer.IsReady = true;
                computer.Difficulty = difficulty;
            }

            return CreateResult(status, room);
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

        private static RoomResult ChangeSettings(GameRoom room, string playerId, RoomSettings settings)
        {
            RoomStatus status = GetSettingsStatus(room, playerId, settings);

            if (status == RoomStatus.Success)
            {
                room.Settings = new RoomSettings
                {
                    MaxPlayers = settings.MaxPlayers,
                    IsPrivate = settings.IsPrivate,
                    Speed = settings.Speed,
                    Name = (settings.Name ?? string.Empty).Trim()
                };
            }

            return CreateResult(status, room);
        }

        private static RoomStatus GetSettingsStatus(GameRoom room, string playerId, RoomSettings settings)
        {
            RoomStatus status = RoomStatus.Success;

            if (!room.IsHost(playerId))
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
            bool isWithinCapacity = settings != null
                && settings.MaxPlayers >= MinCapacity
                && settings.MaxPlayers <= MaxCapacity;
            bool hasPlaceForPlayers = isWithinCapacity && settings.MaxPlayers >= room.Players.Count;
            bool hasValidName = settings?.Name == null || settings.Name.Trim().Length <= MaxRoomNameLength;

            return hasPlaceForPlayers && hasValidName && Enum.IsDefined(typeof(GameSpeed), settings.Speed);
        }

        private static RoomResult ChangeReady(GameRoom room, string playerId, bool isReady)
        {
            RoomStatus status = RoomStatus.Success;

            if (room.IsStarted)
            {
                status = RoomStatus.GameAlreadyStarted;
            }
            else if (!room.HasPlayer(playerId))
            {
                status = RoomStatus.PlayerNotFound;
            }
            else
            {
                room.SetReady(playerId, isReady);
            }

            return CreateResult(status, room);
        }

        private static RoomResult StartRoomGame(GameRoom room, string playerId)
        {
            DateTime waitedEnough = DateTime.UtcNow.AddMinutes(-ReadyWaitMinutes);
            Func<RoomPlayer, bool> isNotReady = player => !player.IsReady && !room.IsHost(player.Id);
            Func<RoomPlayer, bool> canStillConfirm = player => player.JoinedAt > waitedEnough;
            List<RoomPlayer> notReady = room.Players.Where(isNotReady).ToList();
            int waitedTooLong = notReady.Count(player => !canStillConfirm(player));
            RoomStatus status = GetStartStatus(room, playerId, waitedTooLong);
            RoomResult result = RoomResult.CreateFailed(status);

            if (status == RoomStatus.Success && notReady.Any(canStillConfirm))
            {
                result = RoomResult.CreateFailed(RoomStatus.PlayersNotReady);
            }
            else if (status == RoomStatus.Success)
            {
                result = StartWithoutPlayers(room, notReady);
            }

            return result;
        }

        private static RoomResult StartWithoutPlayers(GameRoom room, List<RoomPlayer> leftOutPlayers)
        {
            List<string> leftOutIds = leftOutPlayers.Select(player => player.Id).ToList();

            foreach (string leftOutId in leftOutIds)
            {
                room.RemovePlayer(leftOutId);
            }

            room.IsStarted = true;

            return RoomResult.CreateStarted(room.ToDetails(), leftOutIds);
        }

        private static RoomStatus GetStartStatus(GameRoom room, string playerId, int leftOutCount)
        {
            RoomStatus status = RoomStatus.Success;

            if (!room.IsHost(playerId))
            {
                status = RoomStatus.NotHost;
            }
            else if (room.IsStarted)
            {
                status = RoomStatus.GameAlreadyStarted;
            }
            else if (room.Players.Count - leftOutCount < MinPlayersToStart)
            {
                status = RoomStatus.NotEnoughPlayers;
            }

            return status;
        }

        private static RoomStatus GetHostActionStatus(GameRoom room, string playerId, string targetPlayerId)
        {
            RoomStatus status = RoomStatus.Success;

            if (!room.IsHost(playerId))
            {
                status = RoomStatus.NotHost;
            }
            else if (string.Equals(playerId, targetPlayerId, StringComparison.Ordinal))
            {
                status = RoomStatus.InvalidTarget;
            }
            else if (!room.HasPlayer(targetPlayerId))
            {
                status = RoomStatus.PlayerNotFound;
            }

            return status;
        }

        private static RoomResult ExpelPlayer(GameRoom room, string playerId, string targetPlayerId)
        {
            RoomStatus status = GetHostActionStatus(room, playerId, targetPlayerId);

            if (status == RoomStatus.Success)
            {
                room.Expel(targetPlayerId);
            }

            return CreateResult(status, room);
        }

        private static RoomResult ChangeHost(GameRoom room, string playerId, string newHostId)
        {
            RoomStatus status = GetHostActionStatus(room, playerId, newHostId);
            bool isComputer = room.TryGetPlayer(newHostId, out RoomPlayer newHost) && newHost.IsComputer;

            if (status == RoomStatus.Success && isComputer)
            {
                status = RoomStatus.InvalidTarget;
            }
            else if (status == RoomStatus.Success)
            {
                room.SetHost(newHostId);
            }

            return CreateResult(status, room);
        }

        private static RoomResult ForgetAbsent(GameRoom room, string playerId)
        {
            room.ForgetAbsentPlayer(playerId);

            return CreateResult(RoomStatus.Success, room);
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
            string trimmedName = playerName.Trim();

            int avatarId = _avatarLookup.GetAvatarId(trimmedName);
            int level = _avatarLookup.GetLevel(trimmedName);

            return new RoomApplicant(playerName, avatarId, level);
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
            if (!room.HasPeople)
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
