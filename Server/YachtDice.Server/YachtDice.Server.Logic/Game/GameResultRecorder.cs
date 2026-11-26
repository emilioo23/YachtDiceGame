using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Logging;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Rooms;

namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// Saves the result of a finished game in the history. Only the registered players are saved: the server
    /// looks for each name of the game among the accounts, and the names that do not belong to any account,
    /// like the guests, are left out. A player who was not in the game when it finished is saved as
    /// abandoned and cannot be the winner. Each saved player also gets the experience earned in the game;
    /// the level is not saved because it is calculated from the experience.
    /// </summary>
    public sealed class GameResultRecorder : IGameResultRecorder
    {
        private const int NoExperience = 0;
        private const int NoBonusPoints = 0;

        private readonly IPlayerRepository _playerRepository;
        private readonly IGameHistoryRepository _gameHistoryRepository;

        /// <summary>
        /// Initializes the recorder with the data access it needs.
        /// </summary>
        /// <param name="playerRepository">Data access used to find the accounts by their name.</param>
        /// <param name="gameHistoryRepository">Data access where the results are saved.</param>
        public GameResultRecorder(IPlayerRepository playerRepository, IGameHistoryRepository gameHistoryRepository)
        {
            _playerRepository = playerRepository
                ?? throw new ArgumentNullException(nameof(playerRepository), LogMessages.NullPlayerRepositoryMessage);
            _gameHistoryRepository = gameHistoryRepository
                ?? throw new ArgumentNullException(nameof(gameHistoryRepository),
                    LogMessages.NullGameHistoryRepositoryMessage);
        }

        /// <inheritdoc />
        public void Record(RoomSettings settings, GameSnapshot snapshot)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings), LogMessages.NullSettingsMessage);
            }

            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot), LogMessages.NullSnapshotMessage);
            }

            var result = new GameResultRecord
            {
                MaxPlayers = settings.MaxPlayers,
                IsPrivate = settings.IsPrivate,
                HasComputer = snapshot.Players.Any(player => player.IsComputer)
            };
            var savedPlayerIds = new HashSet<int>();

            Func<PlayerSnapshot, bool> isPerson = candidate => !candidate.IsComputer;

            foreach (PlayerSnapshot player in snapshot.Players.Where(isPerson))
            {
                AddIfRegistered(result, savedPlayerIds, player);
            }

            Func<PlayerSnapshot, bool> isComputer = candidate => candidate.IsComputer;

            foreach (PlayerSnapshot computer in snapshot.Players.Where(isComputer))
            {
                result.ComputerPlayers.Add(CreateComputerResult(computer));
            }

            if (result.Players.Count > 0)
            {
                result.HostPlayerId = result.Players.First().PlayerId;
                _gameHistoryRepository.Save(result);
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

        private static PlayerResultRecord CreateComputerResult(PlayerSnapshot computer)
        {
            return new PlayerResultRecord
            {
                FinalScore = computer.Total,
                IsWinner = computer.IsWinner,
                Status = PlayerResultStatus.Completed,
                HasUpperBonus = computer.UpperBonus > NoBonusPoints,
                YachtBonusCount = computer.YachtBonus / ScoreCard.YachtBonusPoints,
                Scores = computer.Scores.Select(ToScoreRecord).ToList()
            };
        }

        private static ScoreRecord ToScoreRecord(ScoreEntry entry)
        {
            return new ScoreRecord
            {
                CategoryName = entry.Category.ToString(),
                Points = entry.Points,
                IsUsed = entry.IsFilled
            };
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
                HasUpperBonus = player.UpperBonus > NoBonusPoints,
                YachtBonusCount = player.YachtBonus / ScoreCard.YachtBonusPoints,
                Scores = player.Scores.Select(ToScoreRecord).ToList()
            };
        }
    }
}
