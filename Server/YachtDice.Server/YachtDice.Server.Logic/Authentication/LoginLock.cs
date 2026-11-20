namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// How long an account cannot log in after too many wrong passwords.
    /// </summary>
    public enum LoginLock
    {
        /// <summary>The account can log in.</summary>
        None,

        /// <summary>Three wrong passwords in a row: the account waits five minutes.</summary>
        Temporary,

        /// <summary>Ten wrong passwords in the same day: the account waits twenty-four hours.</summary>
        Daily,

        /// <summary>Several different players reported the player in one day: the account waits a day.</summary>
        Reported
    }
}
