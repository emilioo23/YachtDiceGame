using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Players;
using YachtDice.Server.Logic.Rooms;

namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// Saves the result of a finished game in the history. Only the registered players are saved: the server
    /// looks for each name of the game among the accounts, and the names that do not belong to any account,
    /// like the guests, are left out. A player who was not in the game when it finished is saved as
    /// abandoned and cannot be the winner. Each saved player also gets the experience earned in the game,
    /// and the level that matches the new experience.
    /// </summary>
    public sealed class GameResultRecorder : IGameResultRecorder
    {
        private const string NullSettingsMessage = "The settings of the room cannot be null.";
        private const string NullSnapshotMessage = "The state of the game cannot be null.";
        private const string NullPlayerRepositoryMessage = "The data access of the players cannot be null.";
        private const string NullGameHistoryRepositoryMessage = "The data access of the game history cannot be null.";
        private const string NullLevelTableMessage = "The table of levels cannot be null.";
        private const int NoExperience = 0;

        private readonly IPlayerRepository _playerRepository;
        private readonly IGameHistoryRepository _gameHistoryRepository;
        private readonly LevelTable _levelTable;

        /// <summary>
        /// Initializes the recorder with the data access it needs.
        /// </summary>
        /// <param name="playerRepository">Data access used to find the accounts by their name.</param>
        /// <param name="gameHistoryRepository">Data access where the results are saved.</param>
        /// <param name="levelTable">Experience that each level needs.</param>
        public GameResultRecorder(
            IPlayerRepository playerRepository,
            IGameHistoryRepository gameHistoryRepository,
            LevelTable levelTable)
        {
            _playerRepository = playerRepository
                ?? throw new ArgumentNullException(nameof(playerRepository), NullPlayerRepositoryMessage);
            _gameHistoryRepository = gameHistoryRepository
                ?? throw new ArgumentNullException(nameof(gameHistoryRepository), NullGameHistoryRepositoryMessage);
            _levelTable = levelTable ?? throw new ArgumentNullException(nameof(levelTable), NullLevelTableMessage);
        }

        /// <inheritdoc />
        public void Record(RoomSettings settings, GameSnapshot snapshot)
        {
            ValidateArguments(settings, snapshot);

            var result = new GameResultRecord
            {
                MaxPlayers = settings.MaxPlayers,
                IsPrivate = settings.IsPrivate
            };
            var savedPlayerIds = new HashSet<int>();

            foreach (PlayerSnapshot player in snapshot.Players)
            {
                AddIfRegistered(result, savedPlayerIds, player);
            }

            if (result.Players.Count > 0)
            {
                result.HostPlayerId = result.Players.First().PlayerId;
                _gameHistoryRepository.Save(result);
            }
        }

        private static void ValidateArguments(RoomSettings settings, GameSnapshot snapshot)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings), NullSettingsMessage);
            }

            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot), NullSnapshotMessage);
            }
        }

        private void AddIfRegistered(GameResultRecord result, ISet<int> savedPlayerIds, PlayerSnapshot player)
        {
            Player account = _playerRepository.FindByName(player.Name).FirstOrDefault();

            if (account != null && savedPlayerIds.Add(account.Id))
            {
                result.Players.Add(CreatePlayerResult(account, player));
            }
        }

        private PlayerResultRecord CreatePlayerResult(Player account, PlayerSnapshot player)
        {
            PlayerResultStatus status = player.IsPresent ? PlayerResultStatus.Completed : PlayerResultStatus.Abandoned;
            int experience = (account.XP ?? NoExperience) + player.ExperienceGained;

            return new PlayerResultRecord
            {
                PlayerId = account.Id,
                FinalScore = player.Total,
                IsWinner = player.IsWinner && player.IsPresent,
                Status = status,
                Experience = experience,
                Level = _levelTable.GetLevel(experience)
            };
        }
    }
}
