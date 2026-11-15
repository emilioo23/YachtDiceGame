namespace YachtDice.Client.Logic.Authentication
{
    /// <summary>
    /// Checks the three passwords of the password change before the server is asked. The strength of the new
    /// password is not checked here: the server has the rules and always checks them again.
    /// </summary>
    public static class PasswordChangeInputValidator
    {
        /// <summary>
        /// Checks that the three passwords were written and that the confirmation matches the new password.
        /// </summary>
        /// <param name="currentPassword">Password that the player has now.</param>
        /// <param name="newPassword">Password that the player wants.</param>
        /// <param name="confirmation">New password written a second time.</param>
        /// <returns>The first problem found, or <see cref="PasswordChangeInputStatus.Valid"/>.</returns>
        public static PasswordChangeInputStatus Validate(
            string currentPassword,
            string newPassword,
            string confirmation)
        {
            PasswordChangeInputStatus status = PasswordChangeInputStatus.Valid;

            if (string.IsNullOrEmpty(currentPassword))
            {
                status = PasswordChangeInputStatus.EmptyCurrentPassword;
            }
            else if (string.IsNullOrWhiteSpace(newPassword))
            {
                status = PasswordChangeInputStatus.EmptyNewPassword;
            }
            else if (newPassword != confirmation)
            {
                status = PasswordChangeInputStatus.PasswordsDoNotMatch;
            }

            return status;
        }
    }
}
