using Xunit;
using YachtDice.Client.Logic.Avatars;

namespace YachtDice.Client.Tests.Avatars
{
    /// <summary>
    /// Rules of the resource name that each avatar drawing uses.
    /// </summary>
    public class AvatarResourceKeyTests
    {
        private const int SingleDigitAvatarId = 5;
        private const int TwoDigitAvatarId = 10;
        private const string SingleDigitKey = "Avatar_05";
        private const string TwoDigitKey = "Avatar_10";

        [Fact]
        public void Create_SingleDigitIdentifier_PadsItWithAZero()
        {
            Assert.Equal(SingleDigitKey, AvatarResourceKey.Create(SingleDigitAvatarId));
        }

        [Fact]
        public void Create_TwoDigitIdentifier_KeepsBothDigits()
        {
            Assert.Equal(TwoDigitKey, AvatarResourceKey.Create(TwoDigitAvatarId));
        }
    }
}
