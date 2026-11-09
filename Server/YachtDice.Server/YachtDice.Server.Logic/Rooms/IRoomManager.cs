namespace YachtDice.Server.Logic.Rooms
{
    /// <summary>
    /// Rules of the rooms: who is inside each one, who controls it and when its game can start. The players
    /// are recognized by the identifier that the manager gives them when they enter, never by their name.
    /// </summary>
    public interface IRoomManager
    {
        /// <summary>
        /// Creates a room with a new code and puts the player inside as its host.
        /// </summary>
        /// <param name="hostName">Name that the player who creates the room wants to show.</param>
        /// <returns>The state of the new room and the identifier of the host.</returns>
        /// <exception cref="System.ArgumentException">The name is empty.</exception>
        RoomResult CreateRoom(string hostName);

        /// <summary>
        /// Puts the player inside the room that has the code. If another player of the room already uses the
        /// name, a number is added to it.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="playerName">Name that the player who enters wants to show.</param>
        /// <returns>Whether the player entered, the state of the room and the identifier of the player.</returns>
        /// <exception cref="System.ArgumentException">The name is empty.</exception>
        RoomResult Join(string roomCode, string playerName);

        /// <summary>
        /// Takes the player out of the room. If nobody stays, the room is deleted.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="playerId">Identifier of the player who leaves.</param>
        /// <returns>Whether the player was inside and the state of the room after leaving.</returns>
        RoomResult Leave(string roomCode, string playerId);

        /// <summary>
        /// Changes the options of the room. Only the host can do it, and only before the game starts.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="playerId">Identifier of the player who asks for the change.</param>
        /// <param name="settings">New options of the room.</param>
        /// <returns>Outcome of the change and the state of the room.</returns>
        RoomResult UpdateSettings(string roomCode, string playerId, RoomSettings settings);

        /// <summary>
        /// Marks the game of the room as started. Only the host can do it and the room needs enough players.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="playerId">Identifier of the player who starts the game.</param>
        /// <returns>Outcome of the start and the state of the room.</returns>
        RoomResult StartGame(string roomCode, string playerId);

        /// <summary>
        /// Looks for a player inside a room.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="player">The player, or null if the room does not exist or the player is not inside.</param>
        /// <returns>True if the room exists and the player is inside.</returns>
        bool TryGetPlayer(string roomCode, string playerId, out RoomPlayer player);
    }
}
