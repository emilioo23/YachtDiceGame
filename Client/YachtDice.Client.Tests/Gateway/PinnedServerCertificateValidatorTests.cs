using System;
using System.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Xunit;
using YachtDice.Gateway;

namespace YachtDice.Client.Tests.Gateway
{
    /// <summary>
    /// Rules that accept only the certificate of the game server, identified by its thumbprint.
    /// </summary>
    public sealed class PinnedServerCertificateValidatorTests : IDisposable
    {
        private const string SubjectName = "CN=YachtDiceServer";
        private const int KeySize = 2048;
        private const int ValidDays = 1;
        private const string SpacedThumbprintSeparator = " ";

        private readonly X509Certificate2 _serverCertificate = CreateCertificate();
        private readonly X509Certificate2 _otherCertificate = CreateCertificate();

        /// <summary>
        /// Frees the certificates created for the tests.
        /// </summary>
        public void Dispose()
        {
            _serverCertificate.Dispose();
            _otherCertificate.Dispose();
        }

        [Fact]
        public void IsPinned_CertificateWithTheExpectedThumbprint_ReturnsTrue()
        {
            var validator = new PinnedServerCertificateValidator(_serverCertificate.Thumbprint);

            bool isPinned = validator.IsPinned(_serverCertificate);

            Assert.True(isPinned);
        }

        [Fact]
        public void IsPinned_ThumbprintWithSpacesAndLowerCase_ReturnsTrue()
        {
            string spacedThumbprint = string.Join(
                SpacedThumbprintSeparator,
                _serverCertificate.Thumbprint.ToLowerInvariant().ToCharArray());
            var validator = new PinnedServerCertificateValidator(spacedThumbprint);

            bool isPinned = validator.IsPinned(_serverCertificate);

            Assert.True(isPinned);
        }

        [Fact]
        public void IsPinned_CertificateWithTheSameNameButAnotherKey_ReturnsFalse()
        {
            var validator = new PinnedServerCertificateValidator(_serverCertificate.Thumbprint);

            bool isPinned = validator.IsPinned(_otherCertificate);

            Assert.False(isPinned);
        }

        [Fact]
        public void IsPinned_NoExpectedThumbprint_ReturnsFalse()
        {
            var validator = new PinnedServerCertificateValidator(null);

            bool isPinned = validator.IsPinned(_serverCertificate);

            Assert.False(isPinned);
        }

        [Fact]
        public void IsPinned_NoCertificate_ReturnsFalse()
        {
            var validator = new PinnedServerCertificateValidator(_serverCertificate.Thumbprint);

            bool isPinned = validator.IsPinned(null);

            Assert.False(isPinned);
        }

        [Fact]
        public void Validate_CertificateOfAnotherServer_ThrowsSecurityTokenValidationException()
        {
            var validator = new PinnedServerCertificateValidator(_serverCertificate.Thumbprint);

            Assert.Throws<SecurityTokenValidationException>(() => validator.Validate(_otherCertificate));
        }

        private static X509Certificate2 CreateCertificate()
        {
            X509Certificate2 certificate;

            using (RSA key = RSA.Create(KeySize))
            {
                var certificateRequest = new CertificateRequest(
                    SubjectName,
                    key,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);
                DateTimeOffset now = DateTimeOffset.UtcNow;

                certificate = certificateRequest.CreateSelfSigned(now, now.AddDays(ValidDays));
            }

            return certificate;
        }
    }
}
