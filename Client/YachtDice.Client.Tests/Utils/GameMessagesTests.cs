using System.Collections.Generic;
using GameCategory = YachtDice.GameServiceReference.ScoreCategory;
using Xunit;
using YachtDice.GameServiceReference;
using YachtDice.PlayerServiceReference;
using YachtDice.Resources;
using YachtDice.RoomServiceReference;
using YachtDice.Utils;

namespace YachtDice.Client.Tests.Utils
{
    /// <summary>
    /// Texts of the results of the actions of the game, the presence of the players and the saved scorecards.
    /// </summary>
    public class GameMessagesTests
    {
        private const int UnknownCode = 99;
        private const string PlayerName = "Beto";
        private const int UnknownEvent = 99;
        private const string NameWithBraces = "{0}";
        private const int Total = 210;
        private const int YachtPoints = 50;
        private const int NoYachtBonus = 0;

        [Fact]
        public void GetMessage_NotYourTurn_ReturnsTheTurnMessage()
        {
            string message = GameMessages.GetMessage(GameResultCode.NotYourTurn);

            Assert.Equal(Strings.Game_Result_NotYourTurn, message);
        }

        [Fact]
        public void GetMessage_MustRollFirst_ReturnsTheRollHint()
        {
            string message = GameMessages.GetMessage(GameResultCode.MustRollFirst);

            Assert.Equal(Strings.Game_Dice_HintPress, message);
        }

        [Fact]
        public void GetMessage_UnknownCode_ReturnsTheUnexpectedError()
        {
            var unknownCode = (GameResultCode)UnknownCode;

            string message = GameMessages.GetMessage(unknownCode);

            Assert.Equal(Strings.Dialogs_UnexpectedError, message);
        }

        [Fact]
        public void GetMessage_GameNotFound_ReturnsTheMessageOfThatResult()
        {
            string message = GameMessages.GetMessage(GameResultCode.GameNotFound);

            Assert.Equal(Strings.Game_Result_GameNotFound, message);
        }

        [Fact]
        public void GetMessage_PlayerNotInGame_ReturnsTheMessageOfThatResult()
        {
            string message = GameMessages.GetMessage(GameResultCode.PlayerNotInGame);

            Assert.Equal(Strings.Game_Result_PlayerNotInGame, message);
        }

        [Fact]
        public void GetMessage_GameFinished_ReturnsTheMessageOfThatResult()
        {
            string message = GameMessages.GetMessage(GameResultCode.GameFinished);

            Assert.Equal(Strings.Game_Result_GameFinished, message);
        }

        [Fact]
        public void GetMessage_NoRollsLeft_ReturnsTheMessageOfThatResult()
        {
            string message = GameMessages.GetMessage(GameResultCode.NoRollsLeft);

            Assert.Equal(Strings.Game_Result_NoRollsLeft, message);
        }

        [Fact]
        public void GetMessage_CategoryFilled_ReturnsTheMessageOfThatResult()
        {
            string message = GameMessages.GetMessage(GameResultCode.CategoryFilled);

            Assert.Equal(Strings.Game_Result_CategoryFilled, message);
        }

        [Fact]
        public void GetMessage_CategoryNotAllowed_ReturnsTheMessageOfThatResult()
        {
            string message = GameMessages.GetMessage(GameResultCode.CategoryNotAllowed);

            Assert.Equal(Strings.Game_Result_CategoryNotAllowed, message);
        }

        [Fact]
        public void GetDepartureMessage_PlayerKicked_ReturnsTheKickedNotice()
        {
            string message = GameMessages.GetDepartureMessage(RoomEventType.PlayerKicked, PlayerName);

            Assert.Equal(string.Format(Strings.Game_Chat_PlayerKicked, PlayerName), message);
        }

        [Fact]
        public void GetDepartureMessage_PlayerDisconnected_ReturnsTheLeftNotice()
        {
            string message = GameMessages.GetDepartureMessage(RoomEventType.PlayerDisconnected, PlayerName);

            Assert.Equal(string.Format(Strings.Game_Chat_PlayerLeft, PlayerName), message);
        }

        [Fact]
        public void GetReturnMessage_PlayerName_ReturnsTheReturnNotice()
        {
            string message = GameMessages.GetReturnMessage(PlayerName);

            Assert.Equal(string.Format(Strings.Game_Chat_PlayerReturned, PlayerName), message);
        }

        [Fact]
        public void GetDepartureMessage_EventOutsideTheList_ReturnsTheLeftNotice()
        {
            var unknownEvent = (RoomEventType)UnknownEvent;

            string message = GameMessages.GetDepartureMessage(unknownEvent, PlayerName);

            Assert.Equal(string.Format(Strings.Game_Chat_PlayerLeft, PlayerName), message);
        }

        [Fact]
        public void GetDepartureMessage_KickedWithoutName_UsesAnEmptyName()
        {
            string message = GameMessages.GetDepartureMessage(RoomEventType.PlayerKicked, string.Empty);

            Assert.Equal(string.Format(Strings.Game_Chat_PlayerKicked, string.Empty), message);
        }

        [Fact]
        public void GetDepartureMessage_NullName_UsesAnEmptyName()
        {
            string message = GameMessages.GetDepartureMessage(RoomEventType.PlayerDisconnected, null);

            Assert.Equal(string.Format(Strings.Game_Chat_PlayerLeft, string.Empty), message);
        }

        [Fact]
        public void GetReturnMessage_EmptyName_UsesAnEmptyName()
        {
            string message = GameMessages.GetReturnMessage(string.Empty);

            Assert.Equal(string.Format(Strings.Game_Chat_PlayerReturned, string.Empty), message);
        }

        [Fact]
        public void GetReturnMessage_NameWithBraces_KeepsTheNameAsWritten()
        {
            string message = GameMessages.GetReturnMessage(NameWithBraces);

            Assert.Equal(string.Format(Strings.Game_Chat_PlayerReturned, NameWithBraces), message);
        }

        [Fact]
        public void CreateTitle_Scorecard_ShowsTheNameAndTheTotal()
        {
            var scorecard = new SavedScorecard { PlayerName = PlayerName, Total = Total };
            string titleExpected = string.Format(Strings.Scorecard_TitleFormat, PlayerName, Total);

            string title = GameMessages.CreateTitle(scorecard);

            Assert.Equal(titleExpected, title);
        }

        [Fact]
        public void CreateLines_UsedYachtWithoutBonus_WritesTheCategoryAndBothBonuses()
        {
            var scorecard = new SavedScorecard
            {
                Scores = new[]
                {
                    new SavedScore { Category = GameCategory.Yacht.ToString(), Points = YachtPoints, IsUsed = true }
                },
                HasUpperBonus = false,
                YachtBonusCount = NoYachtBonus
            };
            var linesExpected = new List<string>
            {
                string.Format(Strings.Scorecard_LineFormat, CategoryNames.GetName(GameCategory.Yacht), YachtPoints),
                string.Format(Strings.Scorecard_UpperBonusFormat, Strings.Scorecard_No),
                string.Format(Strings.Scorecard_YachtBonusFormat, NoYachtBonus)
            };

            List<string> lines = GameMessages.CreateLines(scorecard);

            Assert.Equal(linesExpected, lines);
        }
    }
}
