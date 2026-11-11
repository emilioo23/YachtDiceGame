using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Players
{
    /// <summary>
    /// An avatar of the game and the level that a player needs to use it.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class UnlockableAvatar
    {
        /// <summary>
        /// Identifier of the avatar.
        /// </summary>
        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// Minimum level that a player needs to use the avatar.
        /// </summary>
        [DataMember]
        public int RequiredLevel { get; set; }
    }
}
