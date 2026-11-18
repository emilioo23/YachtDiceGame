namespace YachtDice.Client.Logic.Passkeys
{
    /// <summary>
    /// Values of the passkey ceremonies that the client and the server must share.
    /// </summary>
    public static class PasskeyRules
    {
        /// <summary>
        /// Relying party identifier of the game. It is the same one that the server checks in the answer.
        /// </summary>
        public const string RelyingPartyId = "yachtdice.localhost";

        /// <summary>
        /// Origin written in the client data. It is the same one that the server expects.
        /// </summary>
        public const string Origin = "https://yachtdice.localhost";

        /// <summary>
        /// Name of the game that Windows shows when it asks for the passkey.
        /// </summary>
        public const string RelyingPartyName = "Yacht Dice";

        /// <summary>
        /// Type of the client data of the creation of a passkey.
        /// </summary>
        public const string CreateClientDataType = "webauthn.create";

        /// <summary>
        /// Type of the client data of the login with a passkey.
        /// </summary>
        public const string GetClientDataType = "webauthn.get";

        /// <summary>
        /// Time that the player has to answer the Windows dialog, in milliseconds.
        /// </summary>
        public const int TimeoutMilliseconds = 120000;

        /// <summary>
        /// COSE identifier of ECDSA with P-256 and SHA-256, the algorithm of Windows Hello and of the phones.
        /// </summary>
        public const int EcdsaAlgorithm = -7;

        /// <summary>
        /// COSE identifier of RSA with PKCS#1 v1.5 and SHA-256, which some security keys use.
        /// </summary>
        public const int RsaAlgorithm = -257;
    }
}
