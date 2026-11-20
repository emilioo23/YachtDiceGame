using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;

namespace YachtDice.Server.Tests.InMemory
{
    /// <summary>
    /// Keeps the expulsions in memory so the tests do not need the database. The names come from the players of
    /// the in-memory repository it receives.
    /// </summary>
    internal sealed class InMemoryKickLogRepository : IKickLogRepository
    {
        private readonly InMemoryPlayerRepository _players;
        private readonly List<KickLog> _kicks = new List<KickLog>();

        /// <summary>
        /// Initializes the repository with the players whose names are shown.
        /// </summary>
        /// <param name="players">In-memory players of the test.</param>
        public InMemoryKickLogRepository(InMemoryPlayerRepository players)
        {
            _players = players;
        }

        /// <summary>
        /// Gets the expulsions saved so far.
        /// </summary>
        public IReadOnlyList<KickLog> Kicks => _kicks;

        /// <inheritdoc />
        public void Add(KickLog kickLog)
        {
            _kicks.Add(kickLog);
        }

        /// <inheritdoc />
        public List<KickLogRecord> FindByPlayer(int playerId, int maxCount)
        {
            Func<KickLog, bool> hasPlayer = kick => kick.HostId == playerId || kick.TargetId == playerId;

            return _kicks
                .Where(hasPlayer)
                .OrderByDescending(kick => kick.Date)
                .Take(maxCount)
                .Select(ToRecord)
                .ToList();
        }

        private KickLogRecord ToRecord(KickLog kick)
        {
            return new KickLogRecord
            {
                HostName = FindName(kick.HostId),
                TargetName = FindName(kick.TargetId),
                RoomCode = kick.RoomCode,
                Date = kick.Date ?? DateTime.MinValue
            };
        }

        private string FindName(int playerId)
        {
            Func<Player, bool> isPlayer = player => player.Id == playerId;

            return _players.Players.First(isPlayer).DisplayName;
        }
    }
}
