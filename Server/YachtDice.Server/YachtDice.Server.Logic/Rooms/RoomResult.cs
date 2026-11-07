namespace YachtDice.Server.Logic.Rooms
{
    /// <summary>
    /// Outcome of an operation on a room and the state of the room after it.
    /// </summary>
    public class RoomResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoomResult"/> class.
        /// </summary>
        /// <param name="status">Outcome of the operation.</param>
        /// <param name="room">State of the room after the operation.</param>
        public RoomResult(RoomStatus status, RoomDetails room)
        {
            Status = status;
            Room = room;
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
        /// Creates the result of an operation that could not be done.
        /// </summary>
        /// <param name="status">Reason why the operation failed.</param>
        /// <returns>A result with an empty room.</returns>
        public static RoomResult CreateFailed(RoomStatus status)
        {
            return new RoomResult(status, RoomDetails.CreateEmpty());
        }
    }
}
