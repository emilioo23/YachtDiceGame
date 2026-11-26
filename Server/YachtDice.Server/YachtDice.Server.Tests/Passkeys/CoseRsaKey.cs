using System.IO;
using System.Security.Cryptography;

namespace YachtDice.Server.Tests.Passkeys
{
    /// <summary>
    /// Writes an RSA public key in the COSE format, as an authenticator would send it.
    /// </summary>
    internal static class CoseRsaKey
    {
        private const int ByteMask = 0xFF;
        private const int BitsPerByte = 8;

        private static readonly byte[] Start = { 0xA4, 0x01, 0x03, 0x03, 0x39, 0x01, 0x00 };
        private static readonly byte[] ModulusLabel = { 0x20, 0x59 };
        private static readonly byte[] ExponentLabel = { 0x21, 0x43 };
        /// <summary>
        /// Writes the key.
        /// </summary>
        /// <param name="parameters">Public parameters of the RSA key.</param>
        /// <returns>The bytes of the key in COSE format.</returns>
        public static byte[] Create(RSAParameters parameters)
        {
            var key = new MemoryStream();

            key.Write(Start, 0, Start.Length);
            key.Write(ModulusLabel, 0, ModulusLabel.Length);
            key.WriteByte((byte)(parameters.Modulus.Length >> BitsPerByte));
            key.WriteByte((byte)(parameters.Modulus.Length & ByteMask));
            key.Write(parameters.Modulus, 0, parameters.Modulus.Length);
            key.Write(ExponentLabel, 0, ExponentLabel.Length);
            key.Write(parameters.Exponent, 0, parameters.Exponent.Length);

            return key.ToArray();
        }
    }
}
