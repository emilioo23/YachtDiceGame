using System.Linq;

namespace YachtDice.Client.Logic.Authentication
{
    /// <summary>
    /// Checks and texts of the data that the player writes in the windows of the account.
    /// </summary>
    public static class AccountInputRules
    {
        /// <summary>
        /// Number of digits in a backup code. It must match the length that the server creates.
        /// </summary>
        public const int BackupCodeLength = 8;

        /// <summary>
        /// Number of digits of the recovery code.
        /// </summary>
        public const int CodeLength = 6;

        private const int VisibleLocalPartChars = 2;
        private const int FirstCharacterIndex = 0;
        private const char MaskCharacter = '*';
        private const char AtSign = '@';

        /// <summary>
        /// Indicates whether the text has the shape of a backup code: only digits, with the exact length.
        /// </summary>
        /// <param name="code">Text written by the player, without the spaces around it.</param>
        /// <returns>True if the code can be sent to the server.</returns>
        public static bool IsValid(string code)
        {
            bool hasBackupLength = code != null && code.Length == BackupCodeLength;

            return hasBackupLength && code.All(char.IsDigit);
        }

        /// <summary>
        /// Checks that the three passwords were written and that the confirmation matches the new password.
        /// </summary>
        /// <param name="currentPassword">Password that the player has now.</param>
        /// <param name="newPassword">Password that the player wants.</param>
        /// <param name="confirmation">New password written a second time.</param>
        /// <returns>The first problem found, or <see cref="PasswordChangeStatus.Valid"/>.</returns>
        public static PasswordChangeStatus ValidatePasswordChange(
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

        /// <summary>
        /// Keeps the first characters of the address and the domain, and replaces the rest with asterisks.
        /// </summary>
        /// <param name="email">Email address to hide.</param>
        /// <returns>The hidden address, or the same address if its local part is too short to hide.</returns>
        public static string Mask(string email)
        {
            int atIndex = email.IndexOf(AtSign);
            string maskedEmail = email;

            if (atIndex > VisibleLocalPartChars)
            {
                string visiblePart = email.Substring(FirstCharacterIndex, VisibleLocalPartChars);
                string hiddenPart = new string(MaskCharacter, atIndex - VisibleLocalPartChars);

                maskedEmail = visiblePart + hiddenPart + email.Substring(atIndex);
            }

            return maskedEmail;
        }

        /// <summary>
        /// Checks the code, the new password and its confirmation.
        /// </summary>
        /// <param name="code">Code that the player received by email.</param>
        /// <param name="newPassword">New password written by the player.</param>
        /// <param name="confirmation">New password written again by the player.</param>
        /// <returns>The first problem found, or <see cref="ResetPasswordStatus.Valid"/>.</returns>
        public static ResetPasswordStatus ValidateResetPassword(string code, string newPassword, string confirmation)
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
