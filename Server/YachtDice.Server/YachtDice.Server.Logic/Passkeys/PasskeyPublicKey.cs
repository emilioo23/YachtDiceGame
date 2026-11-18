using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace YachtDice.Server.Logic.Passkeys
{
    /// <summary>
    /// Public key of a passkey, read from the COSE format that the authenticators send. It accepts ECDSA with
    /// P-256 and RSA with PKCS#1 v1.5, both with SHA-256, which are the algorithms that Windows Hello and the
    /// phones use.
    /// </summary>
    public sealed class PasskeyPublicKey
    {
        private const long KeyTypeLabel = 1;
        private const long AlgorithmLabel = 3;
        private const long CurveOrModulusLabel = -1;
        private const long XOrExponentLabel = -2;
        private const long YLabel = -3;
        private const long EllipticKeyType = 2;
        private const long RsaKeyType = 3;
        private const long P256Curve = 1;
        private const int P256PartSize = 32;

        private readonly byte[] _coseKey;
        private readonly long _algorithm;
        private readonly byte[] _first;
        private readonly byte[] _second;
        private readonly byte[] _third;

        private PasskeyPublicKey(byte[] coseKey, long algorithm, byte[][] parts)
        {
            _coseKey = coseKey;
            _algorithm = algorithm;
            _first = parts[0];
            _second = parts[1];
            _third = parts[2];
        }

        /// <summary>
        /// Bytes of the key in the COSE format, as the authenticator sent them.
        /// </summary>
        /// <returns>A copy of the bytes of the key.</returns>
        public byte[] ToBytes()
        {
            return (byte[])_coseKey.Clone();
        }

        /// <summary>
        /// Reads a key from the COSE format.
        /// </summary>
        /// <param name="coseKey">Bytes of the key.</param>
        /// <param name="key">The key; null if the bytes are not a supported key.</param>
        /// <returns>True if the key is valid and uses a supported algorithm.</returns>
        public static bool TryParse(byte[] coseKey, out PasskeyPublicKey key)
        {
            key = null;

            if (coseKey != null)
            {
                key = ReadKey(coseKey);
            }

            return key != null;
        }

        /// <summary>
        /// Checks a signature made with the private key that matches this public key.
        /// </summary>
        /// <param name="signedData">Data that was signed.</param>
        /// <param name="signature">Signature that the authenticator sent.</param>
        /// <returns>True if the signature is valid for the data.</returns>
        public bool Verify(byte[] signedData, byte[] signature)
        {
            bool isValid = false;

            if (signedData != null && signature != null)
            {
                isValid = _algorithm == PasskeyRules.EcdsaAlgorithm
                    ? VerifyEcdsa(signedData, signature)
                    : VerifyRsa(signedData, signature);
            }

            return isValid;
        }

        private static PasskeyPublicKey ReadKey(byte[] coseKey)
        {
            PasskeyPublicKey key = null;

            try
            {
                var map = new CborReader(coseKey).ReadItem() as Dictionary<object, object>;

                if (map != null)
                {
                    key = CreateKey(coseKey, map);
                }
            }
            catch (FormatException)
            {
                key = null;
            }

            return key;
        }

        private static PasskeyPublicKey CreateKey(byte[] coseKey, Dictionary<object, object> map)
        {
            PasskeyPublicKey key = null;
            object keyType = GetValue(map, KeyTypeLabel);
            object algorithm = GetValue(map, AlgorithmLabel);

            if (keyType is long && algorithm is long)
            {
                key = CreateTypedKey(coseKey, map, (long)keyType, (long)algorithm);
            }

            return key;
        }

        private static PasskeyPublicKey CreateTypedKey(
            byte[] coseKey,
            Dictionary<object, object> map,
            long keyType,
            long algorithm)
        {
            PasskeyPublicKey key = null;
            var parts = new[]
            {
                GetValue(map, CurveOrModulusLabel),
                GetValue(map, XOrExponentLabel),
                GetValue(map, YLabel)
            };

            if (keyType == EllipticKeyType && algorithm == PasskeyRules.EcdsaAlgorithm && IsValidEllipticKey(parts))
            {
                key = new PasskeyPublicKey(coseKey, algorithm, new[] { null, (byte[])parts[1], (byte[])parts[2] });
            }
            else if (keyType == RsaKeyType && algorithm == PasskeyRules.RsaAlgorithm && IsValidRsaKey(parts))
            {
                key = new PasskeyPublicKey(coseKey, algorithm, new[] { (byte[])parts[0], (byte[])parts[1], null });
            }

            return key;
        }

        private static bool IsValidEllipticKey(object[] parts)
        {
            return parts[0] is long && (long)parts[0] == P256Curve
                && parts[1] is byte[] && ((byte[])parts[1]).Length == P256PartSize
                && parts[2] is byte[] && ((byte[])parts[2]).Length == P256PartSize;
        }

        private static bool IsValidRsaKey(object[] parts)
        {
            return parts[0] is byte[] && ((byte[])parts[0]).Length > 0
                && parts[1] is byte[] && ((byte[])parts[1]).Length > 0;
        }

        private static object GetValue(Dictionary<object, object> map, long label)
        {
            map.TryGetValue(label, out object value);

            return value;
        }

        private bool VerifyEcdsa(byte[] signedData, byte[] signature)
        {
            bool isValid = false;

            if (EcdsaSignatureConverter.TryConvert(signature, P256PartSize, out byte[] fixedSignature))
            {
                isValid = VerifyWithAlgorithm(() => CreateEcdsa(), algorithm =>
                    ((ECDsa)algorithm).VerifyData(signedData, fixedSignature, HashAlgorithmName.SHA256));
            }

            return isValid;
        }

        private bool VerifyRsa(byte[] signedData, byte[] signature)
        {
            return VerifyWithAlgorithm(() => CreateRsa(), algorithm =>
                ((RSA)algorithm).VerifyData(
                    signedData,
                    signature,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1));
        }

        private static bool VerifyWithAlgorithm(Func<IDisposable> create, Func<IDisposable, bool> verify)
        {
            bool isValid;

            try
            {
                using (IDisposable algorithm = create())
                {
                    isValid = verify(algorithm);
                }
            }
            catch (CryptographicException)
            {
                isValid = false;
            }

            return isValid;
        }

        private ECDsa CreateEcdsa()
        {
            var parameters = new ECParameters
            {
                Curve = ECCurve.NamedCurves.nistP256,
                Q = new ECPoint { X = _second, Y = _third }
            };

            return ECDsa.Create(parameters);
        }

        private RSA CreateRsa()
        {
            var parameters = new RSAParameters { Modulus = _first, Exponent = _second };
            RSA rsa = RSA.Create();

            rsa.ImportParameters(parameters);

            return rsa;
        }
    }
}
