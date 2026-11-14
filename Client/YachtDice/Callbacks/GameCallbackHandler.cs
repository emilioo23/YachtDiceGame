using System;
using System.ServiceModel;
using YachtDice.GameServiceReference;

namespace YachtDice.Callbacks
{
    /// <summary>
    /// Handles the game callbacks from the server: every change of the dice, the turns and the scorecards.
    /// </summary>
    [CallbackBehavior(UseSynchronizationContext = false)]
    public class GameCallbackHandler : IGameServiceCallback
    {
        /// <summary>
        /// Event triggered when something changes in the game. It is raised from a network thread.
        /// </summary>
        public event Action<GameState, GameEventType> GameChanged;

        /// <summary>
        /// Raises the game changed event when the server reports a change.
        /// </summary>
        /// <param name="state">State of the game after the change.</param>
        /// <param name="eventType">What happened in the game.</param>
        void IGameServiceCallback.GameChanged(GameState state, GameEventType eventType)
        {
            GameChanged?.Invoke(state, eventType);
        }
    }
}
