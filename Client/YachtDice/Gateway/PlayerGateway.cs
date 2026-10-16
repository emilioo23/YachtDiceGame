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
                { ServiceErrorCode.InvalidRequest, GatewayErrorCode.InvalidRequest }
            };

        /// <summary>
        /// Gets the game statistics of a player.
        /// </summary>
        /// <param name="request">Name of the player.</param>
        /// <returns>Statistics of the player.</returns>
        /// <exception cref="GatewayException">The request could not be completed.</exception>
        public Task<PlayerStatistics> GetStatisticsAsync(PlayerStatisticsQuery request)
        {
            return ExecuteAsync(client => client.GetStatisticsAsync(request));
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

        /// <summary>
        /// Gets the leaderboard ranking entries.
        /// </summary>
        /// <returns>A list of leaderboard entry DTOs.</returns>
        /// <exception cref="GatewayException">The request could not be completed.</exception>
        public async Task<List<LeaderboardEntry>> GetLeaderboardAsync()
        {
            using (var client = new PlayerServiceClient())
            {
                var leaderboardArray = await client.GetLeaderboardAsync();
                return leaderboardArray.ToList();
            }
        }
    }
}