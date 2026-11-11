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
    }
}
