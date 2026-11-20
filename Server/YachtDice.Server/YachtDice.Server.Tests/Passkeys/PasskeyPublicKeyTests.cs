using System.Security.Cryptography;
using System.Text;
using Xunit;
using YachtDice.Server.Logic.Passkeys;

namespace YachtDice.Server.Tests.Passkeys
{
    /// <summary>
    /// Rules of the public key of a passkey and of the signatures that it checks.
    /// </summary>
    public class PasskeyPublicKeyTests
    {
        private const string SignedText = "text that the authenticator signs";
        private const string OtherText = "other text";
        private const int RsaKeySize = 2048;
        private static readonly byte[] UnsupportedAlgorithmKey = { 0xA2, 0x01, 0x02, 0x03, 0x27 };
        private static readonly byte[] NotCbor = { 0xFF, 0xFF };

        [Fact]
        public void Verify_SignatureOfTheMatchingPrivateKey_ReturnsTrue()
        {
            using (var authenticator = new FakeAuthenticator())
            {
                PasskeyPublicKey.TryParse(authenticator.GetCoseKey(), out PasskeyPublicKey key);
                byte[] signedData = Encoding.UTF8.GetBytes(SignedText);

                bool isValid = key.Verify(signedData, authenticator.SignDer(signedData));

                Assert.True(isValid);
            }
        }

        [Fact]
        public void Verify_SignatureOfOtherData_ReturnsFalse()
        {
            using (var authenticator = new FakeAuthenticator())
            {
                PasskeyPublicKey.TryParse(authenticator.GetCoseKey(), out PasskeyPublicKey key);
                byte[] signature = authenticator.SignDer(Encoding.UTF8.GetBytes(SignedText));

                bool isValid = key.Verify(Encoding.UTF8.GetBytes(OtherText), signature);

                Assert.False(isValid);
            }
        }

        [Fact]
        public void Verify_SignatureOfAnotherKey_ReturnsFalse()
        {
            using (var authenticator = new FakeAuthenticator())
            using (var otherAuthenticator = new FakeAuthenticator())
            {
                PasskeyPublicKey.TryParse(authenticator.GetCoseKey(), out PasskeyPublicKey key);
                byte[] signedData = Encoding.UTF8.GetBytes(SignedText);

                bool isValid = key.Verify(signedData, otherAuthenticator.SignDer(signedData));

                Assert.False(isValid);
            }
        }

        [Fact]
        public void Verify_RsaSignature_ReturnsTrue()
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.KeySize = RsaKeySize;
                PasskeyPublicKey.TryParse(CoseRsaKey.Create(rsa.ExportParameters(false)), out PasskeyPublicKey key);
                byte[] signedData = Encoding.UTF8.GetBytes(SignedText);
                byte[] signature = rsa.SignData(signedData, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                bool isValid = key.Verify(signedData, signature);

                Assert.True(isValid);
            }
        }

        [Fact]
        public void TryParse_UnsupportedAlgorithm_ReturnsFalse()
        {
            bool isParsed = PasskeyPublicKey.TryParse(UnsupportedAlgorithmKey, out PasskeyPublicKey key);

            Assert.False(isParsed);
        }

        [Fact]
        public void TryParse_BytesThatAreNotCbor_ReturnsFalse()
        {
            bool isParsed = PasskeyPublicKey.TryParse(NotCbor, out PasskeyPublicKey key);

            Assert.False(isParsed);
        }
    }
}
