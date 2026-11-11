namespace YachtDice.Server.Logic.Rooms
{
    /// <summary>
    /// A player seen from the room where he or she plays: the code of the room and the identifier that the
    /// room gave to the player. Rooms, games and callback channels always need both values together.
    /// </summary>
    public sealed class RoomMember
    {
        /// <summary>
        /// Initializes the member.
        /// </summary>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="playerId">Identifier that the room gave to the player.</param>
        public RoomMember(string roomCode, string playerId)
        {
            RoomCode = roomCode;
            PlayerId = playerId;
        }

        /// <summary>
        /// Gets the code of the room.
        /// </summary>
        public string RoomCode { get; }

        /// <summary>
        /// Gets the identifier that the room gave to the player.
        /// </summary>
        public string PlayerId { get; }
    }
}
