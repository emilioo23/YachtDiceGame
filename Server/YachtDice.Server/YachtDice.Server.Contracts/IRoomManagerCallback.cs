using System.ServiceModel;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Notificaciones que el servidor envía a los clientes sobre el estado de la sala.
    /// </summary>
    [ServiceContract(Namespace = ServiceNamespaces.Contracts)]
    public interface IRoomManagerCallback
    {
        /// <summary>
        /// Se llama cuando un nuevo jugador se une a la sala.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void OnPlayerJoined(PlayerProfile player);

        /// <summary>
        /// Se llama cuando un jugador abandona la sala.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void OnPlayerLeft(string username);
    }
}
