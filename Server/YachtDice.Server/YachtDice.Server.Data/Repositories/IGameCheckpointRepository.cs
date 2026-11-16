using System.Collections.Generic;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Data access of the games in progress that are saved so they can go on after a restart of the server.
    /// </summary>
    public interface IGameCheckpointRepository
    {
        /// <summary>
        /// Saves the current state of a game in progress, or creates it the first time.
        /// </summary>
        /// <param name="checkpoint">State of the game.</param>
        void Save(GameCheckpointRecord checkpoint);

        /// <summary>
        /// Gets every game in progress that was saved.
        /// </summary>
        /// <returns>The games, or an empty list if there is none.</returns>
        List<GameCheckpointRecord> FindAll();

        /// <summary>
        /// Removes a saved game in progress, for example because it finished or its room was closed.
        /// </summary>
        /// <param name="roomCode">Code of the room of the game.</param>
        void Remove(string roomCode);
    }
}
