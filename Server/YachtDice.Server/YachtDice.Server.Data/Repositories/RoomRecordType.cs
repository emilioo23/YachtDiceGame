namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Kind of a room, as it is stored in the database.
    /// </summary>
    public enum RoomRecordType
    {
        /// <summary>
        /// Any player can see the room in the list and join it.
        /// </summary>
        Public,

        /// <summary>
        /// Only the players who know the code of the room can join it.
        /// </summary>
        Private,

        /// <summary>
        /// At least one computer player took part in the game, so its results are counted apart.
        /// </summary>
        Computer
    }
}
