using System.Collections.Generic;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Data access operations for the expulsions of players from a room.
    /// </summary>
    public interface IKickLogRepository
    {
        /// <summary>
        /// Saves a new expulsion.
        /// </summary>
        /// <param name="kickLog">Expulsion with the host, the expelled player, the room code and the date.</param>
        void Add(KickLog kickLog);

        /// <summary>
        /// Finds the latest expulsions where the player was the host or the expelled player.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="maxCount">Most expulsions to return.</param>
        /// <returns>The expulsions from the newest to the oldest, or an empty list if there is none.</returns>
        List<KickLogRecord> FindByPlayer(int playerId, int maxCount);
    }
}
