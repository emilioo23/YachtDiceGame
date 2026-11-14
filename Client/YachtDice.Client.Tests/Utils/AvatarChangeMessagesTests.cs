using Xunit;
using YachtDice.PlayerServiceReference;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Client.Tests.Utils
{
    /// <summary>
    /// Messages shown when an avatar cannot be changed.
    /// </summary>
    public class AvatarChangeMessagesTests
    {
        private const int RequiredLevel = 5;
        private const int SecondLevel = 2;
        private const int NoLevel = 0;
        private const int UnknownResultCode = 99;

        [Fact]
        public void GetMessage_AvatarLocked_IncludesTheRequiredLevel()
        {
            string message = AvatarChangeMessages.GetMessage(AvatarChangeResultCode.AvatarLocked, RequiredLevel);

            Assert.Equal(string.Format(Strings.Profile_AvatarLocked, RequiredLevel), message);
        }

        [Fact]
        public void GetMessage_AvatarNotFound_ReturnsTheSaveError()
        {
            string message = AvatarChangeMessages.GetMessage(AvatarChangeResultCode.AvatarNotFound, RequiredLevel);

            Assert.Equal(Strings.Profile_AvatarSaveError, message);
        }

        [Fact]
        public void GetMessage_PlayerNotFound_ReturnsTheSaveError()
        {
            string message = AvatarChangeMessages.GetMessage(AvatarChangeResultCode.PlayerNotFound, RequiredLevel);

            Assert.Equal(Strings.Profile_AvatarSaveError, message);
        }

        [Fact]
        public void GetMessage_ResultCodeOutsideTheList_ReturnsTheSaveError()
        {
            string message = AvatarChangeMessages.GetMessage((AvatarChangeResultCode)UnknownResultCode, RequiredLevel);

            Assert.Equal(Strings.Profile_AvatarSaveError, message);
        }

        [Fact]
        public void GetMessage_AvatarLockedForTheSecondLevel_IncludesThatLevel()
        {
            string message = AvatarChangeMessages.GetMessage(AvatarChangeResultCode.AvatarLocked, SecondLevel);

            Assert.Equal(string.Format(Strings.Profile_AvatarLocked, SecondLevel), message);
        }

        [Fact]
        public void GetMessage_AvatarLockedWithoutLevel_IncludesTheZero()
        {
            string message = AvatarChangeMessages.GetMessage(AvatarChangeResultCode.AvatarLocked, NoLevel);

            Assert.Equal(string.Format(Strings.Profile_AvatarLocked, NoLevel), message);
        }

        [Fact]
        public void GetMessage_AvatarNotFoundWithALevel_IgnoresTheLevel()
        {
            string message = AvatarChangeMessages.GetMessage(AvatarChangeResultCode.AvatarNotFound, SecondLevel);

            Assert.Equal(Strings.Profile_AvatarSaveError, message);
        }
    }
}
