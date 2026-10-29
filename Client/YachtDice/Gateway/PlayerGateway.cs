using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.PlayerServiceReference;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Single entry point of the client to the player operations of the server.
    /// </summary>
    public class PlayerGateway : GatewayBase<PlayerServiceClient>
    {
        private static readonly Dictionary<ServiceErrorCode, GatewayErrorCode> _errorCodes =
            new Dictionary<ServiceErrorCode, GatewayErrorCode>
            {
                { ServiceErrorCode.UnexpectedError, GatewayErrorCode.UnexpectedError },
                { ServiceErrorCode.DatabaseUnavailable, GatewayErrorCode.DatabaseUnavailable },
                { ServiceErrorCode.ValidationFailed, GatewayErrorCode.ValidationFailed }
            };

        /// <summary>
        /// Gets the game statistics of a player.
        /// </summary>
        /// <param name="username">Username of the player.</param>
        /// <returns>The statistics of the player.</returns>
        public Task<PlayerStatistics> GetStatisticsAsync(string username)
        {
            var player = new Player { Username = username };

            return ExecuteAsync(client => client.GetPlayerStatistics(player));
        }

        /// <summary>
        /// Gets the leaderboard ranking entries.
        /// </summary>
        /// <returns>The ranking, from the player with most points to the one with least.</returns>
        public Task<List<LeaderboardEntry>> GetLeaderboardAsync()
        {
            return ExecuteAsync(client => client.GetLeaderboard().ToList());
        }

        /// <inheritdoc />
        protected override GatewayErrorCode GetErrorCode(FaultException fault)
        {
            var typedFault = fault as FaultException<ServiceFault>;
            GatewayErrorCode errorCode;

            if (typedFault == null || !_errorCodes.TryGetValue(typedFault.Detail.Code, out errorCode))
            {
                errorCode = GatewayErrorCode.UnexpectedError;
            }

            return errorCode;
        }
    }
}