namespace YachtDice.Server.Logic.Avatars
{
    /// <summary>
    /// What the profile screen needs to know about a player to let them choose an avatar and see how much
    /// experience is left for the next level.
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
        /// Gets or sets the experience points that the player needed to reach the current level.
        /// </summary>
        public int LevelStartExperience { get; set; }

        /// <summary>
        /// Gets or sets the experience points that the player needs to reach the next level.
        /// </summary>
        public int NextLevelExperience { get; set; }
    }
}
