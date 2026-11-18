using Xunit;
using YachtDice.Server.Logic.Passkeys;

namespace YachtDice.Server.Tests.Passkeys
{
    /// <summary>
    /// Rules of the conversion of ECDSA signatures from DER to the fixed format.
    /// </summary>
    public class EcdsaSignatureConverterTests
    {
        private const int PartSize = 4;
        private static readonly byte[] DerWithLeadingZero =
            { 0x30, 0x0A, 0x02, 0x03, 0x00, 0x80, 0x01, 0x02, 0x03, 0x01, 0x02, 0x03 };
        private static readonly byte[] ExpectedFixed = { 0x00, 0x00, 0x80, 0x01, 0x00, 0x01, 0x02, 0x03 };
        private static readonly byte[] NotASequence = { 0x31, 0x00 };
        private static readonly byte[] WithExtraBytes = { 0x30, 0x06, 0x02, 0x01, 0x01, 0x02, 0x01, 0x01, 0xFF };

        [Fact]
        public void TryConvert_DerWithLeadingZero_ReturnsRAndSPaddedToTheSameSize()
        {
            EcdsaSignatureConverter.TryConvert(DerWithLeadingZero, PartSize, out byte[] fixedSignature);

            Assert.Equal(ExpectedFixed, fixedSignature);
        }

        [Fact]
        public void TryConvert_NotASequence_ReturnsFalse()
        {
            bool isConverted = EcdsaSignatureConverter.TryConvert(NotASequence, PartSize, out byte[] fixedSignature);

            Assert.False(isConverted);
        }

        [Fact]
        public void TryConvert_BytesAfterTheSequence_ReturnsFalse()
        {
            bool isConverted = EcdsaSignatureConverter.TryConvert(WithExtraBytes, PartSize, out byte[] fixedSignature);

            Assert.False(isConverted);
        }
    }
}
