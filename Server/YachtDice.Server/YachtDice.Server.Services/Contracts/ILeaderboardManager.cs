using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace YachtDice.Server.Services.Contracts
{
    /// <summary>
    /// Operations that the clients can use to fetch the top players.
    /// </summary>
    [ServiceContract(Namespace = ServiceNamespaces.Contracts)]
    public interface ILeaderboardManager
    {
        /// <summary>
        /// Gets the players with the highest scores.
        /// </summary>
        /// <returns>The top players, from highest to lowest score.</returns>
        [OperationContract]
        Task<List<PlayerProfile>> FetchTopScoresAsync();
    }
}
