namespace YachtDice.Server.Logic.Avatars
{
    /// <summary>
    /// An avatar that the players can use, with the level a player needs to unlock it.
    /// </summary>
    public class AvatarInfo
    {
        /// <summary>
        /// Initializes the description of an avatar.
        /// </summary>
        /// <param name="id">Identifier of the avatar, the same that is saved in the database.</param>
        /// <param name="requiredLevel">Minimum level that a player needs to use the avatar.</param>
        public AvatarInfo(int id, int requiredLevel)
        {
            Id = id;
            RequiredLevel = requiredLevel;
        }

        /// <summary>
        /// Gets the identifier of the avatar.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets the minimum level that a player needs to use the avatar.
        /// </summary>
        public int RequiredLevel { get; }
    }
}
