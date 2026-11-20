namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Passkey of a player together with the data of the player that the login needs.
    /// </summary>
    public sealed class PasskeyRecord
    {
        /// <summary>
        /// Identifier of the passkey in the database; zero if it was not saved yet.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identifier of the player that owns the passkey.
        /// </summary>
        public int PlayerId { get; set; }

        /// <summary>
        /// Identifier of the credential, in base64url.
        /// </summary>
        public string CredentialId { get; set; }

        /// <summary>
        /// Public key of the credential in COSE format, in base64url.
        /// </summary>
        public string PublicKey { get; set; }

        /// <summary>
        /// Number of signatures that the authenticator reported the last time it was used.
        /// </summary>
        public long SignCount { get; set; }

        /// <summary>
        /// Name that identifies the passkey in the lists.
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// Name that the player shows in the game.
        /// </summary>
        public string PlayerDisplayName { get; set; }

        /// <summary>
        /// First name of the player, used only to greet them.
        /// </summary>
        public string PlayerFirstName { get; set; }
    }
}
