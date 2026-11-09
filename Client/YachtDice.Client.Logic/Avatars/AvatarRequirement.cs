namespace YachtDice.Client.Logic.Avatars
{
    /// <summary>
    /// An avatar of the catalog of the server and the level that a player needs to use it.
    /// </summary>
    public sealed class AvatarRequirement
    {
        /// <summary>
        /// Gets or sets the identifier of the avatar.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the minimum level that a player needs to use the avatar.
        /// </summary>
        public int RequiredLevel { get; set; }
    }
}
