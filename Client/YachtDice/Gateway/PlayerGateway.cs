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
    public class PlayerGateway : GatewayBase<PlayerServiceClient, IPlayerService>
    {
        /// <summary>
        /// Gets the game statistics of a player.
        /// </summary>
        /// <param name="username">Username of the player.</param>
        /// <returns>The statistics of the player.</returns>
        public async Task<PlayerStatistics> GetStatisticsAsync(string username)
        {
            var player = new Player { Username = username };

            PlayerStatistics statistics = await ExecuteAsync(client => client.GetPlayerStatistics(player));

            return statistics;
        }

        /// <summary>
        /// Gets the leaderboard ranking entries.
        /// </summary>
        /// <returns>The ranking, from the player with most points to the one with least.</returns>
        public async Task<List<LeaderboardEntry>> GetLeaderboardAsync()
        {
            LeaderboardEntry[] entries = await ExecuteAsync(client => client.GetLeaderboard());

            return entries.ToList();
        }

        /// <inheritdoc />
        protected override GatewayErrorCode GetErrorCode(FaultException fault)
        {
            var typedFault = fault as FaultException<ServiceFault>;

            return GatewayErrorCodeParser.Parse(typedFault?.Detail.Code);
        }
    }
}