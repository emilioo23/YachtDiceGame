namespace YachtDice.Server.Logic.Rooms
{
    /// <summary>
    /// Outcome of an operation on a room and the state of the room after it.
    /// </summary>
    public class RoomResult
    {
        private RoomResult(RoomStatus status, RoomDetails room, string playerId)
        {
            Status = status;
            Room = room;
            PlayerId = playerId;
        }

        /// <summary>
        /// Gets the outcome of the operation.
        /// </summary>
        public RoomStatus Status { get; }

        /// <summary>
        /// Gets the state of the room after the operation. It is empty if the operation failed.
        /// </summary>
        public RoomDetails Room { get; }

        /// <summary>
        /// Gets the identifier that the server gave to the player who entered the room. It is empty if the
        /// operation did not put a player inside a room.
        /// </summary>
        public string PlayerId { get; }

        /// <summary>
        /// Creates the result of an operation that could not be done.
        /// </summary>
        /// <param name="status">Reason why the operation failed.</param>
        /// <returns>A result with an empty room.</returns>
        public static RoomResult CreateFailed(RoomStatus status)
        {
            return new RoomResult(status, RoomDetails.CreateEmpty(), string.Empty);
        }

        /// <summary>
        /// Creates the result of an operation that was done and did not put a player inside a room.
        /// </summary>
        /// <param name="room">State of the room after the operation.</param>
        /// <returns>A successful result.</returns>
        public static RoomResult CreateSucceeded(RoomDetails room)
        {
            return new RoomResult(RoomStatus.Success, room, string.Empty);
        }

        /// <summary>
        /// Creates the result of a player who entered a room.
        /// </summary>
        /// <param name="room">State of the room after the player entered.</param>
        /// <param name="playerId">Identifier that the server gave to the player.</param>
        /// <returns>A successful result that carries the identifier of the player.</returns>
        public static RoomResult CreateEntered(RoomDetails room, string playerId)
        {
            return new RoomResult(RoomStatus.Success, room, playerId);
        }
    }
}
