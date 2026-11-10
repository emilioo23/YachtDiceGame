using System.Linq;

namespace YachtDice.Client.Logic.Authentication
{
    /// <summary>
    /// Checks the data written to reset the password, so obvious mistakes are shown without calling the server.
    /// </summary>
    public static class ResetPasswordInputValidator
    {
        /// <summary>
        /// Number of digits of the recovery code.
        /// </summary>
        public const int CodeLength = 6;

        /// <summary>
        /// Checks the code, the new password and its confirmation.
        /// </summary>
        /// <param name="code">Code written by the player.</param>
        /// <param name="newPassword">New password written by the player.</param>
        /// <param name="confirmation">Confirmation of the new password.</param>
        /// <returns>The first problem found, or <see cref="ResetPasswordInputStatus.Valid"/>.</returns>
        public static ResetPasswordInputStatus Validate(string code, string newPassword, string confirmation)
        {
            ResetPasswordInputStatus status = ResetPasswordInputStatus.Valid;

            if (!IsCompleteCode(code))
            {
                status = ResetPasswordInputStatus.IncompleteCode;
            }
            else if (string.IsNullOrWhiteSpace(newPassword))
            {
                status = ResetPasswordInputStatus.EmptyPassword;
            }
            else if (newPassword != confirmation)
            {
                status = ResetPasswordInputStatus.PasswordsDoNotMatch;
            }

            return status;
        }

        private static bool IsCompleteCode(string code)
        {
            return code != null && code.Length == CodeLength && code.All(char.IsDigit);
        }
    }
}
