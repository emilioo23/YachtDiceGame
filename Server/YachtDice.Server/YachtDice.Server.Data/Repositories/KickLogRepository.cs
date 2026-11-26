using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using YachtDice.Server.Data.Exceptions;
using YachtDice.Server.Data.Logging;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Entity Framework implementation of the data access of the expulsions.
    /// </summary>
    public class KickLogRepository : IKickLogRepository
    {
        /// <summary>
        /// Saves a new expulsion.
        /// </summary>
        /// <param name="kickLog">Expulsion with the host, the expelled player, the room code and the date.</param>
        /// <exception cref="ArgumentNullException">The expulsion is null.</exception>
        /// <exception cref="DataAccessException">The expulsion could not be saved.</exception>
        public void Add(KickLog kickLog)
        {
            if (kickLog == null)
            {
                throw new ArgumentNullException(nameof(kickLog), LogMessages.NullKickMessage);
            }

            DatabaseOperation.Save(context => context.KickLog.Add(kickLog), LogMessages.KickLogUpdateFailedMessage);
        }

        /// <summary>
        /// Finds the latest expulsions where the player was the host or the expelled player.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="maxCount">Most expulsions to return.</param>
        /// <returns>The expulsions from the newest to the oldest, or an empty list if there is none.</returns>
        /// <exception cref="DataAccessException">The expulsions could not be read.</exception>
        public List<KickLogRecord> FindByPlayer(int playerId, int maxCount)
        {
            List<KickLogRecord> kicks = DatabaseOperation.Query(
                context => SelectByPlayer(context, playerId, maxCount),
                LogMessages.KickLogQueryFailedMessage);

            return kicks;
        }

        private static List<KickLogRecord> SelectByPlayer(YachtDiceContext context, int playerId, int maxCount)
        {
            Expression<Func<KickLog, bool>> hasPlayer = kick => kick.HostId == playerId || kick.TargetId == playerId;
            List<KickLogRecord> kicks = context.KickLog
                .Where(hasPlayer)
                .OrderByDescending(kick => kick.Date)
                .Take(maxCount)
                .Select(kick => new KickLogRecord
                {
                    HostName = kick.Player.DisplayName,
                    TargetName = kick.Player1.DisplayName,
                    RoomCode = kick.RoomCode,
                    Date = kick.Date ?? DateTime.MinValue
                })
                .ToList();

            return kicks;
        }
    }
}
