using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace YachtDice.Server.Logic.Security
{
    /// <summary>
    /// Utility to hash and verify passwords. A new password is saved with PBKDF2 (HMAC-SHA256), a random salt
    /// and a high number of iterations, so two equal passwords give different hashes and trying guesses against
    /// a stolen table is slow. The hashes of the accounts created before this rule (a plain SHA-256 without
    /// salt) are still verified, so those players can log in.
    /// </summary>
    public static class PasswordHasher
    {
        private const string Algorithm = "pbkdf2-sha256";
        private const char Separator = '$';
        private const int Iterations = 100000;
        private const int SaltBytes = 16;
        private const int KeyBytes = 32;
        private const int PartsCount = 4;
        private const int IterationsPart = 1;
        private const int SaltPart = 2;
        private const int KeyPart = 3;
        private const int NoIterations = 0;
        private const string HexByteFormat = "x2";
        private const string EmptyValueMessage = "The value cannot be empty.";
        private const string SaltedCodeFormat = "{0}:{1}";
        private const string SaltedHashFormat = "{0}{1}{2}{1}{3}{1}{4}";

        /// <summary>
        /// Computes the salted hash of a plain-text password.
        /// </summary>
        /// <param name="plainPassword">Password in plain text.</param>
        /// <returns>The algorithm, the iterations, the salt and the key, separated by dollar signs.</returns>
        /// <exception cref="ArgumentException">The password is empty or only spaces.</exception>
        public static string ComputeHash(string plainPassword)
        {
            if (string.IsNullOrWhiteSpace(plainPassword))
            {
                throw new ArgumentException(EmptyValueMessage, nameof(plainPassword));
            }

            byte[] salt = new byte[SaltBytes];

            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(salt);
            }

            byte[] key = DeriveKey(plainPassword, salt, Iterations);

            return string.Format(
                CultureInfo.InvariantCulture,
                SaltedHashFormat,
                Algorithm,
                Separator,
                Iterations,
                Convert.ToBase64String(salt),
                Convert.ToBase64String(key));
        }

        /// <summary>
        /// Checks a plain-text password against a hash that was saved, comparing in constant time so the
        /// answer does not depend on how many characters match.
        /// </summary>
        /// <param name="plainPassword">Password written by the player.</param>
        /// <param name="storedHash">Hash saved in the database, salted or from the old SHA-256 format.</param>
        /// <returns>True if the password is the one that produced the hash.</returns>
        /// <exception cref="ArgumentException">The password or the saved hash is empty.</exception>
        public static bool Verify(string plainPassword, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(plainPassword))
            {
                throw new ArgumentException(EmptyValueMessage, nameof(plainPassword));
            }

            if (string.IsNullOrWhiteSpace(storedHash))
            {
                throw new ArgumentException(EmptyValueMessage, nameof(storedHash));
            }

            bool isValid = IsSalted(storedHash)
                ? VerifySalted(plainPassword, storedHash)
                : VerifyLegacy(plainPassword, storedHash);

            return isValid;
        }

        /// <summary>
        /// Computes the hash that is saved instead of a recovery code. It always gives the same result for the
        /// same player and code, because the server compares it with the one saved when the code was sent.
        /// </summary>
        /// <param name="playerId">Identifier of the player, mixed in so equal codes give different hashes.</param>
        /// <param name="code">Code that was sent by email.</param>
        /// <returns>The hash as a lowercase hexadecimal string.</returns>
        public static string ComputeResetCodeHash(int playerId, string code)
        {
            string saltedCode = string.Format(CultureInfo.InvariantCulture, SaltedCodeFormat, playerId, code);

            return ComputeSha256(saltedCode);
        }

        private static bool IsSalted(string storedHash)
        {
            return storedHash.StartsWith(Algorithm + Separator, StringComparison.Ordinal);
        }

        private static bool VerifySalted(string plainPassword, string storedHash)
        {
            string[] parts = storedHash.Split(Separator);
            bool isValid = false;

            if (parts.Length == PartsCount && TryReadIterations(parts[IterationsPart], out int iterations))
            {
                isValid = MatchesSaltedKey(plainPassword, parts[SaltPart], parts[KeyPart], iterations);
            }

            return isValid;
        }

        private static bool TryReadIterations(string text, out int iterations)
        {
            bool isNumber = int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out iterations);

            return isNumber && iterations > NoIterations;
        }

        private static bool MatchesSaltedKey(string plainPassword, string saltText, string keyText, int iterations)
        {
            bool isValid;

            try
            {
                byte[] salt = Convert.FromBase64String(saltText);
                byte[] expectedKey = Convert.FromBase64String(keyText);

                isValid = AreEqual(DeriveKey(plainPassword, salt, iterations), expectedKey);
            }
            catch (FormatException)
            {
                isValid = false;
            }

            return isValid;
        }

        private static bool VerifyLegacy(string plainPassword, string storedHash)
        {
            byte[] computed = Encoding.UTF8.GetBytes(ComputeSha256(plainPassword));
            byte[] stored = Encoding.UTF8.GetBytes(storedHash.ToLowerInvariant());

            return AreEqual(computed, stored);
        }

        private static byte[] DeriveKey(string plainPassword, byte[] salt, int iterations)
        {
            using (var derivation = new Rfc2898DeriveBytes(plainPassword, salt, iterations, HashAlgorithmName.SHA256))
            {
                return derivation.GetBytes(KeyBytes);
            }
        }

        private static bool AreEqual(byte[] first, byte[] second)
        {
            int difference = first.Length ^ second.Length;

            for (int index = 0; index < first.Length && index < second.Length; index++)
            {
                difference |= first[index] ^ second[index];
            }

            return difference == 0;
        }

        private static string ComputeSha256(string text)
        {
            byte[] hashBytes;

            using (SHA256 sha256 = SHA256.Create())
            {
                hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
            }

            var builder = new StringBuilder();

            foreach (byte currentByte in hashBytes)
            {
                builder.Append(currentByte.ToString(HexByteFormat, CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }
    }
}
