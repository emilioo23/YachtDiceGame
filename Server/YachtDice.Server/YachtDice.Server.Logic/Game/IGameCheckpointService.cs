using System.Collections.Generic;
using YachtDice.Server.Logic.Rooms;

namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// Saves the games in progress so the players can go on with them after a restart of the server.
    /// </summary>
    public interface IGameCheckpointService
    {
        /// <summary>
        /// Starts saving the game of a room if every player has an account. Games with guests or computer players
        /// are not saved, because those players cannot be found again after the restart.
        /// </summary>
        /// <param name="room">Room whose game just started.</param>
        /// <returns>True if the game of the room will be saved.</returns>
        /// <exception cref="System.ArgumentNullException">The room is null.</exception>
        /// <exception cref="Data.Exceptions.DataAccessException">The accounts could not be checked.</exception>
        bool Track(RoomDetails room);

        /// <summary>
        /// Saves the current state of a game whose room is being saved. A finished game is not saved.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="snapshot">Current state of the game.</param>
        /// <exception cref="System.ArgumentNullException">The snapshot is null.</exception>
        /// <exception cref="Data.Exceptions.DataAccessException">The game could not be saved.</exception>
        void Save(string roomCode, GameSnapshot snapshot);

        /// <summary>
        /// Stops saving the game of a room and removes what was saved, for example because the game finished.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <exception cref="Data.Exceptions.DataAccessException">The saved game could not be removed.</exception>
        void Forget(string roomCode);

        /// <summary>
        /// Reads every saved game so the server can restore it, and keeps saving those games.
        /// </summary>
        /// <returns>The saved games, or an empty list if there is none.</returns>
        /// <exception cref="Data.Exceptions.DataAccessException">The saved games could not be read.</exception>
        List<SavedGame> LoadAll();
    }
}
