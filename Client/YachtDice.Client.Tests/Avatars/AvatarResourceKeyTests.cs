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
        private const string FirstAvatarKey = "Avatar_01";
        private const int NineAvatarId = 9;
        private const string NineAvatarKey = "Avatar_09";
        private const int NinetyNineAvatarId = 99;
        private const string NinetyNineAvatarKey = "Avatar_99";
        private const int NoAvatarId = 0;
        private const string NoAvatarKey = "Avatar_00";
        private const int HundredAvatarId = 100;
        private const string HundredAvatarKey = "Avatar_100";

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

        [Fact]
        public void Create_DefaultAvatar_ReturnsTheKeyOfTheFirstAvatar()
        {
            string key = AvatarResourceKey.Create(AvatarResourceKey.DefaultAvatarId);

            Assert.Equal(FirstAvatarKey, key);
        }

        [Fact]
        public void Create_LastSingleDigitIdentifier_PadsItWithAZero()
        {
            string key = AvatarResourceKey.Create(NineAvatarId);

            Assert.Equal(NineAvatarKey, key);
        }

        [Fact]
        public void Create_LargestTwoDigitIdentifier_KeepsBothDigits()
        {
            string key = AvatarResourceKey.Create(NinetyNineAvatarId);

            Assert.Equal(NinetyNineAvatarKey, key);
        }

        [Fact]
        public void Create_Zero_PadsItWithAZero()
        {
            string key = AvatarResourceKey.Create(NoAvatarId);

            Assert.Equal(NoAvatarKey, key);
        }

        [Fact]
        public void Create_ThreeDigitIdentifier_DoesNotCutTheNumber()
        {
            string key = AvatarResourceKey.Create(HundredAvatarId);

            Assert.Equal(HundredAvatarKey, key);
        }
    }
}
