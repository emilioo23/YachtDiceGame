namespace YachtDice.Server.Logic.Avatars
{
    /// <summary>
    /// What the profile screen needs to know about a player to let them choose an avatar.
    /// </summary>
    public class PlayerProfile
    {
        /// <summary>
        /// Gets or sets the avatar that the player is using now.
        /// </summary>
        public int AvatarId { get; set; }

        /// <summary>
        /// Gets or sets the level of the player.
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Gets or sets the experience points of the player.
        /// </summary>
        public int Experience { get; set; }

        /// <summary>
        /// Gets or sets the experience where the current level of the player starts.
        /// </summary>
        public int LevelStartExperience { get; set; }

        /// <summary>
        /// Gets or sets the experience where the next level starts. At the last level it is the same as
        /// <see cref="LevelStartExperience"/>.
        /// </summary>
        public int NextLevelExperience { get; set; }
    }
}
