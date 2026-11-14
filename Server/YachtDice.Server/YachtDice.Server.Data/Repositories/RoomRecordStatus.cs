namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Progress of a room, as it is stored in the database.
    /// </summary>
    public enum RoomRecordStatus
    {
        /// <summary>
        /// The room is waiting for the game to start.
        /// </summary>
        Waiting,

        /// <summary>
        /// The game of the room already finished.
        /// </summary>
        Finished
    }
}
