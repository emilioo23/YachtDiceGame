using YachtDice.Server.Logic.Rooms;

namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// Keeps the result of the games that finish.
    /// </summary>
    public interface IGameResultRecorder
    {
        /// <summary>
        /// Saves the result of a finished game for the registered players who took part in it.
        /// </summary>
        /// <param name="settings">Settings of the room where the game was played.</param>
        /// <param name="snapshot">Final state of the game.</param>
        /// <exception cref="System.ArgumentNullException">The settings or the snapshot are null.</exception>
        /// <exception cref="Data.Exceptions.DataAccessException">The result could not be saved.</exception>
        void Record(RoomSettings settings, GameSnapshot snapshot);
    }
}
