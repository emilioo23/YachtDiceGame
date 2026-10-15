using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YachtDice.Server.Data.Repositories;

namespace YachtDice.Server.Logic.Players
{
    /// <summary>
    /// Builds the ranking of players from the results of the games played.
    /// </summary>
    public class LeaderboardService
    {
        private const int FirstPosition = 1;
        private const int MaxEntries = 100;
        private const string NullDependencyMessage = "The dependency cannot be null.";

        private readonly IGameHistoryRepository _gameHistoryRepository;

        /// <summary>
        /// Initializes the service with the data access it needs.
        /// </summary>
        /// <param name="gameHistoryRepository">Repository used to read the game results.</param>
        public LeaderboardService(IGameHistoryRepository gameHistoryRepository)
        {
            if (gameHistoryRepository == null)
            {
                throw new ArgumentNullException(nameof(gameHistoryRepository), NullDependencyMessage);
            }

            _gameHistoryRepository = gameHistoryRepository;
        }

        /// <summary>
        /// Gets the best players, ordered from the one with most points to the one with least.
        /// </summary>
        /// <returns>The ranking, or an empty list if there are no games played.</returns>
        public async Task<List<LeaderboardEntry>> GetTopPlayersAsync()
        {
            List<PlayerRankingRecord> records = await _gameHistoryRepository.GetRankingAsync(MaxEntries);

            return records
                .Select((record, index) => new LeaderboardEntry
                {
                    Position = index + FirstPosition,
                    DisplayName = record.DisplayName,
                    TotalPoints = record.TotalPoints,
                    Wins = record.Wins
                })
                .ToList();
        }
    }
}