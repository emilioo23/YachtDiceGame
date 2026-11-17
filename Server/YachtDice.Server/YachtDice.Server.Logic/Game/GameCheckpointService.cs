using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Rooms;

namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// Saves the games in progress of registered players after every move, so they can go on after a restart of
    /// the server. A state is saved only when it is newer than the last one that was sent to the database.
    /// </summary>
    public sealed class GameCheckpointService : IGameCheckpointService
    {
        private const string NullDependencyMessage = "A dependency of the game checkpoint service cannot be null.";
        private const string NullRoomMessage = "The room cannot be null.";
        private const string NullSnapshotMessage = "The state of the game cannot be null.";
        private const int RollsPerTurn = 3;
        private const int FirstDiePosition = 1;
        private const int NoPoints = 0;
        private const int NoVersion = 0;

        private readonly IPlayerRepository _playerRepository;
        private readonly IGameCheckpointRepository _checkpointRepository;
        private readonly object _syncRoot = new object();
        private readonly Dictionary<string, RoomSettings> _trackedRooms =
            new Dictionary<string, RoomSettings>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _savedVersions =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="GameCheckpointService"/> class.
        /// </summary>
        /// <param name="playerRepository">The accounts, to know whether every player is registered.</param>
        /// <param name="checkpointRepository">The repository where the games in progress are saved.</param>
        public GameCheckpointService(
            IPlayerRepository playerRepository,
            IGameCheckpointRepository checkpointRepository)
        {
            _playerRepository = playerRepository
                ?? throw new ArgumentNullException(nameof(playerRepository), NullDependencyMessage);
            _checkpointRepository = checkpointRepository
                ?? throw new ArgumentNullException(nameof(checkpointRepository), NullDependencyMessage);
        }

        /// <inheritdoc />
        public bool Track(RoomDetails room)
        {
            if (room == null)
            {
                throw new ArgumentNullException(nameof(room), NullRoomMessage);
            }

            Func<RoomPlayer, bool> isRegisteredPerson =
                player => !player.IsComputer && _playerRepository.FindByName(player.DisplayName).Any();
            bool canBeSaved = room.Players.Any() && room.Players.All(isRegisteredPerson);

            if (canBeSaved)
            {
                lock (_syncRoot)
                {
                    _trackedRooms[room.Code] = room.Settings;
                    _savedVersions[room.Code] = NoVersion;
                }
            }

            return canBeSaved;
        }

        /// <inheritdoc />
        public void Save(string roomCode, GameSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot), NullSnapshotMessage);
            }

            RoomSettings settings;
            bool mustSave;

            lock (_syncRoot)
            {
                mustSave = _trackedRooms.TryGetValue(roomCode, out settings)
                    && snapshot.Version > _savedVersions[roomCode]
                    && snapshot.Status == GameStatus.InProgress;

                if (mustSave)
                {
                    _savedVersions[roomCode] = snapshot.Version;
                }
            }

            if (mustSave)
            {
                _checkpointRepository.Save(CreateRecord(roomCode, settings, snapshot));
                RemoveIfForgotten(roomCode);
            }
        }

        /// <inheritdoc />
        public void Forget(string roomCode)
        {
            bool isTracked;

            lock (_syncRoot)
            {
                isTracked = _trackedRooms.Remove(roomCode);
                _savedVersions.Remove(roomCode);
            }

            if (isTracked)
            {
                _checkpointRepository.Remove(roomCode);
            }
        }

        /// <inheritdoc />
        public List<SavedGame> LoadAll()
        {
            List<SavedGame> games = _checkpointRepository.FindAll().Select(ToSavedGame).ToList();

            lock (_syncRoot)
            {
                foreach (SavedGame game in games)
                {
                    _trackedRooms[game.RoomCode] = game.Settings;
                    _savedVersions[game.RoomCode] = NoVersion;
                }
            }

            return games;
        }

        private void RemoveIfForgotten(string roomCode)
        {
            bool isForgotten;

            lock (_syncRoot)
            {
                isForgotten = !_trackedRooms.ContainsKey(roomCode);
            }

            if (isForgotten)
            {
                _checkpointRepository.Remove(roomCode);
            }
        }

        private static GameCheckpointRecord CreateRecord(string roomCode, RoomSettings settings, GameSnapshot snapshot)
        {
            Func<PlayerSnapshot, bool> isCurrentPlayer = player => player.PlayerId == snapshot.CurrentPlayerId;
            PlayerSnapshot currentPlayer = snapshot.Players.FirstOrDefault(isCurrentPlayer);
            var record = new GameCheckpointRecord
            {
                RoomCode = roomCode,
                RoomName = settings.Name,
                MaxPlayers = settings.MaxPlayers,
                IsPrivate = settings.IsPrivate,
                Speed = settings.Speed.ToString(),
                Players = snapshot.Players.Select(ToPlayerRecord).ToList()
            };

            if (currentPlayer != null)
            {
                record.CurrentPlayerName = currentPlayer.Name;
                record.RollsMade = RollsPerTurn - snapshot.RollsLeft;
                record.Dice = CreateDice(snapshot);
            }

            return record;
        }

        private static List<RolledDieRecord> CreateDice(GameSnapshot snapshot)
        {
            var dice = new List<RolledDieRecord>();

            for (int index = 0; index < snapshot.Dice.Count; index++)
            {
                dice.Add(new RolledDieRecord
                {
                    Position = index + FirstDiePosition,
                    Value = snapshot.Dice[index],
                    IsHeld = snapshot.HeldDice[index]
                });
            }

            return dice;
        }

        private static CheckpointPlayerRecord ToPlayerRecord(PlayerSnapshot player)
        {
            Func<ScoreEntry, bool> isFilled = entry => entry.IsFilled;

            return new CheckpointPlayerRecord
            {
                PlayerName = player.Name,
                HasForfeited = player.HasForfeited,
                HasUpperBonus = player.UpperBonus > NoPoints,
                YachtBonusCount = player.YachtBonus / ScoreCard.YachtBonusPoints,
                Scores = player.Scores.Where(isFilled).Select(ToScoreRecord).ToList()
            };
        }

        private static ScoreRecord ToScoreRecord(ScoreEntry entry)
        {
            return new ScoreRecord
            {
                CategoryName = entry.Category.ToString(),
                Points = entry.Points,
                IsUsed = true
            };
        }

        private static SavedGame ToSavedGame(GameCheckpointRecord record)
        {
            Enum.TryParse(record.Speed, out GameSpeed speed);
            var game = new SavedGame
            {
                RoomCode = record.RoomCode,
                Settings = new RoomSettings
                {
                    MaxPlayers = record.MaxPlayers,
                    IsPrivate = record.IsPrivate,
                    Speed = speed,
                    Name = record.RoomName ?? string.Empty
                },
                CurrentPlayerName = record.CurrentPlayerName ?? string.Empty,
                RollsMade = record.RollsMade
            };

            game.Players.AddRange(record.Players.Select(ToSavedPlayer));
            game.Dice.AddRange(record.Dice.Select(die => die.Value));
            game.HeldDice.AddRange(record.Dice.Select(die => die.IsHeld));

            return game;
        }

        private static SavedPlayer ToSavedPlayer(CheckpointPlayerRecord record)
        {
            var player = new SavedPlayer
            {
                Name = record.PlayerName,
                HasForfeited = record.HasForfeited,
                YachtBonusCount = record.YachtBonusCount
            };

            foreach (ScoreRecord score in record.Scores)
            {
                if (Enum.TryParse(score.CategoryName, out ScoreCategory category))
                {
                    player.Scores.Add(new ScoreEntry(category, true, score.Points));
                }
            }

            return player;
        }
    }
}
