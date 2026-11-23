

namespace YachtDice.Client.Logic.Game
{
    /// <summary>
    /// Rules that decide the phase of the turn and when the yacht is celebrated.
    /// </summary>
    public static class TurnRules
    {
        private const int NoRolls = 0;
        private const int YachtPoints = 50;

        /// <summary>
        /// Decides the phase of the turn.
        /// </summary>
        /// <param name="isFinished">True if the game is over.</param>
        /// <param name="currentPlayerId">Identifier of the player in turn; empty if the game is paused.</param>
        /// <param name="localPlayerId">Identifier of the player who uses the window.</param>
        /// <returns>The phase that matches the state.</returns>
        public static TurnPhase ResolvePhase(bool isFinished, string currentPlayerId, string localPlayerId)
        {
            var phase = TurnPhase.OpponentTurn;

            if (isFinished)
            {
                phase = TurnPhase.Finished;
            }
            else if (string.IsNullOrEmpty(currentPlayerId))
            {
                phase = TurnPhase.Paused;
            }
            else if (currentPlayerId == localPlayerId)
            {
                phase = TurnPhase.LocalTurn;
            }

            return phase;
        }

        /// <summary>
        /// Decides the hint to show below the dice.
        /// </summary>
        /// <param name="phase">Phase of the turn.</param>
        /// <param name="rollsLeft">Rolls that the current player has left.</param>
        /// <param name="maxRolls">Rolls that a turn gives.</param>
        /// <returns>The hint that matches the moment of the turn.</returns>
        public static TurnHint ResolveHint(TurnPhase phase, int rollsLeft, int maxRolls)
        {
            var hint = TurnHint.None;

            if (phase == TurnPhase.OpponentTurn)
            {
                hint = TurnHint.WaitOpponent;
            }
            else if (phase == TurnPhase.LocalTurn)
            {
                hint = ResolveLocalHint(rollsLeft, maxRolls);
            }

            return hint;
        }

        /// <summary>
        /// Tells whether a change in the Yacht box deserves the celebration.
        /// </summary>
        /// <param name="isFilledBefore">Whether the Yacht box already had points before the change.</param>
        /// <param name="isFilled">Whether the Yacht box has points after the change.</param>
        /// <param name="points">Points of the Yacht box after the change.</param>
        /// <returns>True if the box was just filled with the fifty points of a Yacht.</returns>
        public static bool IsCelebrationDue(bool isFilledBefore, bool isFilled, int points)
        {
            bool isJustFilled = !isFilledBefore && isFilled;

            return isJustFilled && points == YachtPoints;
        }

        private static TurnHint ResolveLocalHint(int rollsLeft, int maxRolls)
        {
            var hint = TurnHint.ChooseOrRoll;

            if (rollsLeft >= maxRolls)
            {
                hint = TurnHint.PressRoll;
            }
            else if (rollsLeft <= NoRolls)
            {
                hint = TurnHint.PickBox;
            }

            return hint;
        }
    }
}
