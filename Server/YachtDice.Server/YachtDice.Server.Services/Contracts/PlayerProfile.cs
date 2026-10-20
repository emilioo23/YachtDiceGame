using System.Runtime.Serialization;

namespace YachtDice.Server.Services.Contracts
{
    /// <summary>
    /// Minimal public information of a player inside a room, as seen by the client.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class PlayerProfile
    {
        /// <summary>
        /// Username of the player.
        /// </summary>
        [DataMember]
        public string Username { get; set; }

        /// <summary>
        /// Identifier of the avatar chosen by the player.
        /// </summary>
        [DataMember]
        public int AvatarId { get; set; }

        /// <summary>
        /// Current score of the player in the room.
        /// </summary>
        [DataMember]
        public int Score { get; set; }
    }
}
