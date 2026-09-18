using System;
using System.Security.Cryptography;
using System.Text;

namespace YachtDice.Utils
{
    /// <summary>
    /// Utilidad para generar y verificar hashes SHA-256 de contraseñas.
    /// </summary>
    public static class PasswordHasher
    {
        /// <summary>
        /// Calcula el hash SHA-256 de una contraseña en texto plano,
        /// devuelto como cadena hexadecimal en minúsculas.
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
        /// Compara una contraseña en texto plano contra un hash almacenado.
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