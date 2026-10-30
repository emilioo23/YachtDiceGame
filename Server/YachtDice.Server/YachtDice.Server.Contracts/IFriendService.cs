using System.Collections.Generic;
using System.ServiceModel;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Operations that let the players add friends, answer requests and see their friends.
    /// </summary>
    [ServiceContract(Namespace = ServiceNamespaces.Contracts)]
    public interface IFriendService
    {
        /// <summary>
        /// Sends a friend request to the player who owns the friend code.
        /// </summary>
        /// <param name="displayName">Name of the player who sends the request.</param>
        /// <param name="friendCode">Friend code of the player who will receive the request.</param>
        /// <returns>Outcome of the request.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        FriendRequestResultCode SendFriendRequest(string displayName, string friendCode);

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
        /// Gets the friend code that other players use to send a friend request to the player.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The friend code of the player, or an empty text if the player does not exist.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        string GetFriendCode(string displayName);

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
