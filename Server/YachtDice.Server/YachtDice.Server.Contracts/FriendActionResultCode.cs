using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Outcome of answering a friend request or removing a friend, as seen by the client.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum FriendActionResultCode
    {
        /// <summary>The action was done.</summary>
        [EnumMember]
        Success,

        /// <summary>One of the players does not exist.</summary>
        [EnumMember]
        PlayerNotFound,

        /// <summary>There is no pending request or friendship between the players.</summary>
        [EnumMember]
        RelationNotFound
    }
}
