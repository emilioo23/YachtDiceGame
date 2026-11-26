using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml;
using YachtDice.Server.Data.Logging;

namespace YachtDice.Server.Logic.Passkeys
{
    /// <summary>
    /// Rules and helpers of the passkeys: names, PasskeyRules text, ECDSA signatures and the check of the client data.
    /// </summary>
    public static class PasskeyRules
    {
        /// <summary>
        /// Identifier of the relying party. The authenticators sign it inside every response, so the client and
        /// the server must use exactly the same text.
        /// </summary>
        public const string RelyingPartyId = "yachtdice.localhost";

        /// <summary>
        /// Origin written by the client in the client data of every ceremony.
        /// </summary>
        public const string Origin = "https://yachtdice.localhost";

        /// <summary>
        /// Value of the type field of the client data when a passkey is created.
        /// </summary>
        public const string CreateClientDataType = "webauthn.create";

        /// <summary>
        /// Value of the type field of the client data when a passkey is used to log in.
        /// </summary>
        public const string GetClientDataType = "webauthn.get";

        /// <summary>
        /// Maximum number of passkeys that one player can register.
        /// </summary>
        public const int MaxPasskeysPerPlayer = 5;

        /// <summary>
        /// Number of random bytes of every challenge.
        /// </summary>
        public const int ChallengeSize = 32;

        /// <summary>
        /// Minutes that the player has to finish a ceremony before the challenge stops working.
        /// </summary>
        public const int ChallengeMinutes = 5;

        /// <summary>
        /// Maximum number of challenges waiting for an answer. It stops anybody from filling the memory of the
        /// server by asking for challenges and never answering.
        /// </summary>
        public const int MaxPendingChallenges = 500;

        /// <summary>
        /// Maximum length accepted for the base64url texts that the client sends.
        /// </summary>
        public const int MaxEncodedLength = 8192;

        /// <summary>
        /// Maximum length of the name saved with a passkey.
        /// </summary>
        public const int DeviceNameMaxLength = 100;

        /// <summary>
        /// Identifier of the ECDSA with P-256 and SHA-256 algorithm in the COSE registry.
        /// </summary>
        public const int EcdsaAlgorithm = -7;

        /// <summary>
        /// Identifier of the RSASSA-PKCS1-v1_5 with SHA-256 algorithm in the COSE registry.
        /// </summary>
        public const int RsaAlgorithm = -257;

        /// <summary>
        /// Transport code that means that Windows did not report the authenticator.
        /// </summary>
        public const int UnknownTransport = 0;

        /// <summary>
        /// Transport code that Windows reports when the authenticator is the computer itself.
        /// </summary>
        public const int InternalTransport = 0x10;

        /// <summary>
        /// Transport code that Windows reports when the authenticator is a phone paired with the computer.
        /// </summary>
        public const int HybridTransport = 0x20;

        /// <summary>
        /// Transport code that Windows reports for a security key connected by USB.
        /// </summary>
        public const int UsbTransport = 0x01;

        /// <summary>
        /// Transport code that Windows reports for a security key read by NFC.
        /// </summary>
        public const int NfcTransport = 0x02;

        /// <summary>
        /// Transport code that Windows reports for a security key connected by Bluetooth.
        /// </summary>
        public const int BluetoothTransport = 0x04;

        /// <summary>
        /// Name of a passkey that lives in Windows Hello.
        /// </summary>
        public const string WindowsHelloName = "Windows Hello";

        /// <summary>
        /// Name of a passkey that lives in a phone.
        /// </summary>
        public const string PhoneName = "Celular";

        /// <summary>
        /// Name of a passkey that lives in a security key.
        /// </summary>
        public const string SecurityKeyName = "Llave de seguridad";

        /// <summary>
        /// Name of a passkey whose authenticator is not known.
        /// </summary>
        public const string UnknownName = "Passkey";

        private const char StandardPlus = '+';
        private const char StandardSlash = '/';
        private const char UrlMinus = '-';
        private const char UrlUnderscore = '_';
        private const char Padding = '=';
        private const int GroupSize = 4;
        private const byte SequenceTag = 0x30;
        private const byte IntegerTag = 0x02;
        private const int LongFormFlag = 0x80;
        private const int OneByteLongForm = 0x81;
        private const int PartCount = 2;
        private const int MaxClientDataSize = 2048;

        /// <summary>
        /// Gets the name of the kind of authenticator that Windows reported.
        /// </summary>
        /// <param name="transport">Transport code that Windows reported; zero if it did not report one.</param>
        /// <returns>The name of the kind; the unknown name if the code is not recognized.</returns>
        public static string GetName(int transport)
        {
            string name = UnknownName;

            if (transport == InternalTransport)
            {
                name = WindowsHelloName;
            }
            else if (transport == HybridTransport)
            {
                name = PhoneName;
            }
            else if (transport == UsbTransport || transport == NfcTransport || transport == BluetoothTransport)
            {
                name = SecurityKeyName;
            }

            return name;
        }

        /// <summary>
        /// Writes the bytes as base64url without padding.
        /// </summary>
        /// <param name="data">Bytes to write.</param>
        /// <returns>The base64url text.</returns>
        /// <exception cref="ArgumentNullException">The bytes are null.</exception>
        public static string Encode(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), LogMessages.PasskeyNullDataMessage);
            }

            string text = Convert.ToBase64String(data)
                .TrimEnd(Padding)
                .Replace(StandardPlus, UrlMinus)
                .Replace(StandardSlash, UrlUnderscore);

            return text;
        }

        /// <summary>
        /// Reads a base64url text, with or without padding.
        /// </summary>
        /// <param name="text">Text to read.</param>
        /// <param name="data">Bytes of the text; null if the text is not valid.</param>
        /// <returns>True if the text is valid base64url.</returns>
        public static bool TryDecode(string text, out byte[] data)
        {
            bool isValid = false;

            data = null;

            if (!string.IsNullOrEmpty(text) && text.Length <= PasskeyRules.MaxEncodedLength)
            {
                string standard = text.Replace(UrlMinus, StandardPlus).Replace(UrlUnderscore, StandardSlash);
                int missing = (GroupSize - standard.Length % GroupSize) % GroupSize;
                string padded = standard.PadRight(standard.Length + missing, Padding);

                isValid = TryConvert(padded, out data);
            }

            return isValid;
        }

        /// <summary>
        /// Converts the signature.
        /// </summary>
        /// <param name="derSignature">Signature in DER format.</param>
        /// <param name="partSize">Size in bytes of r and of s; 32 for the P-256 curve.</param>
        /// <param name="fixedSignature">Signature with r and s one after the other; null if it is not valid.</param>
        /// <returns>True if the signature was converted.</returns>
        public static bool TryConvert(byte[] derSignature, int partSize, out byte[] fixedSignature)
        {
            fixedSignature = null;

            return derSignature != null
                && new DerReader(derSignature).TryReadSignature(partSize, out fixedSignature);
        }

        /// <summary>
        /// Checks the client data of one ceremony.
        /// </summary>
        /// <param name="clientDataJson">Bytes of the client data JSON.</param>
        /// <param name="expectedType">Type that the ceremony must have.</param>
        /// <param name="expectedChallenge">Challenge that the server issued.</param>
        /// <returns>True if the type, the challenge and the origin are the expected ones.</returns>
        public static bool IsValid(byte[] clientDataJson, string expectedType, byte[] expectedChallenge)
        {
            bool isValid = false;

            if (clientDataJson != null && expectedChallenge != null && clientDataJson.Length <= MaxClientDataSize)
            {
                ClientData clientData = Read(clientDataJson);

                isValid = clientData != null
                    && clientData.Type == expectedType
                    && clientData.Challenge == PasskeyRules.Encode(expectedChallenge)
                    && clientData.Origin == PasskeyRules.Origin;
            }

            return isValid;
        }

        private static bool TryConvert(string padded, out byte[] data)
        {
            bool isValid;

            try
            {
                data = Convert.FromBase64String(padded);
                isValid = true;
            }
            catch (FormatException)
            {
                data = null;
                isValid = false;
            }

            return isValid;
        }

        private sealed class DerReader
        {
            private readonly byte[] _data;
            private int _position;

            public DerReader(byte[] data)
            {
                _data = data;
            }

            public bool IsAtEnd => _position == _data.Length;

            public bool TryReadSignature(int partSize, out byte[] signature)
            {
                byte[] r = null;
                byte[] s = null;
                bool isValid = TryReadSequence() && TryReadInteger(partSize, out r)
                    && TryReadInteger(partSize, out s) && IsAtEnd;

                signature = null;

                if (isValid)
                {
                    signature = new byte[partSize * PartCount];
                    Array.Copy(r, 0, signature, 0, partSize);
                    Array.Copy(s, 0, signature, partSize, partSize);
                }

                return isValid;
            }

            private bool TryReadSequence()
            {
                int length = 0;
                bool isValid = TryReadTag(SequenceTag) && TryReadLength(out length);

                if (isValid)
                {
                    isValid = length == _data.Length - _position;
                }

                return isValid;
            }

            private bool TryReadInteger(int partSize, out byte[] value)
            {
                int length = 0;
                bool isValid = TryReadTag(IntegerTag) && TryReadLength(out length)
                    && length > 0 && length <= _data.Length - _position;

                value = null;

                if (isValid)
                {
                    value = Pad(length, partSize);
                    isValid = value != null;
                    _position += length;
                }

                return isValid;
            }

            private bool TryReadTag(byte tag)
            {
                bool isValid = _position < _data.Length && _data[_position] == tag;

                if (isValid)
                {
                    _position++;
                }

                return isValid;
            }

            private bool TryReadLength(out int length)
            {
                bool isValid = _position < _data.Length;

                length = 0;

                if (isValid)
                {
                    int first = _data[_position];

                    _position++;

                    if (first < LongFormFlag)
                    {
                        length = first;
                    }
                    else if (first == OneByteLongForm && _position < _data.Length)
                    {
                        length = _data[_position];
                        _position++;
                    }
                    else
                    {
                        isValid = false;
                    }
                }

                return isValid;
            }

            private byte[] Pad(int length, int partSize)
            {
                int start = _position;
                int end = _position + length;
                byte[] padded = null;

                while (start < end - 1 && _data[start] == 0)
                {
                    start++;
                }

                int significant = end - start;

                if (significant <= partSize)
                {
                    padded = new byte[partSize];
                    Array.Copy(_data, start, padded, partSize - significant, significant);
                }

                return padded;
            }
        }

        private static ClientData Read(byte[] clientDataJson)
        {
            ClientData clientData;

            try
            {
                using (var stream = new MemoryStream(clientDataJson))
                {
                    clientData = new DataContractJsonSerializer(typeof(ClientData)).ReadObject(stream) as ClientData;
                }
            }
            catch (SerializationException)
            {
                clientData = null;
            }
            catch (XmlException)
            {
                clientData = null;
            }

            return clientData;
        }

        [DataContract]
        private sealed class ClientData
        {
            [DataMember(Name = "type")]
            public string Type { get; set; }

            [DataMember(Name = "challenge")]
            public string Challenge { get; set; }

            [DataMember(Name = "origin")]
            public string Origin { get; set; }
        }
    }
}
