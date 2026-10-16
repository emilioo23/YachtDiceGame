using System.ServiceModel;
using System.Threading.Tasks;

namespace YachtDiceGame.Network.Contracts
{
    [ServiceContract(CallbackContract = typeof(IChatCallback))]
    public interface IChatManager
    {
        [OperationContract]
        Task SendMessageAsync(ChatMessage message, string roomId);
    }

    [ServiceContract]
    public interface IChatCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnMessageReceived(ChatMessage message);
    }
}