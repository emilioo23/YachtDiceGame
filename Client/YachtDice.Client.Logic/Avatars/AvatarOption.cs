namespace YachtDice.Client.Logic.Avatars
{
    /// <summary>
    /// An avatar as the profile window shows it to one player: free or locked, and chosen or not.
    /// </summary>
    public sealed class AvatarOption
    {
        /// <summary>
        /// Gets or sets the identifier of the avatar.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the level that the avatar needs.
        /// </summary>
        public int RequiredLevel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the player has reached the level that the avatar needs.
        /// </summary>
        public bool IsUnlocked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the avatar is the one that the player has chosen now.
        /// </summary>
        public bool IsSelected { get; set; }
    }
}
