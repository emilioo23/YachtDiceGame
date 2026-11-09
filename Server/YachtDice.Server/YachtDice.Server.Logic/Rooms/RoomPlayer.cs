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
        /// <param name="avatarId">Identifier of the avatar that the player uses.</param>
        public RoomPlayer(string id, string displayName, int avatarId)
        {
            Id = id;
            DisplayName = displayName;
            AvatarId = avatarId;
        }

        /// <summary>
        /// Gets the identifier that the server gave to the player. It is unique and it does not change.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the name that the screens show for the player. It is unique only inside its room.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets the identifier of the avatar that the player had when entering the room.
        /// </summary>
        public int AvatarId { get; }
    }
}
