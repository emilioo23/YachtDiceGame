using System.ServiceModel;

namespace YachtDice.Server.Contracts
{
    /// <summary>
    /// Operations that the clients can use to play a turn of a game in progress.
    /// </summary>
    [ServiceContract(Namespace = ServiceNamespaces.Contracts, CallbackContract = typeof(IGameCallback))]
    public interface IGameService
    {
        /// <summary>
        /// Rolls the dice for the current player's turn.
        /// </summary>
        /// <param name="roomId">Identifier of the room.</param>
        /// <param name="username">Username of the player rolling the dice.</param>
        [OperationContract(IsOneWay = true)]
        void RollDice(string roomId, string username);

        /// <summary>
        /// Registers the score of the current turn in the given category.
        /// </summary>
        /// <param name="roomId">Identifier of the room.</param>
        /// <param name="username">Username of the player scoring the turn.</param>
        /// <param name="categoryId">Identifier of the chosen scoring category.</param>
        [OperationContract(IsOneWay = true)]
        void SelectScoreCategory(string roomId, string username, int categoryId);
    }
}
