using Xunit;
using YachtDice.Server.Logic.Security;

namespace YachtDice.Server.Tests.Security
{
    /// <summary>
    /// Hash saved in the database instead of the password recovery code.
    /// </summary>
    public class PasswordResetCodeHasherTests
    {
        private const int FirstPlayerId = 1;
        private const int SecondPlayerId = 2;
        private const string Code = "123456";
        private const string OtherCode = "654321";

        [Fact]
        public void Compute_SamePlayerAndCode_ReturnsTheSameHash()
        {
            string first = PasswordResetCodeHasher.Compute(FirstPlayerId, Code);
            string second = PasswordResetCodeHasher.Compute(FirstPlayerId, Code);

            Assert.Equal(first, second);
        }

        [Fact]
        public void Compute_SameCodeForTwoPlayers_ReturnsDifferentHashes()
        {
            string first = PasswordResetCodeHasher.Compute(FirstPlayerId, Code);
            string second = PasswordResetCodeHasher.Compute(SecondPlayerId, Code);

            Assert.NotEqual(first, second);
        }

        [Fact]
        public void Compute_DifferentCodes_ReturnsDifferentHashes()
        {
            string first = PasswordResetCodeHasher.Compute(FirstPlayerId, Code);
            string second = PasswordResetCodeHasher.Compute(FirstPlayerId, OtherCode);

            Assert.NotEqual(first, second);
        }

        [Fact]
        public void Compute_AnyCode_DoesNotReturnThePlainCode()
        {
            string hash = PasswordResetCodeHasher.Compute(FirstPlayerId, Code);

            Assert.DoesNotContain(Code, hash);
        }
    }
}
