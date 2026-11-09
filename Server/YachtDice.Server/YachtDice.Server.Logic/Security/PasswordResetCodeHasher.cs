using System.Globalization;

namespace YachtDice.Server.Logic.Security
{
    /// <summary>
    /// Builds the hash of a password recovery code. Only the hash is saved in the database, never the code.
    /// </summary>
    public static class PasswordResetCodeHasher
    {
        private const string SaltedCodeFormat = "{0}:{1}";

        /// <summary>
        /// Computes the hash that is saved for the recovery code of a player.
        /// </summary>
        /// <param name="playerId">Identifier of the player, used so equal codes of two players differ.</param>
        /// <param name="code">Recovery code in plain text.</param>
        /// <returns>The hash as a lowercase hexadecimal string.</returns>
        public static string Compute(int playerId, string code)
        {
            string saltedCode = string.Format(CultureInfo.InvariantCulture, SaltedCodeFormat, playerId, code);

            return PasswordHasher.ComputeHash(saltedCode);
        }
    }
}
