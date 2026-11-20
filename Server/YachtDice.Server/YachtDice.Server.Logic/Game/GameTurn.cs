using System;
namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// A turn of the game of a room, identified by the code of the room and the number of the turn.
    /// </summary>
    public sealed class GameTurn
    {
        private const string NullRoomCodeMessage = "The code of the room cannot be null.";

        /// <summary>
        /// Initializes the turn.
        /// </summary>
        /// <param name="roomCode">Code of the room where the game is played.</param>
        /// <param name="turnNumber">Number of the turn inside the game.</param>
        public GameTurn(string roomCode, int turnNumber)
        {
            RoomCode = roomCode ?? throw new ArgumentNullException(nameof(roomCode), NullRoomCodeMessage);
            TurnNumber = turnNumber;
        }

        /// <summary>
        /// Gets the code of the room where the game is played.
        /// </summary>
        public string RoomCode { get; }

        /// <summary>
        /// Gets the number of the turn inside the game.
        /// </summary>
        public int TurnNumber { get; }
    }
}
