namespace YachtDice.Server.Logic.Rooms
{
    /// <summary>
    /// A player inside a room. The server decides the identifier when the player enters and uses it to
    /// recognize the player in every later operation. The display name is only a label for the screens: two
    /// players can choose the same name, but never share an identifier.
    /// </summary>
    public class RoomPlayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoomPlayer"/> class.
        /// </summary>
        /// <param name="id">Identifier that the server gave to the player.</param>
        /// <param name="displayName">Name that the screens show for the player.</param>
        public RoomPlayer(string id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }

        /// <summary>
        /// Gets the identifier that the server gave to the player. It is unique and it does not change.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the name that the screens show for the player. It is unique only inside its room.
        /// </summary>
        public string DisplayName { get; }
    }
}
