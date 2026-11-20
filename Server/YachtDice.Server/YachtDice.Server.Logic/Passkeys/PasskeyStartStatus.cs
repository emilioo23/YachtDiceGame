namespace YachtDice.Server.Logic.Passkeys
{
    /// <summary>
    /// Outcome of asking for the challenge of a passkey ceremony.
    /// </summary>
    public enum PasskeyStartStatus
    {
        /// <summary>The challenge was issued.</summary>
        Created,

        /// <summary>The player of the session does not exist.</summary>
        PlayerNotFound,

        /// <summary>The player already has the maximum number of passkeys.</summary>
        TooManyPasskeys,

        /// <summary>Too many ceremonies are waiting for an answer; the player must try again later.</summary>
        Busy
    }
}
