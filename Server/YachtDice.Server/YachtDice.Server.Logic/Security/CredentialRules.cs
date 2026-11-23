using System;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace YachtDice.Server.Logic.Security
{
    /// <summary>
    /// Rules of the credentials: validation of names, emails and passwords, and the hash of the passwords.
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

        /// <summary>
        /// Maximum number of characters of an email address. It is the size of the column in the database.
        /// </summary>
        public const int EmailMaxLength = 100;

        /// <summary>
        /// Minimum number of characters of a first name or a last name.
        /// </summary>
        public const int NameMinLength = 2;

        /// <summary>
        /// Maximum number of characters of a first name or a last name. It is the size of the column in the
        /// database.
        /// </summary>
        public const int NameMaxLength = 50;

        private const string UsernameCharactersPattern = "^[A-Za-z0-9._]+$";
        private const string PersonNamePattern = @"^\p{L}+([ '\-]\p{L}+)*$";
        private const int UsernameRegexTimeoutMilliseconds = 100;
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
        private static readonly Regex UsernameCharacters = new Regex(
            UsernameCharactersPattern,
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(UsernameRegexTimeoutMilliseconds));
        private static readonly Regex PersonNameCharacters = new Regex(
            PersonNamePattern,
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(UsernameRegexTimeoutMilliseconds));

        /// <summary>
        /// Indicates whether a first name or a last name has a valid length and is made only of letters, with
        /// single spaces, hyphens or apostrophes between the words (for example "María José", "Anne-Marie" or
        /// "O'Brien"). Digits and symbols are not allowed.
        /// </summary>
        /// <param name="name">Name already trimmed.</param>
        /// <returns>True if the name can be used.</returns>
        public static bool IsValidPersonName(string name)
        {
            bool hasValidLength = name != null && name.Length >= NameMinLength && name.Length <= NameMaxLength;

            return hasValidLength && PersonNameCharacters.IsMatch(name);
        }

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

            bool hasLetter = hasValidLength && password.Any(char.IsLetter);
            bool hasLetterAndDigit = hasLetter && password.Any(char.IsDigit);

            return hasLetterAndDigit;
        }

        /// <summary>
        /// Tells whether the text is an email address written correctly.
        /// </summary>
        /// <param name="email">Text to check, without spaces around it.</param>
        /// <returns>True if the text is a valid email address; false for empty or malformed text.</returns>
        public static bool IsValid(string email)
        {
            var isValid = false;

            if (!string.IsNullOrWhiteSpace(email) && email.Length <= CredentialRules.EmailMaxLength)
            {
                try
                {
                    var address = new MailAddress(email);
                    isValid = address.Address == email;
                }
                catch (FormatException)
                {
                    isValid = false;
                }
            }

            return isValid;
        }

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

            bool isValid;

            if (IsSalted(storedHash))
            {
                isValid = VerifySalted(plainPassword, storedHash);
            }
            else
            {
                isValid = VerifyLegacy(plainPassword, storedHash);
            }

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
                isValid = MatchesSaltedKey(plainPassword, parts, iterations);
            }

            return isValid;
        }

        private static bool TryReadIterations(string text, out int iterations)
        {
            bool isNumber = int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out iterations);

            return isNumber && iterations > NoIterations;
        }

        private static bool MatchesSaltedKey(string plainPassword, string[] parts, int iterations)
        {
            bool isValid;

            try
            {
                byte[] salt = Convert.FromBase64String(parts[SaltPart]);
                byte[] expectedKey = Convert.FromBase64String(parts[KeyPart]);

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
            byte[] key;

            using (var derivation = new Rfc2898DeriveBytes(plainPassword, salt, iterations, HashAlgorithmName.SHA256))
            {
                key = derivation.GetBytes(KeyBytes);
            }

            return key;
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
