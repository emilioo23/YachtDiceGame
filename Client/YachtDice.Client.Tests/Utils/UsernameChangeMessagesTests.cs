using Xunit;
using YachtDice.PlayerServiceReference;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Client.Tests.Utils
{
    /// <summary>
    /// Messages that explain why a username could not be changed.
    /// </summary>
    public class UsernameChangeMessagesTests
    {
        private const int UnknownResultCode = 99;

        [Fact]
        public void GetMessage_InvalidUsername_ReturnsTheRulesOfTheUsername()
        {
            string message = UsernameChangeMessages.GetMessage(UsernameChangeResultCode.InvalidUsername);

            Assert.Equal(Strings.Auth_ErrorInvalidUsername, message);
        }

        [Fact]
        public void GetMessage_OffensiveUsername_ReturnsTheOffensiveWordMessage()
        {
            string message = UsernameChangeMessages.GetMessage(UsernameChangeResultCode.OffensiveUsername);

            Assert.Equal(Strings.Auth_ErrorOffensiveUsername, message);
        }

        [Fact]
        public void GetMessage_UsernameAlreadyTaken_ReturnsTheTakenMessage()
        {
            string message = UsernameChangeMessages.GetMessage(UsernameChangeResultCode.UsernameAlreadyTaken);

            Assert.Equal(Strings.Dialogs_UsernameTaken, message);
        }

        [Fact]
        public void GetMessage_PlayerNotFound_ReturnsTheSaveError()
        {
            string message = UsernameChangeMessages.GetMessage(UsernameChangeResultCode.PlayerNotFound);

            Assert.Equal(Strings.Profile_UsernameSaveError, message);
        }

        [Fact]
        public void GetMessage_ResultCodeOutsideTheList_ReturnsTheSaveError()
        {
            string message = UsernameChangeMessages.GetMessage((UsernameChangeResultCode)UnknownResultCode);

            Assert.Equal(Strings.Profile_UsernameSaveError, message);
        }
    }
}
