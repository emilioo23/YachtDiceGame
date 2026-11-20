using YachtDice.GameServiceReference;
using YachtDice.Resources;

namespace YachtDice.Utils
{
    /// <summary>
    /// Chooses the message shown to the player when an action of the game does not succeed.
    /// </summary>
    public static class GameResultMessages
    {
        /// <summary>
        /// Gets the localized message that matches the outcome of the action.
        /// </summary>
        /// <param name="resultCode">Outcome reported by the server.</param>
        /// <returns>The text to show to the player.</returns>
        public static string GetMessage(GameResultCode resultCode)
        {
            string message;

            switch (resultCode)
            {
                case GameResultCode.GameNotFound:
                    message = Strings.Game_Result_GameNotFound;
                    break;
                case GameResultCode.PlayerNotInGame:
                    message = Strings.Game_Result_PlayerNotInGame;
                    break;
                case GameResultCode.NotYourTurn:
                    message = Strings.Game_Result_NotYourTurn;
                    break;
                case GameResultCode.GameFinished:
                    message = Strings.Game_Result_GameFinished;
                    break;
                case GameResultCode.NoRollsLeft:
                    message = Strings.Game_Result_NoRollsLeft;
                    break;
                case GameResultCode.MustRollFirst:
                    message = Strings.Game_Dice_HintPress;
                    break;
                case GameResultCode.CategoryFilled:
                    message = Strings.Game_Result_CategoryFilled;
                    break;
                case GameResultCode.CategoryNotAllowed:
                    message = Strings.Game_Result_CategoryNotAllowed;
                    break;
                default:
                    message = Strings.Dialogs_UnexpectedError;
                    break;
            }

            return message;
        }
    }
}
