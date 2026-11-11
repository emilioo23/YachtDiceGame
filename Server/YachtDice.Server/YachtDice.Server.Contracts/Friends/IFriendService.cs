using System.Collections.Generic;
using System.ServiceModel;
using YachtDice.Server.Contracts.Rooms;

namespace YachtDice.Server.Contracts.Friends
{
    /// <summary>
    /// Operations that let the players add friends, answer requests and see their friends.
    /// </summary>
    [ServiceContract(Namespace = ServiceNamespaces.Contracts)]
    public interface IFriendService
    {
        /// <summary>
        /// Sends a friend request to the player who owns the username.
        /// </summary>
        /// <param name="displayName">Name of the player who sends the request.</param>
        /// <param name="targetUsername">Username of the player who will receive the request.</param>
        /// <returns>Outcome of the request.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        FriendRequestResultCode SendFriendRequest(string displayName, string targetUsername);

        /// <summary>
        /// Accepts or rejects a friend request that the player received.
        /// </summary>
        /// <param name="displayName">Name of the player who answers the request.</param>
        /// <param name="requesterName">Name of the player who sent the request.</param>
        /// <param name="isAccepted">True to accept the request, false to reject it.</param>
        /// <returns>Outcome of the answer.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        FriendActionResultCode RespondToFriendRequest(string displayName, string requesterName, bool isAccepted);

        /// <summary>
        /// Removes a friend from the list of the player.
        /// </summary>
        /// <param name="displayName">Name of the player who removes the friend.</param>
        /// <param name="friendName">Name of the friend to remove.</param>
        /// <returns>Outcome of the removal.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        FriendActionResultCode RemoveFriend(string displayName, string friendName);

        /// <summary>
        /// Invites a friend who is online to join the room where the player is.
        /// </summary>
        /// <param name="sender">Room and identifier of the player who invites.</param>
        /// <param name="friendName">Name of the friend to invite.</param>
        /// <returns>Whether the invitation reached the friend or why not.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        InvitationResultCode InviteToRoom(RoomMember sender, string friendName);

        /// <summary>
        /// Gets the username that other players use to send a friend request to the player.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The username of the player, or an empty text if the player does not exist.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        string GetUsername(string displayName);

        /// <summary>
        /// Gets the names of the friends of the player.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The names of the friends, or an empty list if the player has none.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        List<string> GetFriends(string displayName);

        /// <summary>
        /// Gets the names of the players who sent a friend request that the player has not answered.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The names of the players who sent a request, or an empty list if there is none.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        List<string> GetFriendRequests(string displayName);
    }
}
