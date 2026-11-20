namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Data the player writes to finish a password recovery.
    /// </summary>
    public class PasswordResetConfirmation
    {
        /// <summary>
        /// Gets or sets the email of the account to recover.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the code received by email.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the new password in plain text.
        /// </summary>
        public string NewPassword { get; set; }
    }
}
