namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Outcome of a request to receive a password recovery code.
    /// </summary>
    public enum PasswordResetRequestStatus
    {
        /// <summary>
        /// The request was accepted. The same answer is given when the email is not registered or when a code was
        /// sent a moment ago, so nobody can find out which emails have an account.
        /// </summary>
        CodeSent,

        /// <summary>The email was not written.</summary>
        MissingRequiredField,

        /// <summary>The code was generated but the email could not be sent.</summary>
        CodeSendFailed
    }
}
