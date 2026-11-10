namespace YachtDice.Server.Logic.Rooms
{
    /// <summary>
    /// A player who asks to enter a room: the name he or she asked for and the avatar already looked up.
    /// The avatar is searched before the lock of the rooms is taken, so the lock never waits for the database.
    /// </summary>
    internal sealed class RoomApplicant
    {
        /// <summary>
        /// Initializes the applicant.
        /// </summary>
        /// <param name="name">Name that the player asked for.</param>
        /// <param name="avatarId">Identifier of the avatar of the player.</param>
        public RoomApplicant(string name, int avatarId)
        {
            Name = name;
            AvatarId = avatarId;
        }

        /// <summary>
        /// Gets the name that the player asked for.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the identifier of the avatar of the player.
        /// </summary>
        public int AvatarId { get; }
    }
}
