using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.FriendServiceReference;
using FriendFault = YachtDice.FriendServiceReference.ServiceFault;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Single entry point of the client to the friend operations of the server.
    /// </summary>
    public class FriendGateway : GatewayBase<FriendServiceClient, IFriendService>
    {
        /// <summary>
        /// Sends a friend request to the player who owns the username.
        /// </summary>
        /// <param name="displayName">Name of the player who sends the request.</param>
        /// <param name="targetUsername">Username of the player who will receive the request.</param>
        /// <returns>Outcome of the request.</returns>
        public async Task<FriendRequestResultCode> SendRequestAsync(string displayName, string targetUsername)
        {
            FriendRequestResultCode result =
                await ExecuteAsync(client => client.SendFriendRequest(displayName, targetUsername));

            return result;
        }

        /// <summary>
        /// Accepts or rejects a friend request that the player received.
        /// </summary>
        /// <param name="displayName">Name of the player who answers the request.</param>
        /// <param name="requesterName">Name of the player who sent the request.</param>
        /// <param name="isAccepted">True to accept the request, false to reject it.</param>
        /// <returns>Outcome of the answer.</returns>
        public async Task<FriendActionResultCode> RespondAsync(
            string displayName,
            string requesterName,
            bool isAccepted)
        {
            FriendActionResultCode result =
                await ExecuteAsync(client => client.RespondToFriendRequest(displayName, requesterName, isAccepted));

            return result;
        }

        /// <summary>
        /// Removes a friend from the list of the player.
        /// </summary>
        /// <param name="displayName">Name of the player who removes the friend.</param>
        /// <param name="friendName">Name of the friend to remove.</param>
        /// <returns>Outcome of the removal.</returns>
        public async Task<FriendActionResultCode> RemoveFriendAsync(string displayName, string friendName)
        {
            FriendActionResultCode result = await ExecuteAsync(client => client.RemoveFriend(displayName, friendName));

            return result;
        }

        /// <summary>
        /// Invites a friend who is online to join the room where the player is.
        /// </summary>
        /// <param name="playerId">Identifier that the room gave to the player who invites.</param>
        /// <param name="friendName">Name of the friend to invite.</param>
        /// <param name="roomCode">Code of the room.</param>
        /// <returns>Whether the invitation reached the friend or why not.</returns>
        public async Task<InvitationResultCode> InviteToRoomAsync(string playerId, string friendName, string roomCode)
        {
            InvitationResultCode result =
                await ExecuteAsync(client => client.InviteToRoom(playerId, friendName, roomCode));

            return result;
        }

        /// <summary>
        /// Gets the username that other players use to add the player.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The username of the player.</returns>
        public async Task<string> GetUsernameAsync(string displayName)
        {
            string username = await ExecuteAsync(client => client.GetUsername(displayName));

            return username;
        }

        /// <summary>
        /// Gets the names of the friends of the player.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The names of the friends.</returns>
        public async Task<List<string>> GetFriendsAsync(string displayName)
        {
            string[] friends = await ExecuteAsync(client => client.GetFriends(displayName));

            return friends.ToList();
        }

        /// <summary>
        /// Gets the names of the players who sent a friend request that the player has not answered.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The names of the players who sent a request.</returns>
        public async Task<List<string>> GetRequestsAsync(string displayName)
        {
            string[] requesters = await ExecuteAsync(client => client.GetFriendRequests(displayName));

            return requesters.ToList();
        }

        /// <inheritdoc />
        protected override GatewayErrorCode GetErrorCode(FaultException fault)
        {
            var typedFault = fault as FaultException<FriendFault>;

            return GatewayErrorCodeParser.Parse(typedFault?.Detail.Code);
        }
    }
}
