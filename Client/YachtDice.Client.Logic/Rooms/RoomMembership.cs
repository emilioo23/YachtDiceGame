using System;
namespace YachtDice.Client.Logic.Rooms
{
    /// <summary>
    /// The player and the room where the player is, which always travel together to use the services of a room.
    /// </summary>
    public sealed class RoomMembership
    {
        private const string NullValueMessage = "A value that the room membership needs cannot be null.";

        /// <summary>
        /// Initializes the membership.
        /// </summary>
        /// <param name="playerId">Identifier that the server gave to the player.</param>
        /// <param name="roomCode">Code of the room where the player is.</param>
        public RoomMembership(string playerId, string roomCode)
        {
            PlayerId = playerId ?? throw new ArgumentNullException(nameof(playerId), NullValueMessage);
            RoomCode = roomCode ?? throw new ArgumentNullException(nameof(roomCode), NullValueMessage);
        }

        /// <summary>
        /// Gets the identifier that the server gave to the player.
        /// </summary>
        public string PlayerId { get; }

        /// <summary>
        /// Gets the code of the room where the player is.
        /// </summary>
        public string RoomCode { get; }
    }
}
