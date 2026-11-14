using System.ServiceModel;

namespace YachtDice.Server.Contracts.Game
{
    /// <summary>
    /// Notifications that the server sends back to a client subscribed to <see cref="IGameService"/>.
    /// </summary>
    [ServiceContract(Namespace = ServiceNamespaces.Contracts)]
    public interface IGameCallback
    {
        /// <summary>
        /// Called when something changes in the game. It always carries the whole state of the game, so the
        /// client only has to show it.
        /// </summary>
        /// <param name="state">State of the game after the change.</param>
        /// <param name="eventType">What happened in the game.</param>
        [OperationContract(IsOneWay = true)]
        void GameChanged(GameState state, GameEventType eventType);
    }
}
