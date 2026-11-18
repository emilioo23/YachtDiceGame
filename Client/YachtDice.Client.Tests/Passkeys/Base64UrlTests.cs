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
        private static readonly byte[] SpecialBytes = { 0xFB, 0xFF, 0xBE };
        private static readonly byte[] OneByte = { 0x01 };
        private const string NotBase64 = "***";

        [Fact]
        public void Encode_BytesThatUseSpecialCharacters_UsesTheUrlAlphabet()
        {
            string text = Base64Url.Encode(SpecialBytes);

            Assert.Equal("-_--", text);
        }

        [Fact]
        public void Encode_BytesThatNeedPadding_RemovesThePadding()
        {
            string text = Base64Url.Encode(OneByte);

            Assert.Equal("AQ", text);
        }

        [Fact]
        public void Encode_NullBytes_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Base64Url.Encode(null));
        }

        [Fact]
        public void TryDecode_TextWithoutPadding_ReturnsTheBytes()
        {
            Base64Url.TryDecode("AQ", out byte[] data);

            Assert.Equal(OneByte, data);
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
