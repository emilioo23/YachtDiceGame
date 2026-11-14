using System;
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

        private const string NullInputMessage = "The input of the password reset cannot be null.";

        /// <summary>
        /// Checks the code, the new password and its confirmation.
        /// </summary>
        /// <param name="input">Texts written by the player.</param>
        /// <returns>The first problem found, or <see cref="ResetPasswordInputStatus.Valid"/>.</returns>
        /// <exception cref="ArgumentNullException">The input is null.</exception>
        public static ResetPasswordInputStatus Validate(ResetPasswordInput input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input), NullInputMessage);
            }

            ResetPasswordInputStatus status = ResetPasswordInputStatus.Valid;

            if (!IsCompleteCode(input.Code))
            {
                status = ResetPasswordInputStatus.IncompleteCode;
            }
            else if (string.IsNullOrWhiteSpace(input.NewPassword))
            {
                status = ResetPasswordInputStatus.EmptyPassword;
            }
            else if (input.NewPassword != input.Confirmation)
            {
                status = ResetPasswordInputStatus.PasswordsDoNotMatch;
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
