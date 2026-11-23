

namespace YachtDice.Server.Logic.Passkeys
{
    /// <summary>
    /// Limits, names and transport codes that the passkeys use.
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
    }
}
