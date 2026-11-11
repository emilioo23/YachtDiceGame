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
    }
}
