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
    /// abandoned and cannot be the winner. The players who finished the game also earn experience and, when
    /// they have enough, they go up a level.
    /// </summary>
    public sealed class GameResultRecorder : IGameResultRecorder
    {
        private const string NullSettingsMessage = "The settings of the room cannot be null.";
        private const string NullSnapshotMessage = "The state of the game cannot be null.";
        private const int NoExperience = 0;

        private readonly IPlayerRepository _playerRepository;
        private readonly IGameHistoryRepository _gameHistoryRepository;

        /// <summary>
        /// Initializes the recorder with the data access it needs.
        /// </summary>
        /// <param name="playerRepository">Data access used to find the accounts by their name.</param>
        /// <param name="gameHistoryRepository">Data access where the results are saved.</param>
        public GameResultRecorder(IPlayerRepository playerRepository, IGameHistoryRepository gameHistoryRepository)
        {
            _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
            _gameHistoryRepository = gameHistoryRepository
                ?? throw new ArgumentNullException(nameof(gameHistoryRepository));
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
            var savedAccounts = new Dictionary<int, Player>();

            foreach (PlayerSnapshot player in snapshot.Players)
            {
                AddIfRegistered(result, savedAccounts, player);
            }

            if (result.Players.Count > 0)
            {
                result.HostPlayerId = result.Players.First().PlayerId;
                _gameHistoryRepository.Save(result);
                AwardExperience(result, savedAccounts);
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

        private static PlayerResultRecord CreatePlayerResult(int playerId, PlayerSnapshot player)
        {
            PlayerResultStatus status = player.IsPresent ? PlayerResultStatus.Completed : PlayerResultStatus.Abandoned;

            return new PlayerResultRecord
            {
                PlayerId = playerId,
                FinalScore = player.Total,
                IsWinner = player.IsWinner && player.IsPresent,
                Status = status
            };
        }

        private void AddIfRegistered(
            GameResultRecord result,
            IDictionary<int, Player> savedAccounts,
            PlayerSnapshot player)
        {
            Player account = _playerRepository.FindByName(player.Name).FirstOrDefault();

            if (account != null && !savedAccounts.ContainsKey(account.Id))
            {
                savedAccounts.Add(account.Id, account);
                result.Players.Add(CreatePlayerResult(account.Id, player));
            }
        }

        private void AwardExperience(GameResultRecord result, IDictionary<int, Player> savedAccounts)
        {
            foreach (PlayerResultRecord playerResult in result.Players)
            {
                if (playerResult.Status == PlayerResultStatus.Completed)
                {
                    SaveProgress(savedAccounts[playerResult.PlayerId], playerResult);
                }
            }
        }

        private void SaveProgress(Player account, PlayerResultRecord playerResult)
        {
            int earnedExperience = PlayerProgression.CalculateExperience(
                playerResult.FinalScore,
                playerResult.IsWinner);
            int experience = (account.XP ?? NoExperience) + earnedExperience;

            _playerRepository.SaveProgress(account.Id, experience, PlayerProgression.GetLevel(experience));
        }
    }
}
