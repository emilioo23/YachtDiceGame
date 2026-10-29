using System.Collections.Generic;
using System.ServiceModel;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Operations that the clients can use to query the players' data.
    /// </summary>
    [ServiceContract(Namespace = ServiceNamespaces.Contracts)]
    public interface IPlayerService
    {
        /// <summary>
        /// Gets the statistics of a player.
        /// </summary>
        /// <param name="player">Player whose statistics are requested. Only the username is used.</param>
        /// <returns>The statistics of the player.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        PlayerStatistics GetPlayerStatistics(Player player);

        /// <summary>
        /// Gets the ranking of the best players.
        /// </summary>
        /// <returns>The ranking, from the player with most points to the one with least.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        List<LeaderboardEntry> GetLeaderboard();
    }
}