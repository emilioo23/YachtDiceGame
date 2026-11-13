using System;

namespace YachtDice.Client.Logic.Game
{
    /// <summary>
    /// Decides the phase of the turn and the hint that the window shows, from what the server sent.
    /// </summary>
    public static class TurnPhaseResolver
    {
        private const int NoRolls = 0;
        private const string NullPlayersMessage = "The players of the turn cannot be null.";
        private const string NullRollsMessage = "The rolls of the turn cannot be null.";

        /// <summary>
        /// Decides the phase of the turn.
        /// </summary>
        /// <param name="isFinished">True if the game is over.</param>
        /// <param name="players">The player in turn and the player who uses the window.</param>
        /// <returns>The phase that matches the state.</returns>
        /// <exception cref="ArgumentNullException">The players are null.</exception>
        public static TurnPhase ResolvePhase(bool isFinished, TurnPlayers players)
        {
            if (players == null)
            {
                throw new ArgumentNullException(nameof(players), NullPlayersMessage);
            }

            var phase = TurnPhase.OpponentTurn;

            if (isFinished)
            {
                phase = TurnPhase.Finished;
            }
            else if (string.IsNullOrEmpty(players.CurrentPlayerId))
            {
                phase = TurnPhase.Paused;
            }
            else if (players.CurrentPlayerId == players.LocalPlayerId)
            {
                phase = TurnPhase.LocalTurn;
            }

            return phase;
        }

        /// <summary>
        /// Decides the hint to show below the dice.
        /// </summary>
        /// <param name="phase">Phase of the turn.</param>
        /// <param name="rolls">Rolls that the current player has left and rolls that a turn gives.</param>
        /// <returns>The hint that matches the moment of the turn.</returns>
        /// <exception cref="ArgumentNullException">The rolls are null.</exception>
        public static TurnHint ResolveHint(TurnPhase phase, TurnRolls rolls)
        {
            if (rolls == null)
            {
                throw new ArgumentNullException(nameof(rolls), NullRollsMessage);
            }

            var hint = TurnHint.None;

            if (phase == TurnPhase.OpponentTurn)
            {
                hint = TurnHint.WaitOpponent;
            }
            else if (phase == TurnPhase.LocalTurn)
            {
                hint = ResolveLocalHint(rolls);
            }

            return hint;
        }

        private static TurnHint ResolveLocalHint(TurnRolls rolls)
        {
            var hint = TurnHint.ChooseOrRoll;

            if (rolls.RollsLeft >= rolls.MaxRolls)
            {
                hint = TurnHint.PressRoll;
            }
            else if (rolls.RollsLeft <= NoRolls)
            {
                hint = TurnHint.PickBox;
            }

            return hint;
        }
    }
}
