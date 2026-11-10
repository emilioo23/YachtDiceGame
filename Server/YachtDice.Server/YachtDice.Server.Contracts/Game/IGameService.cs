using System.ServiceModel;

namespace YachtDice.Server.Contracts.Game
{
    /// <summary>
    /// Operations that the clients use to play a game in progress. The server decides everything: the dice,
    /// the turns and the points.
    /// </summary>
    [ServiceContract(Namespace = ServiceNamespaces.Contracts, CallbackContract = typeof(IGameCallback))]
    public interface IGameService
    {
        /// <summary>
        /// Subscribes the player to the changes of the game of the room and gives the current state. A player
        /// who came back to a game in progress calls it again to see the game as it is.
        /// </summary>
        /// <param name="playerId">Identifier of the player who joins the game.</param>
        /// <param name="roomCode">Code of the room.</param>
        /// <returns>Whether the player joined and the state of the game.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        JoinGameResult JoinGame(string playerId, string roomCode);

        /// <summary>
        /// Rolls the dice of the turn. The first roll of a turn rolls the five dice and the next ones only
        /// roll the dice that are not held. The result reaches every player through the callback.
        /// </summary>
        /// <param name="playerId">Identifier of the player who rolls.</param>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="heldDice">One value for each die: true if the die keeps its value.</param>
        /// <returns>Outcome of the roll.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        GameResultCode RollDice(string playerId, string roomCode, bool[] heldDice);

        /// <summary>
        /// Writes the points of the current dice in a box and passes the turn to the next player.
        /// </summary>
        /// <param name="playerId">Identifier of the player who chooses the box.</param>
        /// <param name="roomCode">Code of the room.</param>
        /// <param name="category">Box to use.</param>
        /// <returns>Outcome of the move.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        GameResultCode SelectCategory(string playerId, string roomCode, ScoreCategory category);
    }
}
