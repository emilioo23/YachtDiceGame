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
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="targetUsername">Username of the player who will receive the request.</param>
        /// <returns>Outcome of the request.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        FriendRequestResultCode SendFriendRequest(string sessionToken, string targetUsername);

        /// <summary>
        /// Accepts or rejects a friend request that the player received.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="answer">Name of the player who sent the request and whether it is accepted.</param>
        /// <returns>Outcome of the answer.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        FriendActionResultCode RespondToFriendRequest(string sessionToken, FriendRequestAnswer answer);

        /// <summary>
        /// Removes a friend from the list of the player.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="friendName">Name of the friend to remove.</param>
        /// <returns>Outcome of the removal.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        FriendActionResultCode RemoveFriend(string sessionToken, string friendName);

        /// <summary>
        /// Blocks another player: they stop being friends and cannot send requests to each other again.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="blockedName">Name of the player who is blocked.</param>
        /// <returns>Outcome of the block.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        FriendActionResultCode BlockPlayer(string sessionToken, string blockedName);

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
        /// Reports another player who finished a game with the player in the last day.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player who reports.</param>
        /// <param name="report">Name of the player who is reported, the reason and an optional comment.</param>
        /// <returns>Outcome of the report.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ReportResultCode ReportPlayer(string sessionToken, PlayerReport report);

        /// <summary>
        /// Gets the invitations to a room that the player received in the last day, from the newest to the oldest.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <returns>The invitations, or an empty list if there is none.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        List<ReceivedInvitation> GetReceivedInvitations(string sessionToken);

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
