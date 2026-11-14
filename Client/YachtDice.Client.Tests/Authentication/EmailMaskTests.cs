using Xunit;
using YachtDice.Client.Logic.Authentication;

namespace YachtDice.Client.Tests.Authentication
{
    /// <summary>
    /// Email addresses hidden before they are shown on screen.
    /// </summary>
    public class EmailMaskTests
    {
        private const string LongEmail = "gabriel@gmail.com";
        private const string MaskedLongEmail = "ga*****@gmail.com";
        private const string ShortEmail = "ab@gmail.com";
        private const string ThreeCharactersEmail = "abc@gmail.com";
        private const string MaskedThreeCharactersEmail = "ab*@gmail.com";
        private const string OneCharacterEmail = "a@gmail.com";
        private const string TextWithoutAtSign = "gabriel.gmail.com";
        private const string AtSignFirstText = "@gmail.com";
        private const string TwoAtSignsText = "gabriel@gm@il.com";
        private const string MaskedTwoAtSignsText = "ga*****@gm@il.com";

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

        [Fact]
        public void Mask_LocalPartOfThreeCharacters_HidesOnlyTheLastOne()
        {
            string masked = EmailMask.Mask(ThreeCharactersEmail);

            Assert.Equal(MaskedThreeCharactersEmail, masked);
        }

        [Fact]
        public void Mask_LocalPartOfOneCharacter_ReturnsTheSameAddress()
        {
            string masked = EmailMask.Mask(OneCharacterEmail);

            Assert.Equal(OneCharacterEmail, masked);
        }

        [Fact]
        public void Mask_TextWithoutAtSign_ReturnsTheSameText()
        {
            string masked = EmailMask.Mask(TextWithoutAtSign);

            Assert.Equal(TextWithoutAtSign, masked);
        }

        [Fact]
        public void Mask_EmptyText_ReturnsAnEmptyText()
        {
            string masked = EmailMask.Mask(string.Empty);

            Assert.Equal(string.Empty, masked);
        }

        [Fact]
        public void Mask_AtSignAtTheStart_ReturnsTheSameText()
        {
            string masked = EmailMask.Mask(AtSignFirstText);

            Assert.Equal(AtSignFirstText, masked);
        }

        [Fact]
        public void Mask_TwoAtSigns_MasksUntilTheFirstOne()
        {
            string masked = EmailMask.Mask(TwoAtSignsText);

            Assert.Equal(MaskedTwoAtSignsText, masked);
        }
    }
}
