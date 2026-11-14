namespace YachtDice.Client.Logic.Authentication
{
    /// <summary>
    /// Result of checking what the player wrote to reset the password, before asking the server.
    /// </summary>
    public enum ResetPasswordInputStatus
    {
        /// <summary>Everything the player wrote can be sent to the server.</summary>
        Valid,

        /// <summary>The code does not have the expected number of digits.</summary>
        IncompleteCode,

        /// <summary>The new password was not written.</summary>
        EmptyPassword,

        /// <summary>The confirmation is different from the new password.</summary>
        PasswordsDoNotMatch
    }
}
