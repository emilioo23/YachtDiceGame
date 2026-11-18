namespace YachtDice.Server.Logic.Passkeys
{
    /// <summary>
    /// Outcome of saving a new passkey.
    /// </summary>
    public enum PasskeyRegistrationStatus
    {
        /// <summary>The passkey was saved.</summary>
        Registered,

        /// <summary>The answer of the authenticator is not valid.</summary>
        InvalidResponse,

        /// <summary>The challenge was already used or expired.</summary>
        ChallengeExpired,

        /// <summary>The player already has the maximum number of passkeys.</summary>
        TooManyPasskeys,

        /// <summary>The credential is already registered.</summary>
        AlreadyRegistered,

        /// <summary>The player of the session does not exist.</summary>
        PlayerNotFound
    }
}
