using System.ServiceModel;
using System.Threading.Tasks;

namespace YachtDiceGame.Network.Contracts
{
    [ServiceContract(CallbackContract = typeof(IGameCallback))]
    public interface IGameManager
    {
        [OperationContract]
        Task RollDiceAsync(string roomId, string username);

        [OperationContract]
        Task SelectScoreCategoryAsync(string roomId, string username, int categoryId);
    }

    [ServiceContract]
    public interface IGameCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnDiceRolled(int[] diceValues);

        [OperationContract(IsOneWay = true)]
        void OnTurnChanged(TurnDetails turnDetails);
    }
}