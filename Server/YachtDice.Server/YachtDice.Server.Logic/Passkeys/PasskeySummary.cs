namespace YachtDice.Server.Logic.Passkeys
{
    /// <summary>
    /// What the player sees of a passkey: an identifier to remove it and a name to recognize it.
    /// </summary>
    public sealed class PasskeySummary
    {
        /// <summary>
        /// Identifier of the passkey.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name that was given to the passkey when it was created.
        /// </summary>
        public string Name { get; set; }
    }
}
