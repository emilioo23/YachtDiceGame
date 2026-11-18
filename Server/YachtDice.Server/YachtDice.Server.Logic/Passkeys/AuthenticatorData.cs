using System;

namespace YachtDice.Server.Logic.Passkeys
{
    /// <summary>
    /// Data that the authenticator signs in every ceremony: the hash of the relying party, the flags, the signature
    /// counter and, when a passkey is created, the new credential.
    /// </summary>
    public sealed class AuthenticatorData
    {
        private const int RelyingPartyHashSize = 32;
        private const int FlagsOffset = 32;
        private const int CounterOffset = 33;
        private const int CounterSize = 4;
        private const int FixedPartSize = 37;
        private const int AaguidSize = 16;
        private const int CredentialLengthSize = 2;
        private const int ByteMask = 0xFF;
        private const int BitsPerByte = 8;
        private const byte UserPresentFlag = 0x01;
        private const byte UserVerifiedFlag = 0x04;
        private const byte AttestedCredentialFlag = 0x40;

        private AuthenticatorData()
        {
        }

        /// <summary>
        /// SHA-256 hash of the identifier of the relying party that the authenticator used.
        /// </summary>
        public byte[] RelyingPartyHash { get; private set; }

        /// <summary>
        /// Indicates that the player touched or approved the authenticator.
        /// </summary>
        public bool IsUserPresent { get; private set; }

        /// <summary>
        /// Indicates that the authenticator checked who the player is, with a face, a fingerprint or a PIN.
        /// </summary>
        public bool IsUserVerified { get; private set; }

        /// <summary>
        /// Number of times that the authenticator has signed; zero if it does not count.
        /// </summary>
        public long SignCount { get; private set; }

        /// <summary>
        /// Identifier of the new credential; null if the data does not carry a credential.
        /// </summary>
        public byte[] CredentialId { get; private set; }

        /// <summary>
        /// Public key of the new credential in COSE format; null if the data does not carry a credential.
        /// </summary>
        public byte[] CredentialPublicKey { get; private set; }

        /// <summary>
        /// Reads the authenticator data.
        /// </summary>
        /// <param name="data">Bytes sent by the authenticator.</param>
        /// <param name="authenticatorData">The data that was read; null if the bytes are not valid.</param>
        /// <returns>True if the bytes are valid.</returns>
        public static bool TryParse(byte[] data, out AuthenticatorData authenticatorData)
        {
            authenticatorData = null;

            if (data != null && data.Length >= FixedPartSize)
            {
                authenticatorData = ReadFixedPart(data);

                if ((data[FlagsOffset] & AttestedCredentialFlag) != 0 && !TryReadCredential(data, authenticatorData))
                {
                    authenticatorData = null;
                }
            }

            return authenticatorData != null;
        }

        private static AuthenticatorData ReadFixedPart(byte[] data)
        {
            byte flags = data[FlagsOffset];
            long counter = 0;

            for (int index = 0; index < CounterSize; index++)
            {
                counter = (counter << BitsPerByte) | (uint)(data[CounterOffset + index] & ByteMask);
            }

            var relyingPartyHash = new byte[RelyingPartyHashSize];

            Array.Copy(data, 0, relyingPartyHash, 0, RelyingPartyHashSize);

            return new AuthenticatorData
            {
                RelyingPartyHash = relyingPartyHash,
                IsUserPresent = (flags & UserPresentFlag) != 0,
                IsUserVerified = (flags & UserVerifiedFlag) != 0,
                SignCount = counter
            };
        }

        private static bool TryReadCredential(byte[] data, AuthenticatorData target)
        {
            int lengthOffset = FixedPartSize + AaguidSize;
            bool isValid = data.Length >= lengthOffset + CredentialLengthSize;

            if (isValid)
            {
                int credentialLength = (data[lengthOffset] << BitsPerByte) | data[lengthOffset + 1];
                int credentialOffset = lengthOffset + CredentialLengthSize;

                isValid = credentialLength > 0 && credentialOffset + credentialLength < data.Length;

                if (isValid)
                {
                    target.CredentialId = new byte[credentialLength];
                    Array.Copy(data, credentialOffset, target.CredentialId, 0, credentialLength);
                    isValid = TryReadKey(data, credentialOffset + credentialLength, target);
                }
            }

            return isValid;
        }

        private static bool TryReadKey(byte[] data, int keyOffset, AuthenticatorData target)
        {
            var keyBytes = new byte[data.Length - keyOffset];
            bool isValid;

            Array.Copy(data, keyOffset, keyBytes, 0, keyBytes.Length);

            try
            {
                var reader = new CborReader(keyBytes);

                reader.ReadItem();
                target.CredentialPublicKey = new byte[reader.Position];
                Array.Copy(keyBytes, 0, target.CredentialPublicKey, 0, reader.Position);
                isValid = true;
            }
            catch (FormatException)
            {
                isValid = false;
            }

            return isValid;
        }
    }
}
