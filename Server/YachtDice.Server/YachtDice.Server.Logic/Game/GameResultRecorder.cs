using System;
using System.Collections.Generic;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Rooms;

namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// Saves the result of a finished game in the history. Only the registered players are saved: the server
    /// looks for each name of the game among the accounts, and the names that do not belong to any account,
    /// like the guests, are left out. A player who was not in the game when it finished is saved as
    /// abandoned and cannot be the winner.
    /// </summary>
    public sealed class GameResultRecorder : IGameResultRecorder
    {
        private const string CompletedStatus = "Completed";
        private const string AbandonedStatus = "Abandoned";

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
                result.HostPlayerId = result.Players[0].PlayerId;
                _gameHistoryRepository.Save(result);
            }
        }

        private static PlayerResultRecord CreatePlayerResult(int playerId, PlayerSnapshot player)
        {
            return new PlayerResultRecord
            {
                PlayerId = playerId,
                FinalScore = player.Total,
                IsWinner = player.IsWinner && player.IsPresent,
                Status = player.IsPresent ? CompletedStatus : AbandonedStatus
            };
        }

        private void AddIfRegistered(GameResultRecord result, ISet<int> savedPlayerIds, PlayerSnapshot player)
        {
            List<Player> accounts = _playerRepository.FindByName(player.Name);

            if (accounts.Count > 0 && savedPlayerIds.Add(accounts[0].Id))
            {
                result.Players.Add(CreatePlayerResult(accounts[0].Id, player));
            }
        }
    }
}
