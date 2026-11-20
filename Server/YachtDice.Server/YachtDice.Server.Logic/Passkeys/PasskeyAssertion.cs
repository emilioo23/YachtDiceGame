namespace YachtDice.Server.Logic.Passkeys
{
    /// <summary>
    /// Answer of the authenticator when the player logs in with a passkey. Every field is in base64url.
    /// </summary>
    public sealed class PasskeyAssertion
    {
        /// <summary>
        /// Identifier of the challenge that was answered.
        /// </summary>
        public string ChallengeId { get; set; }

        /// <summary>
        /// Identifier of the credential that signed.
        /// </summary>
        public string CredentialId { get; set; }

        /// <summary>
        /// Client data JSON that the client signed with the authenticator.
        /// </summary>
        public string ClientDataJson { get; set; }

        /// <summary>
        /// Authenticator data that the authenticator signed.
        /// </summary>
        public string AuthenticatorData { get; set; }

        /// <summary>
        /// Signature of the authenticator data and the hash of the client data.
        /// </summary>
        public string Signature { get; set; }

        /// <summary>
        /// Identifier of the player that the authenticator saved with the credential; it may be empty.
        /// </summary>
        public string UserHandle { get; set; }
    }
}
