using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace YachtDiceGame.Network.Contracts
{
    [ServiceContract]
    public interface ILeaderboardManager
    {
        [OperationContract]
        Task<List<PlayerProfile>> FetchTopScoresAsync();
    }
}