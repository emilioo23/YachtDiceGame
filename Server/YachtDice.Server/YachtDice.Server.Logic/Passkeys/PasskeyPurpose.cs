namespace YachtDice.Server.Logic.Passkeys
{
    /// <summary>
    /// What a challenge of a passkey was issued for.
    /// </summary>
    public enum PasskeyPurpose
    {
        /// <summary>The player is creating a passkey.</summary>
        Registration,

        /// <summary>The player is logging in with a passkey.</summary>
        Login
    }
}
