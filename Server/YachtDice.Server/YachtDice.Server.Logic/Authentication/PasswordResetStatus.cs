namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Outcome of the confirmation of a password recovery.
    /// </summary>
    public enum PasswordResetStatus
    {
        /// <summary>The code was correct and the password was changed.</summary>
        PasswordChanged,

        /// <summary>The email, the code or the new password was not written.</summary>
        MissingRequiredField,

        /// <summary>The code does not match the one that was sent.</summary>
        IncorrectCode,

        /// <summary>The code has expired or none was requested.</summary>
        CodeExpired,

        /// <summary>Too many wrong codes were entered, so the code was discarded.</summary>
        TooManyAttempts,

        /// <summary>The new password is too short, too long or does not have a letter and a digit.</summary>
        WeakPassword
    }
}
