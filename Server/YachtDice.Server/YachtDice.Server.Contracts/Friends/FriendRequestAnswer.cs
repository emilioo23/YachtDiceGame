using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Friends
{
    /// <summary>
    /// Answer of a player to a friend request: who sent the request and whether it is accepted. Both values
    /// always travel together.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public class FriendRequestAnswer
    {
        /// <summary>
        /// Name of the player who sent the request.
        /// </summary>
        [DataMember]
        public string RequesterName { get; set; }

        /// <summary>
        /// True to accept the request, false to reject it.
        /// </summary>
        [DataMember]
        public bool IsAccepted { get; set; }
    }
}
