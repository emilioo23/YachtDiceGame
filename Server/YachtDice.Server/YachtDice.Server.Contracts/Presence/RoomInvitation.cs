using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Presence
{
    /// <summary>
    /// Invitation that a player sends to a friend to join a room.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class RoomInvitation
    {
        /// <summary>
        /// Name that the screens show for the player who invites.
        /// </summary>
        [DataMember]
        public string SenderName { get; set; }

        /// <summary>
        /// Code of the room. With it the invited player can enter even if the room is private.
        /// </summary>
        [DataMember]
        public string RoomCode { get; set; }
    }
}
