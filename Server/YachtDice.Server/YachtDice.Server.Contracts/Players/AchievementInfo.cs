using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Players
{
    /// <summary>
    /// An achievement of the game and whether the player has unlocked it.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class AchievementInfo
    {
        /// <summary>
        /// Identifier of the achievement. It is also the identifier of its title.
        /// </summary>
        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// True if the player has already unlocked the achievement.
        /// </summary>
        [DataMember]
        public bool IsUnlocked { get; set; }

        /// <summary>
        /// True if unlocking the achievement also unlocks a title.
        /// </summary>
        [DataMember]
        public bool HasTitle { get; set; }
    }
}
