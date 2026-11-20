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
        private readonly byte[] _modulus;
        private readonly byte[] _exponent;
        private readonly byte[] _pointX;
        private readonly byte[] _pointY;

        private PasskeyPublicKey(byte[] coseKey, CoseFields fields)
        {
            _coseKey = coseKey;
            _algorithm = (long)fields.Algorithm;

            if (_algorithm == PasskeyRules.EcdsaAlgorithm)
            {
                _pointX = (byte[])fields.XOrExponent;
                _pointY = (byte[])fields.Y;
            }
            else
            {
                _modulus = (byte[])fields.CurveOrModulus;
                _exponent = (byte[])fields.XOrExponent;
            }
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
                if (_algorithm == PasskeyRules.EcdsaAlgorithm)
                {
                    isValid = VerifyEcdsa(signedData, signature);
                }
                else
                {
                    isValid = VerifyRsa(signedData, signature);
                }
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
            var fields = new CoseFields
            {
                KeyType = GetValue(map, KeyTypeLabel),
                Algorithm = GetValue(map, AlgorithmLabel),
                CurveOrModulus = GetValue(map, CurveOrModulusLabel),
                XOrExponent = GetValue(map, XOrExponentLabel),
                Y = GetValue(map, YLabel)
            };
            PasskeyPublicKey key = null;

            if (IsValidEllipticKey(fields) || IsValidRsaKey(fields))
            {
                key = new PasskeyPublicKey(coseKey, fields);
            }

            return key;
        }

        private static bool IsValidEllipticKey(CoseFields fields)
        {
            bool isElliptic = fields.KeyType is long && (long)fields.KeyType == EllipticKeyType
                && fields.Algorithm is long && (long)fields.Algorithm == PasskeyRules.EcdsaAlgorithm;

            return isElliptic
                && fields.CurveOrModulus is long && (long)fields.CurveOrModulus == P256Curve
                && HasLength(fields.XOrExponent, P256PartSize)
                && HasLength(fields.Y, P256PartSize);
        }

        private static bool IsValidRsaKey(CoseFields fields)
        {
            bool isRsa = fields.KeyType is long && (long)fields.KeyType == RsaKeyType
                && fields.Algorithm is long && (long)fields.Algorithm == PasskeyRules.RsaAlgorithm;

            return isRsa
                && fields.CurveOrModulus is byte[] && ((byte[])fields.CurveOrModulus).Length > 0
                && fields.XOrExponent is byte[] && ((byte[])fields.XOrExponent).Length > 0;
        }

        private static bool HasLength(object value, int length)
        {
            return value is byte[] && ((byte[])value).Length == length;
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
                Q = new ECPoint { X = _pointX, Y = _pointY }
            };

            return ECDsa.Create(parameters);
        }

        private RSA CreateRsa()
        {
            var parameters = new RSAParameters { Modulus = _modulus, Exponent = _exponent };
            RSA rsa = RSA.Create();

            rsa.ImportParameters(parameters);

            return rsa;
        }

        private sealed class CoseFields
        {
            public object KeyType { get; set; }

            public object Algorithm { get; set; }

            public object CurveOrModulus { get; set; }

            public object XOrExponent { get; set; }

            public object Y { get; set; }
        }
    }
}
