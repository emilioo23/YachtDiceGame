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
        /// Gets a value indicating whether the host of the room changed because the host left it.
        /// </summary>
        public bool HostChanged { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the player came back to a game in progress after leaving it.
        /// </summary>
        public bool HasReturned { get; private set; }

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
        /// Creates the result of a player who left a room.
        /// </summary>
        /// <param name="room">State of the room after the player left.</param>
        /// <param name="hostChanged">True if the player was the host and another player is the host now.</param>
        /// <returns>A successful result that tells whether the host changed.</returns>
        public static RoomResult CreateLeft(RoomDetails room, bool hostChanged)
        {
            return new RoomResult(RoomStatus.Success, room, string.Empty) { HostChanged = hostChanged };
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

        /// <summary>
        /// Creates the result of a player who came back to a game in progress.
        /// </summary>
        /// <param name="room">State of the room after the player came back.</param>
        /// <param name="playerId">Identifier that the player had before leaving.</param>
        /// <returns>A successful result that carries the identifier of the player.</returns>
        public static RoomResult CreateReturned(RoomDetails room, string playerId)
        {
            return new RoomResult(RoomStatus.Success, room, playerId) { HasReturned = true };
        }
    }
}
