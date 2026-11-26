using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using YachtDice.Server.Logic.Passkeys;

namespace YachtDice.Server.Tests.Passkeys
{
    /// <summary>
    /// Authenticator that signs with a real ECDSA key, so the tests check the same cryptography that Windows Hello
    /// and the phones use.
    /// </summary>
    internal sealed class FakeAuthenticator : IDisposable
    {
        private const int CredentialIdSize = 16;
        private const int AaguidSize = 16;
        private const int ByteMask = 0xFF;
        private const int BitsPerByte = 8;
        private const int CounterSize = 4;
        private const int DerHighBit = 0x80;
        private const byte DerSequence = 0x30;
        private const byte DerInteger = 0x02;
        private const byte AttestedCredentialFlag = 0x40;
        private const int PartsInSignature = 2;
        private const byte ByteStringWithOneByteLength = 0x58;
        private const string ClientDataFormat =
            "{{\"type\":\"{0}\",\"challenge\":\"{1}\",\"origin\":\"{2}\",\"crossOrigin\":false}}";

        private static readonly byte[] CoseKeyStart =
            { 0xA5, 0x01, 0x02, 0x03, 0x26, 0x20, 0x01, 0x21, 0x58, 0x20 };
        private static readonly byte[] CoseKeySecondCoordinate = { 0x22, 0x58, 0x20 };
        private static readonly byte[] AttestationObjectStart =
        {
            0xA3,
            0x63, 0x66, 0x6D, 0x74, 0x64, 0x6E, 0x6F, 0x6E, 0x65,
            0x67, 0x61, 0x74, 0x74, 0x53, 0x74, 0x6D, 0x74, 0xA0,
            0x68, 0x61, 0x75, 0x74, 0x68, 0x44, 0x61, 0x74, 0x61,
            ByteStringWithOneByteLength
        };
        private readonly ECDsa _key = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        /// <summary>
        /// Initializes an authenticator with a new key and a new credential.
        /// </summary>
        public FakeAuthenticator()
        {
            CredentialId = new byte[CredentialIdSize];
            RandomNumberGenerator.Create().GetBytes(CredentialId);
        }

        /// <summary>
        /// Identifier of the credential of this authenticator.
        /// </summary>
        public byte[] CredentialId { get; }

        /// <summary>
        /// Creates the answer of a registration.
        /// </summary>
        /// <param name="challenge">Challenge that the server issued.</param>
        /// <param name="options">Values to change in the answer.</param>
        /// <returns>The answer of the authenticator.</returns>
        public PasskeyAttestation CreateAttestation(PasskeyChallenge challenge, AuthenticatorOptions options)
        {
            byte[] clientData = CreateClientData(PasskeyRules.CreateClientDataType, challenge, options);
            byte[] authenticatorData = CreateAuthenticatorData(options, true);

            return new PasskeyAttestation
            {
                ChallengeId = challenge.ChallengeId,
                ClientDataJson = PasskeyRules.Encode(clientData),
                AttestationObject = PasskeyRules.Encode(CreateAttestationObject(authenticatorData)),
                Transport = options.Transport
            };
        }

        /// <summary>
        /// Creates the answer of a login.
        /// </summary>
        /// <param name="challenge">Challenge that the server issued.</param>
        /// <param name="options">Values to change in the answer.</param>
        /// <returns>The answer of the authenticator.</returns>
        public PasskeyAssertion CreateAssertion(PasskeyChallenge challenge, AuthenticatorOptions options)
        {
            byte[] clientData = CreateClientData(PasskeyRules.GetClientDataType, challenge, options);
            byte[] authenticatorData = CreateAuthenticatorData(options, false);

            return new PasskeyAssertion
            {
                ChallengeId = challenge.ChallengeId,
                CredentialId = PasskeyRules.Encode(CredentialId),
                ClientDataJson = PasskeyRules.Encode(clientData),
                AuthenticatorData = PasskeyRules.Encode(authenticatorData),
                Signature = PasskeyRules.Encode(Sign(authenticatorData, clientData)),
                UserHandle = options.UserHandle
            };
        }

        /// <summary>
        /// Gives the public key of the authenticator in COSE format.
        /// </summary>
        /// <returns>The bytes of the key.</returns>
        public byte[] GetCoseKey()
        {
            ECParameters parameters = _key.ExportParameters(false);
            var key = new MemoryStream();

            key.Write(CoseKeyStart, 0, CoseKeyStart.Length);
            key.Write(parameters.Q.X, 0, parameters.Q.X.Length);
            key.Write(CoseKeySecondCoordinate, 0, CoseKeySecondCoordinate.Length);
            key.Write(parameters.Q.Y, 0, parameters.Q.Y.Length);

            return key.ToArray();
        }

        /// <summary>
        /// Signs data with the key of the authenticator in the DER format.
        /// </summary>
        /// <param name="data">Data to sign.</param>
        /// <returns>The signature in DER format.</returns>
        public byte[] SignDer(byte[] data)
        {
            return ToDer(_key.SignData(data, HashAlgorithmName.SHA256));
        }

        /// <summary>
        /// Frees the key of the authenticator.
        /// </summary>
        public void Dispose()
        {
            _key.Dispose();
        }

        /// <summary>
        /// Converts a signature of fixed format into DER.
        /// </summary>
        /// <param name="fixedSignature">Signature with r followed by s.</param>
        /// <returns>The signature in DER format.</returns>
        public static byte[] ToDer(byte[] fixedSignature)
        {
            int half = fixedSignature.Length / PartsInSignature;
            byte[] r = ToDerInteger(fixedSignature.Take(half).ToArray());
            byte[] s = ToDerInteger(fixedSignature.Skip(half).ToArray());
            var der = new MemoryStream();

            der.WriteByte(DerSequence);
            der.WriteByte((byte)(r.Length + s.Length));
            der.Write(r, 0, r.Length);
            der.Write(s, 0, s.Length);

            return der.ToArray();
        }

        private static byte[] ToDerInteger(byte[] value)
        {
            Func<byte, bool> isZero = item => item == 0;
            byte[] trimmed = value.SkipWhile(isZero).ToArray();
            byte[] content = trimmed.Length == 0 ? new byte[] { 0 } : trimmed;

            if ((content[0] & DerHighBit) != 0)
            {
                content = new byte[] { 0 }.Concat(content).ToArray();
            }

            return new byte[] { DerInteger, (byte)content.Length }.Concat(content).ToArray();
        }

        private static byte[] CreateClientData(
            string ceremonyType,
            PasskeyChallenge challenge,
            AuthenticatorOptions options)
        {
            string json = string.Format(
                ClientDataFormat,
                options.ClientDataType ?? ceremonyType,
                options.Challenge ?? challenge.Challenge,
                options.Origin);

            return Encoding.UTF8.GetBytes(json);
        }

        private byte[] CreateAuthenticatorData(AuthenticatorOptions options, bool includeCredential)
        {
            var data = new MemoryStream();
            byte[] hash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(options.RelyingPartyId));

            data.Write(hash, 0, hash.Length);
            data.WriteByte((byte)(includeCredential ? options.Flags | AttestedCredentialFlag : options.Flags));

            for (int index = CounterSize - 1; index >= 0; index--)
            {
                data.WriteByte((byte)((options.SignCount >> (index * BitsPerByte)) & ByteMask));
            }

            if (includeCredential)
            {
                data.Write(new byte[AaguidSize], 0, AaguidSize);
                data.WriteByte((byte)(CredentialId.Length >> BitsPerByte));
                data.WriteByte((byte)(CredentialId.Length & ByteMask));
                data.Write(CredentialId, 0, CredentialId.Length);
                byte[] coseKey = GetCoseKey();

                data.Write(coseKey, 0, coseKey.Length);
            }

            return data.ToArray();
        }

        private static byte[] CreateAttestationObject(byte[] authenticatorData)
        {
            var attestation = new MemoryStream();
            attestation.Write(AttestationObjectStart, 0, AttestationObjectStart.Length);
            attestation.WriteByte((byte)authenticatorData.Length);
            attestation.Write(authenticatorData, 0, authenticatorData.Length);

            return attestation.ToArray();
        }

        private byte[] Sign(byte[] authenticatorData, byte[] clientData)
        {
            byte[] clientDataHash = SHA256.Create().ComputeHash(clientData);

            return SignDer(authenticatorData.Concat(clientDataHash).ToArray());
        }
    }
}
