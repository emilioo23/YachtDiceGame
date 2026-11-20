using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts.Friends
{
    /// <summary>
    /// Outcome of inviting a friend to a room, as seen by the client.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum InvitationResultCode
    {
        /// <summary>The invitation reached the friend.</summary>
        [EnumMember]
        Sent,

        /// <summary>The player who invites is not inside the room.</summary>
        [EnumMember]
        NotInRoom,

        /// <summary>The player and the invited player are not friends.</summary>
        [EnumMember]
        NotFriends,

        /// <summary>The friend is not online, so the invitation could not be delivered.</summary>
        [EnumMember]
        FriendOffline
    }
}
