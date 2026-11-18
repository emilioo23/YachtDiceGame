using System;
using Xunit;
using YachtDice.Server.Logic.Passkeys;

namespace YachtDice.Server.Tests.Passkeys
{
    /// <summary>
    /// Rules of the base64url text that WebAuthn uses for binary data.
    /// </summary>
    public class Base64UrlTests
    {
        private const string UrlText = "-_8";
        private const string InvalidText = "not*base64";
        private static readonly byte[] BytesWithSpecialCharacters = { 0xFB, 0xFF };

        [Fact]
        public void Encode_BytesWithPlusAndSlash_UsesUrlCharactersWithoutPadding()
        {
            string textExpected = UrlText;

            string textResult = Base64Url.Encode(BytesWithSpecialCharacters);

            Assert.Equal(textExpected, textResult);
        }

        [Fact]
        public void TryDecode_EncodedBytes_ReturnsTheSameBytes()
        {
            byte[] bytesExpected = BytesWithSpecialCharacters;

            Base64Url.TryDecode(UrlText, out byte[] bytesResult);

            Assert.Equal(bytesExpected, bytesResult);
        }

        [Fact]
        public void TryDecode_TextWithInvalidCharacters_ReturnsFalse()
        {
            bool isValid = Base64Url.TryDecode(InvalidText, out byte[] bytes);

            Assert.False(isValid);
        }

        [Fact]
        public void TryDecode_EmptyText_ReturnsFalse()
        {
            bool isValid = Base64Url.TryDecode(string.Empty, out byte[] bytes);

            Assert.False(isValid);
        }

        [Fact]
        public void Encode_NullBytes_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Base64Url.Encode(null));
        }
    }
}
