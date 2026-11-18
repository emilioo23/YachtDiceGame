namespace YachtDice.Server.Logic.Passkeys
{
    /// <summary>
    /// Fixed values of the passkeys (WebAuthn) that the server and the client must share.
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
    }
}
