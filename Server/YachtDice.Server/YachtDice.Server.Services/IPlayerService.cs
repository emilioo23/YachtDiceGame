using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.Server.Services.Contracts;

namespace YachtDice.Server.Services
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
        /// <param name="request">Name of the player.</param>
        /// <returns>The statistics of the player.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        Task<PlayerStatisticsDto> GetStatisticsAsync(PlayerStatisticsRequestDto request);
    }
}