using System;
using YachtDice.GameServiceReference;

namespace YachtDice.Callbacks
{
    /// <summary>
    /// Handles game related callbacks from the server.
    /// </summary>
    public class GameCallbackHandler : IGameServiceCallback
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
        /// Raises the dice rolled event when the server reports a roll.
        /// </summary>
        /// <param name="diceValues">Values of the rolled dice.</param>
        void IGameServiceCallback.DiceRolled(int[] diceValues)
        {
            DiceRolled?.Invoke(diceValues);
        }

        /// <summary>
        /// Raises the turn changed event when the server reports a new turn.
        /// </summary>
        /// <param name="turnDetails">Details of the new turn.</param>
        void IGameServiceCallback.TurnChanged(TurnDetails turnDetails)
        {
            TurnChanged?.Invoke(turnDetails);
        }
    }
}
