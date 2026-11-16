using System.Linq;

namespace YachtDice.Client.Logic.Authentication
{
    /// <summary>
    /// Checks the backup code that the player writes instead of the code sent by email, before asking the server.
    /// </summary>
    public static class BackupCodeInputValidator
    {
        /// <summary>
        /// Number of digits in a backup code. It must match the length that the server creates.
        /// </summary>
        public const int BackupCodeLength = 8;

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
    }
}
