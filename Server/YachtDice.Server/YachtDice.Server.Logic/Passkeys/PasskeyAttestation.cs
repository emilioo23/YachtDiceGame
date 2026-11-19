namespace YachtDice.Server.Logic.Passkeys
{
    /// <summary>
    /// Answer of the authenticator when the player creates a passkey. Every field is in base64url.
    /// </summary>
    public sealed class PasskeyAttestation
    {
        /// <summary>
        /// Identifier of the challenge that was answered.
        /// </summary>
        public string ChallengeId { get; set; }

        /// <summary>
        /// Client data JSON that the client signed with the authenticator.
        /// </summary>
        public string ClientDataJson { get; set; }

        /// <summary>
        /// Attestation object with the authenticator data and the new public key.
        /// </summary>
        public string AttestationObject { get; set; }

        /// <summary>
        /// Transport code that Windows reported for the authenticator; zero if it did not report one.
        /// </summary>
        public int Transport { get; set; }
    }
}
