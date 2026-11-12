using Xunit;
using YachtDice.Client.Logic.Authentication;

namespace YachtDice.Client.Tests.Authentication
{
    /// <summary>
    /// Email addresses hidden before they are shown on screen.
    /// </summary>
    public class EmailMaskTests
    {
        private const string LongEmail = "gabriel@example.com";
        private const string MaskedLongEmail = "ga*****@example.com";
        private const string ShortEmail = "ab@example.com";

        [Fact]
        public void Mask_LongLocalPart_KeepsTheFirstTwoCharactersAndTheDomain()
        {
            string masked = EmailMask.Mask(LongEmail);

            Assert.Equal(MaskedLongEmail, masked);
        }

        [Fact]
        public void Mask_LocalPartOfTwoCharacters_ReturnsTheSameAddress()
        {
            string masked = EmailMask.Mask(ShortEmail);

            Assert.Equal(ShortEmail, masked);
        }
    }
}
