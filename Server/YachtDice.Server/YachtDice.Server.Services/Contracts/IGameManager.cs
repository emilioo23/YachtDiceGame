using System.ServiceModel;
using System.Threading.Tasks;

namespace YachtDice.Server.Services.Contracts
{
    /// <summary>
    /// Operations that the clients can use to play a turn of a game in progress.
    /// </summary>
    [ServiceContract(Namespace = ServiceNamespaces.Contracts, CallbackContract = typeof(IGameCallback))]
    public interface IGameManager
    {
        /// <summary>
        /// Rolls the dice for the current player's turn.
        /// </summary>
        /// <param name="roomId">Identifier of the room.</param>
        /// <param name="username">Username of the player rolling the dice.</param>
        /// <returns>A task that completes when the roll has been broadcast.</returns>
        [OperationContract]
        Task RollDiceAsync(string roomId, string username);

        /// <summary>
        /// Registers the score of the current turn in the given category.
        /// </summary>
        /// <param name="roomId">Identifier of the room.</param>
        /// <param name="username">Username of the player scoring the turn.</param>
        /// <param name="categoryId">Identifier of the chosen scoring category.</param>
        /// <returns>A task that completes when the score has been registered.</returns>
        [OperationContract]
        Task SelectScoreCategoryAsync(string roomId, string username, int categoryId);
    }
}
