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
        /// The game of the room is in progress and is saved so it can go on after a restart of the server.
        /// </summary>
        Playing,

        /// <summary>
        /// The game of the room already finished.
        /// </summary>
        Finished
    }
}
