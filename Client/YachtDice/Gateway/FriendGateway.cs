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
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="targetUsername">Username of the player who will receive the request.</param>
        /// <returns>Outcome of the request.</returns>
        public async Task<FriendRequestResultCode> SendRequestAsync(string sessionToken, string targetUsername)
        {
            FriendRequestResultCode result =
                await ExecuteAsync(client => client.SendFriendRequest(sessionToken, targetUsername));

            return result;
        }

        /// <summary>
        /// Accepts or rejects a friend request that the player received.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="answer">Name of the player who sent the request and whether it is accepted.</param>
        /// <returns>Outcome of the answer.</returns>
        public async Task<FriendActionResultCode> RespondAsync(string sessionToken, FriendRequestAnswer answer)
        {
            FriendActionResultCode result =
                await ExecuteAsync(client => client.RespondToFriendRequest(sessionToken, answer));

            return result;
        }

        /// <summary>
        /// Removes a friend from the list of the player.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="friendName">Name of the friend to remove.</param>
        /// <returns>Outcome of the removal.</returns>
        public async Task<FriendActionResultCode> RemoveFriendAsync(string sessionToken, string friendName)
        {
            FriendActionResultCode result = await ExecuteAsync(client => client.RemoveFriend(sessionToken, friendName));

            return result;
        }

        /// <summary>
        /// Blocks another player: they stop being friends and cannot send requests to each other again.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="blockedName">Name of the player who is blocked.</param>
        /// <returns>Outcome of the block.</returns>
        public async Task<FriendActionResultCode> BlockPlayerAsync(string sessionToken, string blockedName)
        {
            FriendActionResultCode result = await ExecuteAsync(client => client.BlockPlayer(sessionToken, blockedName));

            return result;
        }

        /// <summary>
        /// Removes a block that the player made. The two players do not become friends again.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="blockedName">Name of the player who was blocked.</param>
        /// <returns>Outcome of the removal of the block.</returns>
        public async Task<FriendActionResultCode> UnblockPlayerAsync(string sessionToken, string blockedName)
        {
            FriendActionResultCode result =
                await ExecuteAsync(client => client.UnblockPlayer(sessionToken, blockedName));

            return result;
        }

        /// <summary>
        /// Gets the names of the players that the player blocked.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <returns>The names of the blocked players, or an empty list if there is none.</returns>
        public async Task<List<string>> GetBlockedPlayersAsync(string sessionToken)
        {
            string[] blockedNames = await ExecuteAsync(client => client.GetBlockedPlayers(sessionToken));

            return blockedNames.ToList();
        }

        /// <summary>
        /// Invites a friend who is online to join the room where the player is.
        /// </summary>
        /// <param name="sender">Room and identifier of the player who invites.</param>
        /// <param name="friendName">Name of the friend to invite.</param>
        /// <returns>Whether the invitation reached the friend or why not.</returns>
        public async Task<InvitationResultCode> InviteToRoomAsync(RoomMember sender, string friendName)
        {
            InvitationResultCode result = await ExecuteAsync(client => client.InviteToRoom(sender, friendName));

            return result;
        }

        /// <summary>
        /// Reports another player who finished a game with the player in the last day.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player who reports.</param>
        /// <param name="report">Name of the player who is reported, the reason and an optional comment.</param>
        /// <returns>Outcome of the report.</returns>
        public async Task<ReportResultCode> ReportPlayerAsync(string sessionToken, PlayerReport report)
        {
            ReportResultCode result = await ExecuteAsync(client => client.ReportPlayer(sessionToken, report));

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
        /// Gets the invitations to a room that the player received in the last day.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <returns>The invitations from the newest to the oldest.</returns>
        public async Task<List<ReceivedInvitation>> GetReceivedInvitationsAsync(string sessionToken)
        {
            ReceivedInvitation[] invitations =
                await ExecuteAsync(client => client.GetReceivedInvitations(sessionToken));

            return invitations.ToList();
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
