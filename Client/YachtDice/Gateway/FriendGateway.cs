using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.FriendServiceReference;
using FriendErrorCode = YachtDice.FriendServiceReference.ServiceErrorCode;
using FriendFault = YachtDice.FriendServiceReference.ServiceFault;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Single entry point of the client to the friend operations of the server.
    /// </summary>
    public class FriendGateway : GatewayBase<FriendServiceClient>
    {
        private static readonly Dictionary<FriendErrorCode, GatewayErrorCode> _errorCodes =
            new Dictionary<FriendErrorCode, GatewayErrorCode>
            {
                { FriendErrorCode.UnexpectedError, GatewayErrorCode.UnexpectedError },
                { FriendErrorCode.DatabaseUnavailable, GatewayErrorCode.DatabaseUnavailable },
                { FriendErrorCode.ValidationFailed, GatewayErrorCode.ValidationFailed }
            };

        /// <summary>
        /// Sends a friend request to the player who owns the friend code.
        /// </summary>
        /// <param name="displayName">Name of the player who sends the request.</param>
        /// <param name="friendCode">Friend code of the player who will receive the request.</param>
        /// <returns>Outcome of the request.</returns>
        public Task<FriendRequestResultCode> SendRequestAsync(string displayName, string friendCode)
        {
            return ExecuteAsync(client => client.SendFriendRequest(displayName, friendCode));
        }

        /// <summary>
        /// Accepts or rejects a friend request that the player received.
        /// </summary>
        /// <param name="displayName">Name of the player who answers the request.</param>
        /// <param name="requesterName">Name of the player who sent the request.</param>
        /// <param name="isAccepted">True to accept the request, false to reject it.</param>
        /// <returns>Outcome of the answer.</returns>
        public Task<FriendActionResultCode> RespondAsync(string displayName, string requesterName, bool isAccepted)
        {
            return ExecuteAsync(client => client.RespondToFriendRequest(displayName, requesterName, isAccepted));
        }

        /// <summary>
        /// Removes a friend from the list of the player.
        /// </summary>
        /// <param name="displayName">Name of the player who removes the friend.</param>
        /// <param name="friendName">Name of the friend to remove.</param>
        /// <returns>Outcome of the removal.</returns>
        public Task<FriendActionResultCode> RemoveFriendAsync(string displayName, string friendName)
        {
            return ExecuteAsync(client => client.RemoveFriend(displayName, friendName));
        }

        /// <summary>
        /// Gets the friend code that other players use to add the player.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The friend code of the player.</returns>
        public Task<string> GetFriendCodeAsync(string displayName)
        {
            return ExecuteAsync(client => client.GetFriendCode(displayName));
        }

        /// <summary>
        /// Gets the names of the friends of the player.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The names of the friends.</returns>
        public Task<List<string>> GetFriendsAsync(string displayName)
        {
            return ExecuteAsync(client => client.GetFriends(displayName).ToList());
        }

        /// <summary>
        /// Gets the names of the players who sent a friend request that the player has not answered.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The names of the players who sent a request.</returns>
        public Task<List<string>> GetRequestsAsync(string displayName)
        {
            return ExecuteAsync(client => client.GetFriendRequests(displayName).ToList());
        }

        /// <inheritdoc />
        protected override GatewayErrorCode GetErrorCode(FaultException fault)
        {
            var typedFault = fault as FaultException<FriendFault>;
            GatewayErrorCode errorCode;

            if (typedFault == null || !_errorCodes.TryGetValue(typedFault.Detail.Code, out errorCode))
            {
                errorCode = GatewayErrorCode.UnexpectedError;
            }

            return errorCode;
        }
    }
}
