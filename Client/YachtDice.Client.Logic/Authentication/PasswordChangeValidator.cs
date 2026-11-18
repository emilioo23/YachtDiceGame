namespace YachtDice.Client.Logic.Authentication
{
    /// <summary>
    /// Checks the three passwords of the password change before the server is asked. The strength of the new
    /// password is not checked here: the server has the rules and always checks them again.
    /// </summary>
    public static class PasswordChangeValidator
    {
        /// <summary>
        /// Checks that the three passwords were written and that the confirmation matches the new password.
        /// </summary>
        /// <param name="currentPassword">Password that the player has now.</param>
        /// <param name="newPassword">Password that the player wants.</param>
        /// <param name="confirmation">New password written a second time.</param>
        /// <returns>The first problem found, or <see cref="PasswordChangeStatus.Valid"/>.</returns>
        public static PasswordChangeStatus Validate(
            string currentPassword,
            string newPassword,
            string confirmation)
        {
            PasswordChangeStatus status = PasswordChangeStatus.Valid;

            if (string.IsNullOrEmpty(currentPassword))
            {
                status = PasswordChangeStatus.EmptyCurrentPassword;
            }
            else if (string.IsNullOrWhiteSpace(newPassword))
            {
                status = PasswordChangeStatus.EmptyNewPassword;
            }
            else if (newPassword != confirmation)
            {
                status = PasswordChangeStatus.PasswordsDoNotMatch;
            }

            return status;
        }
    }
}
