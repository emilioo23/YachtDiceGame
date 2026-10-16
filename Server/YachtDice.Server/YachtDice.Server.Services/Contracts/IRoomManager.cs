using System.ServiceModel;
using System.Threading.Tasks;

namespace YachtDiceGame.Network.Contracts
{
    [ServiceContract(CallbackContract = typeof(IRoomCallback))]
    public interface IRoomManager
    {
        [OperationContract]
        Task JoinRoomAsync(PlayerProfile player, string roomId);

        [OperationContract]
        Task LeaveRoomAsync(string username, string roomId);
    }

    [ServiceContract]
    public interface IRoomCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnPlayerJoined(PlayerProfile player);

        [OperationContract(IsOneWay = true)]
        void OnPlayerLeft(string username);
    }
}