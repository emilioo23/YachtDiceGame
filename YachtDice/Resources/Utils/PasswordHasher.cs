using System;
using System.Security.Cryptography;
using System.Text;

namespace YachtDice.Utils
{
    /// <summary>
    /// Utility to generate and verify SHA-256 password hashes.
    /// </summary>
    public static class PasswordHasher
    {
        /// <summary>
        /// Computes the SHA-256 hash of a plain-text password,
        /// returned as a lowercase hexadecimal string.
        /// </summary>
        public static string ComputeHash(string plainPassword)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(plainPassword);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                return ConvertBytesToHex(hashBytes);
            }
        }

        /// <summary>
        /// Compares a plain-text password against a stored hash.
        /// </summary>
        public static bool Verify(string plainPassword, string storedHash)
        {
            string computedHash = ComputeHash(plainPassword);
            return string.Equals(computedHash, storedHash, StringComparison.OrdinalIgnoreCase);
        }

        private static string ConvertBytesToHex(byte[] bytes)
        {
            var builder = new StringBuilder();

            foreach (byte currentByte in bytes)
            {
                builder.Append(currentByte.ToString("x2"));
            }

            return builder.ToString();
        }
    }
}