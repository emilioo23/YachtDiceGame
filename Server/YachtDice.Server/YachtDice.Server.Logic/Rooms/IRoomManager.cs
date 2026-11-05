namespace YachtDice.Server.Logic.Rooms
{
    /// <summary>
    /// Rules of the rooms: who is inside each one, who controls it and when its game can start.
    /// </summary>
    public interface IRoomManager
    {
        /// <summary>
        /// Creates a room with a new code and puts the player inside as its host.
        /// </summary>
        /// <param name="hostName">Name of the player who creates the room.</param>
        /// <returns>The state of the new room.</returns>
        /// <exception cref="System.ArgumentException">The name is empty.</exception>
        RoomDetails CreateRoom(string hostName);

        /// <summary>
        /// Puts the player inside the room that has the code.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="playerName">Name of the player who enters.</param>
        /// <returns>Whether the player entered and the state of the room.</returns>
        /// <exception cref="System.ArgumentException">The name is empty.</exception>
        RoomResult Join(string roomCode, string playerName);

        /// <summary>
        /// Takes the player out of the room. If nobody stays, the room is deleted.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="playerName">Name of the player who leaves.</param>
        /// <returns>Whether the player was inside and the state of the room after leaving.</returns>
        RoomResult Leave(string roomCode, string playerName);

        /// <summary>
        /// Changes the options of the room. Only the host can do it, and only before the game starts.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="playerName">Name of the player who asks for the change.</param>
        /// <param name="settings">New options of the room.</param>
        /// <returns>Outcome of the change and the state of the room.</returns>
        RoomResult UpdateSettings(string roomCode, string playerName, RoomSettings settings);

        /// <summary>
        /// Marks the game of the room as started. Only the host can do it and the room needs enough players.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="playerName">Name of the player who starts the game.</param>
        /// <returns>Outcome of the start and the state of the room.</returns>
        RoomResult StartGame(string roomCode, string playerName);

        /// <summary>
        /// Indicates whether the player is inside the room.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="playerName">Name of the player.</param>
        /// <returns>True if the room exists and the player is inside.</returns>
        bool IsMember(string roomCode, string playerName);
    }
}
