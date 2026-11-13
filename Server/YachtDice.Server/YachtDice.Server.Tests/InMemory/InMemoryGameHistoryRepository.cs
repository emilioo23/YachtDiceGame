using System.Collections.Generic;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;

namespace YachtDice.Server.Tests.InMemory
{
    /// <summary>
    /// Keeps the saved results of the games in memory so the tests do not need the database.
    /// </summary>
    internal sealed class InMemoryGameHistoryRepository : IGameHistoryRepository
    {
        private readonly List<GameResultRecord> _savedResults = new List<GameResultRecord>();

        /// <summary>
        /// Gets the results saved so far, in the order in which they were saved.
        /// </summary>
        public IReadOnlyList<GameResultRecord> SavedResults => _savedResults;

        /// <inheritdoc />
        public List<GameHistoryPlayer> FindByPlayerId(int playerId)
        {
            return new List<GameHistoryPlayer>();
        }

        /// <inheritdoc />
        public List<PlayerRankingRecord> GetRanking(int maxEntries)
        {
            return new List<PlayerRankingRecord>();
        }

        /// <inheritdoc />
        public void Save(GameResultRecord result)
        {
            _savedResults.Add(result);
        }
    }
}
