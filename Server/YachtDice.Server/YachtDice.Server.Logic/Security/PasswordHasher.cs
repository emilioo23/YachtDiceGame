using System;
using System.Security.Cryptography;
using System.Text;

namespace YachtDice.Server.Logic.Security
{
    /// <summary>
    /// Utility to generate and verify SHA-256 password hashes.
    /// </summary>
    public static class PasswordHasher
    {
        private const string HexByteFormat = "x2";
        private const string EmptyValueMessage = "The value cannot be empty.";

        /// <summary>
        /// Computes the SHA-256 hash of a plain-text password.
        /// </summary>
        /// <param name="plainPassword">Password in plain text.</param>
        /// <returns>The hash as a lowercase hexadecimal string.</returns>
        public static string ComputeHash(string plainPassword)
        {
            if (string.IsNullOrWhiteSpace(plainPassword))
            {
                throw new ArgumentException(EmptyValueMessage, nameof(plainPassword));
            }

            byte[] hashBytes;

            using (SHA256 sha256 = SHA256.Create())
            {
                hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(plainPassword));
            }

            return ConvertBytesToHex(hashBytes);
        }

        /// <summary>
        /// Compares a plain-text password against a stored hash.
        /// </summary>
        /// <param name="plainPassword">Password in plain text.</param>
        /// <param name="storedHash">Hash stored in the database.</param>
        /// <returns>True if the password matches the stored hash.</returns>
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

            string computedHash = ComputeHash(plainPassword);

            return string.Equals(computedHash, storedHash, StringComparison.OrdinalIgnoreCase);
        }

        private static string ConvertBytesToHex(byte[] bytes)
        {
            var builder = new StringBuilder();

            foreach (byte currentByte in bytes)
            {
                builder.Append(currentByte.ToString(HexByteFormat));
            }

            return builder.ToString();
        }
    }
}