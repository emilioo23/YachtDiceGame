using System;
using System.Collections.Generic;
using GameCategory = YachtDice.GameServiceReference.ScoreCategory;
using YachtDice.GameServiceReference;
using YachtDice.PlayerServiceReference;
using YachtDice.Resources;
using YachtDice.RoomServiceReference;

namespace YachtDice.Utils
{
    /// <summary>
    /// Texts that tell the player what happened in the game: result of the actions, presence of the players and saved scorecards.
    /// </summary>
    public static class GameMessages
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

        /// <summary>
        /// Gets the notice for a player who is no longer inside the match.
        /// </summary>
        /// <param name="eventType">Event of the room that removed the player.</param>
        /// <param name="playerName">Name of the player.</param>
        /// <returns>The text to show in the chat.</returns>
        public static string GetDepartureMessage(RoomEventType eventType, string playerName)
        {
            string format;

            switch (eventType)
            {
                case RoomEventType.PlayerKicked:
                    format = Strings.Game_Chat_PlayerKicked;
                    break;
                default:
                    format = Strings.Game_Chat_PlayerLeft;
                    break;
            }

            return string.Format(format, playerName);
        }

        /// <summary>
        /// Gets the notice for a player who came back to the match.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        /// <returns>The text to show in the chat.</returns>
        public static string GetReturnMessage(string playerName)
        {
            return string.Format(Strings.Game_Chat_PlayerReturned, playerName);
        }

        /// <summary>
        /// Writes the title of a scorecard: the name of the player and the final points.
        /// </summary>
        /// <param name="scorecard">Scorecard that the server sent.</param>
        /// <returns>The title of the scorecard.</returns>
        public static string CreateTitle(SavedScorecard scorecard)
        {
            string playerName = string.IsNullOrEmpty(scorecard.PlayerName)
                ? Strings.History_ComputerOpponent
                : scorecard.PlayerName;

            return string.Format(Strings.Scorecard_TitleFormat, playerName, scorecard.Total);
        }

        /// <summary>
        /// Writes one line for each category and one for each bonus.
        /// </summary>
        /// <param name="scorecard">Scorecard that the server sent.</param>
        /// <returns>The lines in the order of the scorecard.</returns>
        public static List<string> CreateLines(SavedScorecard scorecard)
        {
            var lines = new List<string>();

            foreach (SavedScore score in scorecard.Scores ?? Array.Empty<SavedScore>())
            {
                string points = Strings.Scorecard_NotUsed;

                if (score.IsUsed)
                {
                    points = score.Points.ToString();
                }

                lines.Add(string.Format(Strings.Scorecard_LineFormat, ReadCategoryName(score.Category), points));
            }

            string upperBonus = scorecard.HasUpperBonus ? Strings.Scorecard_Yes : Strings.Scorecard_No;
            lines.Add(string.Format(Strings.Scorecard_UpperBonusFormat, upperBonus));
            lines.Add(string.Format(Strings.Scorecard_YachtBonusFormat, scorecard.YachtBonusCount));

            return lines;
        }

        private static string ReadCategoryName(string category)
        {
            string name = category;

            if (Enum.TryParse(category, out GameCategory gameCategory))
            {
                name = CategoryNames.GetName(gameCategory);
            }

            return name;
        }
    }
}
