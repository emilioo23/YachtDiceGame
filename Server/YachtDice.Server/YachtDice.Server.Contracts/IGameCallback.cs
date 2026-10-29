using System.ServiceModel;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Notifications that the server sends back to a client subscribed to <see cref="IGameManager"/>.
    /// </summary>
    [ServiceContract(Namespace = ServiceNamespaces.Contracts)]
    public interface IGameCallback
    {
        /// <summary>
        /// Called when the dice have been rolled.
        /// </summary>
        /// <param name="diceValues">Value of each of the five dice.</param>
        [OperationContract(IsOneWay = true)]
        void DiceRolled(int[] diceValues);

        /// <summary>
        /// Called when the turn passes to another player.
        /// </summary>
        /// <param name="turnDetails">Information about the new current turn.</param>
        [OperationContract(IsOneWay = true)]
        void TurnChanged(TurnDetails turnDetails);
    }
}