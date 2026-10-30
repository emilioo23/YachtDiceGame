using System.Runtime.Serialization;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Types of events that can occur regarding the other players: who is online and who is a friend.
    /// </summary>
    [DataContract(Namespace = ServiceNamespaces.Contracts)]
    public enum PresenceEventType
    {
        /// <summary>The player entered the game.</summary>
        [EnumMember]
        PlayerConnected,

        /// <summary>The player left the game or lost the connection.</summary>
        [EnumMember]
        PlayerDisconnected,

        /// <summary>The player sent a friend request to the receiver of the notification.</summary>
        [EnumMember]
        FriendRequestReceived,

        /// <summary>The player accepted a friend request, or sent one that completes a pending request.</summary>
        [EnumMember]
        FriendRequestAccepted,

        /// <summary>The player removed the receiver of the notification from the friends.</summary>
        [EnumMember]
        FriendRemoved
    }
}
