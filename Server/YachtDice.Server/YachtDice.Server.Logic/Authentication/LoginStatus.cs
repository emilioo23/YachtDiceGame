namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Outcome of the first step of the login (credentials check and code delivery).
    /// </summary>
    public enum LoginStatus
    {
        /// <summary>The credentials are correct and the verification code was sent.</summary>
        VerificationCodeSent,

        /// <summary>The email or the password was not provided.</summary>
        MissingRequiredField,

        /// <summary>The email or the password is incorrect.</summary>
        InvalidCredentials,

        /// <summary>
        /// The email is empty or is not written like an email address.
        /// </summary>
        InvalidEmail,

        /// <summary>
        /// The email is well written but no player has it.
        /// </summary>
        EmailNotRegistered,

        /// <summary>
        /// The player exists but the password does not match.
        /// </summary>
        InvalidPassword,

        /// <summary>The two-step verification is locked because of too many failed attempts.</summary>
        VerificationLocked,

        /// <summary>The credentials are correct but the verification code could not be sent.</summary>
        CodeSendFailed,

        /// <summary>The credentials are correct and the player has no two-step verification.</summary>
        LoggedIn
    }
}