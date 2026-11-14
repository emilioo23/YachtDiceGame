using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Players
{
    /// <summary>
    /// Avatar, level and experience of a player, as seen by the client. The profile screen uses it to show
    /// which avatars the player can choose.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class PlayerProfile
    {
        /// <summary>
        /// Identifier of the avatar that the player uses now.
        /// </summary>
        [DataMember]
        public int AvatarId { get; set; }

        /// <summary>
        /// Level of the player.
        /// </summary>
        [DataMember]
        public int Level { get; set; }

        /// <summary>
        /// Experience points of the player.
        /// </summary>
        [DataMember]
        public int Experience { get; set; }

        /// <summary>
        /// Experience where the current level of the player starts.
        /// </summary>
        [DataMember]
        public int LevelStartExperience { get; set; }

        /// <summary>
        /// Experience where the next level starts. At the last level it is the same as the start of the level.
        /// </summary>
        [DataMember]
        public int NextLevelExperience { get; set; }
    }
}
