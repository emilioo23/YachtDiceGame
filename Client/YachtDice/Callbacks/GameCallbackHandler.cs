using System;
using YachtDice.GameServiceReference;

namespace YachtDice.Callbacks
{
    /// <summary>
    /// Handles game related callbacks from the server.
    /// </summary>
    public class GameCallbackHandler : IGameManagerCallback
    {
        /// <summary>
        /// Event triggered when dice are rolled.
        /// </summary>
        public event Action<int[]> DiceRolled;

        /// <summary>
        /// Event triggered when a turn changes.
        /// </summary>
        public event Action<TurnDetails> TurnChanged;

        /// <summary>
        /// Invokes the dice rolled event.
        /// </summary>
        /// <param name="diceValues">Values of the rolled dice.</param>
        public void OnDiceRolled(int[] diceValues)
        {
            DiceRolled?.Invoke(diceValues);
        }

        /// <summary>
        /// Invokes the turn changed event.
        /// </summary>
        /// <param name="turnDetails">Details of the new turn.</param>
        public void OnTurnChanged(TurnDetails turnDetails)
        {
            TurnChanged?.Invoke(turnDetails);
        }
    }
}