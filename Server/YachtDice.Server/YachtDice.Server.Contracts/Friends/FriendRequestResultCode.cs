using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Friends
{
    /// <summary>
    /// Outcome of the request to add a friend by its username, as seen by the client.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum FriendRequestResultCode
    {
        /// <summary>The request was sent and the other player has to accept it.</summary>
        [EnumMember]
        RequestSent,

        /// <summary>The other player had already sent a request, which the player has to accept.</summary>
        [EnumMember]
        RequestAlreadyReceived,

        /// <summary>The username is empty.</summary>
        [EnumMember]
        InvalidUsername,

        /// <summary>No player uses the username.</summary>
        [EnumMember]
        UsernameNotFound,

        /// <summary>The username belongs to the player who sends the request.</summary>
        [EnumMember]
        SelfRequest,

        /// <summary>The players are already friends.</summary>
        [EnumMember]
        AlreadyFriends,

        /// <summary>The player already sent a request that has not been answered.</summary>
        [EnumMember]
        AlreadyRequested,

        /// <summary>The player who sends the request does not exist.</summary>
        [EnumMember]
        PlayerNotFound
    }
}
