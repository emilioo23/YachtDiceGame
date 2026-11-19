using System;
using System.Configuration;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using YachtDice.Utils;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Accepts the certificate of the server only when its thumbprint is the one written in the application
    /// settings. The certificate of the game is self-signed, so this check takes the place of a certificate
    /// authority and the players do not have to install the certificate on their computers.
    /// </summary>
    public sealed class PinnedServerCertificateValidator : X509CertificateValidator
    {
        private const string ThumbprintKey = "ServerCertificateThumbprint";
        private const string UnknownCertificateMessage =
            "The server presented a certificate whose thumbprint is not the one of the game.";
        private const string UnknownCertificateLogMessage =
            "The server presented a certificate that is not the one of the game, so the connection was refused.";

        private readonly AppLogger _logger;
        private readonly string _expectedThumbprint;

        /// <summary>
        /// Initializes the validator with the thumbprint of the application settings. WCF uses this constructor
        /// when the configuration names this class as the validator of the server certificate.
        /// </summary>
        public PinnedServerCertificateValidator()
            : this(ConfigurationManager.AppSettings[ThumbprintKey])
        {
        }

        /// <summary>
        /// Initializes the validator with the thumbprint that the certificate of the server must have.
        /// </summary>
        /// <param name="expectedThumbprint">Thumbprint of the certificate; spaces and letter case are ignored.</param>
        public PinnedServerCertificateValidator(string expectedThumbprint)
        {
            _expectedThumbprint = Normalize(expectedThumbprint);
            _logger = new AppLogger(GetType());
        }

        /// <summary>
        /// Checks the certificate that the server presented when the connection was opened.
        /// </summary>
        /// <param name="certificate">Certificate of the server.</param>
        /// <exception cref="SecurityTokenValidationException">The certificate is not the one of the game.</exception>
        public override void Validate(X509Certificate2 certificate)
        {
            if (!IsPinned(certificate))
            {
                _logger.Warning(UnknownCertificateLogMessage);
                throw new SecurityTokenValidationException(UnknownCertificateMessage);
            }
        }

        /// <summary>
        /// Indicates whether the certificate is the one of the game.
        /// </summary>
        /// <param name="certificate">Certificate of the server.</param>
        /// <returns>True if the thumbprint of the certificate is the expected one.</returns>
        public bool IsPinned(X509Certificate2 certificate)
        {
            return certificate != null
                && _expectedThumbprint.Length > 0
                && string.Equals(Normalize(certificate.Thumbprint), _expectedThumbprint, StringComparison.Ordinal);
        }

        private static string Normalize(string thumbprint)
        {
            string hexDigits = new string((thumbprint ?? string.Empty).Where(Uri.IsHexDigit).ToArray());

            return hexDigits.ToUpperInvariant();
        }
    }
}
