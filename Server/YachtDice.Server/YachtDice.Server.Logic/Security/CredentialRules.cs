using System.Linq;
using System.Text.RegularExpressions;

namespace YachtDice.Server.Logic.Security
{
    /// <summary>
    /// Rules that a new username and a new password must follow. They are checked when an account is created
    /// and when a password is changed, so the accounts that already exist are not affected.
    /// </summary>
    public static class CredentialRules
    {
        /// <summary>
        /// Minimum number of characters of a username.
        /// </summary>
        public const int UsernameMinLength = 3;

        /// <summary>
        /// Maximum number of characters of a username. It is the size of the column in the database.
        /// </summary>
        public const int UsernameMaxLength = 20;

        /// <summary>
        /// Minimum number of characters of a password.
        /// </summary>
        public const int PasswordMinLength = 8;

        /// <summary>
        /// Maximum number of characters of a password.
        /// </summary>
        public const int PasswordMaxLength = 64;

        private const string UsernameCharactersPattern = "^[A-Za-z0-9._]+$";

        private static readonly Regex UsernameCharacters = new Regex(UsernameCharactersPattern, RegexOptions.Compiled);

        /// <summary>
        /// Indicates whether the username has a valid length and only uses letters without accents, digits,
        /// dots and underscores.
        /// </summary>
        /// <param name="username">Username already trimmed.</param>
        /// <returns>True if the username can be used.</returns>
        public static bool IsValidUsername(string username)
        {
            bool hasValidLength = username != null
                && username.Length >= UsernameMinLength
                && username.Length <= UsernameMaxLength;

            return hasValidLength && UsernameCharacters.IsMatch(username);
        }

        /// <summary>
        /// Indicates whether the password has a valid length and at least one letter and one digit.
        /// </summary>
        /// <param name="password">Password written by the player.</param>
        /// <returns>True if the password is strong enough.</returns>
        public static bool IsStrongPassword(string password)
        {
            bool hasValidLength = password != null
                && password.Length >= PasswordMinLength
                && password.Length <= PasswordMaxLength;

            bool hasLetterAndDigit = hasValidLength && password.Any(char.IsLetter) && password.Any(char.IsDigit);

            return hasLetterAndDigit;
        }
    }
}
