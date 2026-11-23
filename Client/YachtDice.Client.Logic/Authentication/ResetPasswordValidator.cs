using System.Linq;

namespace YachtDice.Client.Logic.Authentication
{
    /// <summary>
    /// Checks the data written to reset the password, so obvious mistakes are shown without calling the server.
    /// </summary>
    public static class ResetPasswordValidator
    {
        /// <summary>
        /// Number of digits of the recovery code.
        /// </summary>
        public const int CodeLength = 6;

        /// <summary>
        /// Checks the code, the new password and its confirmation.
        /// </summary>
        /// <param name="code">Code that the player received by email.</param>
        /// <param name="newPassword">New password written by the player.</param>
        /// <param name="confirmation">New password written again by the player.</param>
        /// <returns>The first problem found, or <see cref="ResetPasswordStatus.Valid"/>.</returns>
        public static ResetPasswordStatus Validate(string code, string newPassword, string confirmation)
        {
            ResetPasswordStatus status = ResetPasswordStatus.Valid;

            if (!IsCompleteCode(code))
            {
                status = ResetPasswordStatus.IncompleteCode;
            }
            else if (string.IsNullOrWhiteSpace(newPassword))
            {
                status = ResetPasswordStatus.EmptyPassword;
            }
            else if (newPassword != confirmation)
            {
                status = ResetPasswordStatus.PasswordsDoNotMatch;
            }

            return status;
        }

        private static bool IsCompleteCode(string code)
        {
            bool hasCodeLength = code != null && code.Length == CodeLength;

            return hasCodeLength && code.All(char.IsDigit);
        }
    }
}
