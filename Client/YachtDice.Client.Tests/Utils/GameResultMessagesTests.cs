using Xunit;
using YachtDice.GameServiceReference;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Client.Tests.Utils
{
    /// <summary>
    /// Messages shown when the server rejects an action of the game.
    /// </summary>
    public class GameResultMessagesTests
    {
        private const int UnknownCode = 99;

        [Fact]
        public void GetMessage_NotYourTurn_ReturnsTheTurnMessage()
        {
            string message = GameResultMessages.GetMessage(GameResultCode.NotYourTurn);

            Assert.Equal(Strings.Game_Result_NotYourTurn, message);
        }

        [Fact]
        public void GetMessage_MustRollFirst_ReturnsTheRollHint()
        {
            string message = GameResultMessages.GetMessage(GameResultCode.MustRollFirst);

            Assert.Equal(Strings.Game_Dice_HintPress, message);
        }

        [Fact]
        public void GetMessage_UnknownCode_ReturnsTheUnexpectedError()
        {
            var unknownCode = (GameResultCode)UnknownCode;

            string message = GameResultMessages.GetMessage(unknownCode);

            Assert.Equal(Strings.Dialogs_UnexpectedError, message);
        }

        [Fact]
        public void GetMessage_GameNotFound_ReturnsTheMessageOfThatResult()
        {
            string message = GameResultMessages.GetMessage(GameResultCode.GameNotFound);

            Assert.Equal(Strings.Game_Result_GameNotFound, message);
        }

        [Fact]
        public void GetMessage_PlayerNotInGame_ReturnsTheMessageOfThatResult()
        {
            string message = GameResultMessages.GetMessage(GameResultCode.PlayerNotInGame);

            Assert.Equal(Strings.Game_Result_PlayerNotInGame, message);
        }

        [Fact]
        public void GetMessage_GameFinished_ReturnsTheMessageOfThatResult()
        {
            string message = GameResultMessages.GetMessage(GameResultCode.GameFinished);

            Assert.Equal(Strings.Game_Result_GameFinished, message);
        }

        [Fact]
        public void GetMessage_NoRollsLeft_ReturnsTheMessageOfThatResult()
        {
            string message = GameResultMessages.GetMessage(GameResultCode.NoRollsLeft);

            Assert.Equal(Strings.Game_Result_NoRollsLeft, message);
        }

        [Fact]
        public void GetMessage_CategoryFilled_ReturnsTheMessageOfThatResult()
        {
            string message = GameResultMessages.GetMessage(GameResultCode.CategoryFilled);

            Assert.Equal(Strings.Game_Result_CategoryFilled, message);
        }

        [Fact]
        public void GetMessage_CategoryNotAllowed_ReturnsTheMessageOfThatResult()
        {
            string message = GameResultMessages.GetMessage(GameResultCode.CategoryNotAllowed);

            Assert.Equal(Strings.Game_Result_CategoryNotAllowed, message);
        }
    }
}
