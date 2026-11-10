using System;
using System.Collections.Generic;
using YachtDice.GameServiceReference;
using YachtDice.Resources;

namespace YachtDice.Utils
{
    /// <summary>
    /// Chooses the message shown to the player when an action of the game does not succeed.
    /// </summary>
    public static class GameResultMessages
    {
        private static readonly Dictionary<GameResultCode, Func<string>> MessageByResult =
            new Dictionary<GameResultCode, Func<string>>
            {
                { GameResultCode.GameNotFound, () => Strings.Game_Result_GameNotFound },
                { GameResultCode.PlayerNotInGame, () => Strings.Game_Result_PlayerNotInGame },
                { GameResultCode.NotYourTurn, () => Strings.Game_Result_NotYourTurn },
                { GameResultCode.GameFinished, () => Strings.Game_Result_GameFinished },
                { GameResultCode.NoRollsLeft, () => Strings.Game_Result_NoRollsLeft },
                { GameResultCode.MustRollFirst, () => Strings.Game_Dice_HintPress },
                { GameResultCode.InvalidHeldDice, () => Strings.Dialogs_UnexpectedError },
                { GameResultCode.CategoryFilled, () => Strings.Game_Result_CategoryFilled },
                { GameResultCode.CategoryNotAllowed, () => Strings.Game_Result_CategoryNotAllowed }
            };

        /// <summary>
        /// Gets the localized message that matches the outcome of the action.
        /// </summary>
        /// <param name="resultCode">Outcome reported by the server.</param>
        /// <returns>The text to show to the player.</returns>
        public static string GetMessage(GameResultCode resultCode)
        {
            Func<string> getText;

            if (!MessageByResult.TryGetValue(resultCode, out getText))
            {
                getText = () => Strings.Dialogs_UnexpectedError;
            }

            return getText();
        }
    }
}
