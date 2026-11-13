namespace YachtDice.Client.Logic.Authentication
{
    /// <summary>
    /// What the player typed in the window to change the password after the recovery.
    /// </summary>
    public sealed class ResetPasswordInput
    {
        /// <summary>
        /// Initializes the input with the three texts of the window.
        /// </summary>
        /// <param name="code">Recovery code typed by the player.</param>
        /// <param name="newPassword">New password typed by the player.</param>
        /// <param name="confirmation">Second time that the player typed the new password.</param>
        public ResetPasswordInput(string code, string newPassword, string confirmation)
        {
            Code = code;
            NewPassword = newPassword;
            Confirmation = confirmation;
        }

        /// <summary>
        /// Gets the recovery code typed by the player.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Gets the new password typed by the player.
        /// </summary>
        public string NewPassword { get; }

        /// <summary>
        /// Gets the second time that the player typed the new password.
        /// </summary>
        public string Confirmation { get; }
    }
}
