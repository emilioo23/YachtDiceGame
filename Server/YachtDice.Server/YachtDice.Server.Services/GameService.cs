using System;
using System.ServiceModel;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts;

    /// <summary>
    /// Runs the turns of the games in progress and notifies the players of each room.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class GameService : Contracts.IGameService
    {
        private const int DiceCount = 5;
        private const int MinDieValue = 1;
        private const int MaxDieValue = 6;

        private readonly CallbackRegistry<Contracts.IGameCallback> _callbacks =
            new CallbackRegistry<Contracts.IGameCallback>();

        private readonly Random _random = new Random();

        /// <summary>
        /// Rolls the five dice and notifies every client in the room.
        /// </summary>
        /// <param name="roomId">Identifier of the room.</param>
        /// <param name="username">Username of the player who rolls.</param>
        public void RollDice(string roomId, string username)
        {
            int[] diceValues = CreateDiceValues();

            _callbacks.Register(roomId, OperationContext.Current.GetCallbackChannel<Contracts.IGameCallback>());
            _callbacks.Notify(roomId, callback => callback.DiceRolled(diceValues));
        }

        /// <summary>
        /// Scores a category and notifies every client in the room that the turn changed.
        /// </summary>
        /// <param name="roomId">Identifier of the room.</param>
        /// <param name="username">Username of the player who scores.</param>
        /// <param name="categoryId">Identifier of the selected category.</param>
        public void SelectScoreCategory(string roomId, string username, int categoryId)
        {
            _callbacks.Notify(roomId, callback => callback.TurnChanged(new Contracts.TurnDetails()));
        }

        private int[] CreateDiceValues()
        {
            var diceValues = new int[DiceCount];

            lock (_random)
            {
                for (int diceIndex = 0; diceIndex < DiceCount; diceIndex++)
                {
                    diceValues[diceIndex] = _random.Next(MinDieValue, MaxDieValue + 1);
                }
            }

            return diceValues;
        }
    }
}
