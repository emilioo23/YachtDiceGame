namespace YachtDice.Server.Logic.Rooms
{
    /// <summary>
    /// The room and the player who asks to do something in it, which always travel together in the steps of
    /// the room manager.
    /// </summary>
    internal sealed class RoomActor
    {
        /// <summary>
        /// Initializes the actor.
        /// </summary>
        /// <param name="room">Room where the player wants to act.</param>
        /// <param name="playerId">Identifier of the player who wants to act.</param>
        public RoomActor(GameRoom room, string playerId)
        {
            Room = room;
            PlayerId = playerId;
        }

        /// <summary>
        /// Gets the room where the player wants to act.
        /// </summary>
        public GameRoom Room { get; }

        /// <summary>
        /// Gets the identifier of the player who wants to act.
        /// </summary>
        public string PlayerId { get; }
    }
}
