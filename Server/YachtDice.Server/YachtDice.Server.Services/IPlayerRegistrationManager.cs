using System.ServiceModel;

namespace YachtDice.Server.Services.Contracts
{
    [ServiceContract(Namespace = ServiceNamespaces.Contracts)]
    public interface IPlayerRegistrationManager
    {
        [OperationContract]
        RegisterResultCode Register(PlayerRegistrationInfo request);
    }
}