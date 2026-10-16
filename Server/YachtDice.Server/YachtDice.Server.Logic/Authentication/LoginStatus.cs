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

        /// <summary>The two-step verification is locked because of too many failed attempts.</summary>
        VerificationLocked,

        /// <summary>The credentials are correct but the verification code could not be sent.</summary>
        CodeSendFailed
    }
}