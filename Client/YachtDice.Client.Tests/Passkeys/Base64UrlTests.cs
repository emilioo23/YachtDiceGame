using System;
using Xunit;
using YachtDice.Client.Logic.Passkeys;

namespace YachtDice.Client.Tests.Passkeys
{
    /// <summary>
    /// Rules of the base64url text of the passkeys.
    /// </summary>
    public class Base64UrlTests
    {
        private const string NotBase64 = "***";
        private const string SpecialText = "-_--";
        private const string PaddedText = "AQ";
        private static readonly byte[] SpecialBytes = { 0xFB, 0xFF, 0xBE };
        private static readonly byte[] OneByte = { 0x01 };

        [Fact]
        public void Encode_BytesThatUseSpecialCharacters_UsesTheUrlAlphabet()
        {
            string textExpected = SpecialText;

            string textResult = Base64Url.Encode(SpecialBytes);

            Assert.Equal(textExpected, textResult);
        }

        [Fact]
        public void Encode_BytesThatNeedPadding_RemovesThePadding()
        {
            string textExpected = PaddedText;

            string textResult = Base64Url.Encode(OneByte);

            Assert.Equal(textExpected, textResult);
        }

        [Fact]
        public void Encode_NullBytes_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Base64Url.Encode(null));
        }

        [Fact]
        public void TryDecode_TextWithoutPadding_ReturnsTheBytes()
        {
            byte[] dataExpected = OneByte;

            Base64Url.TryDecode(PaddedText, out byte[] dataResult);

            Assert.Equal(dataExpected, dataResult);
        }

        [Fact]
        public void TryDecode_TextThatIsNotBase64_ReturnsFalse()
        {
            bool isValid = Base64Url.TryDecode(NotBase64, out byte[] data);

            Assert.False(isValid);
        }

        [Fact]
        public void TryDecode_EmptyText_ReturnsFalse()
        {
            bool isValid = Base64Url.TryDecode(string.Empty, out byte[] data);

            Assert.False(isValid);
        }
    }
}
