using System;
using YachtDice.Client.Logic.Logging;

namespace YachtDice.Client.Logic.Passkeys
{
    /// <summary>
    /// Writes and reads the base64url text that the server uses for the binary values of the passkeys.
    /// </summary>
    public static class Base64Url
    {
        private const char StandardPlus = '+';
        private const char StandardSlash = '/';
        private const char UrlMinus = '-';
        private const char UrlUnderscore = '_';
        private const char Padding = '=';
        private const int GroupSize = 4;

        /// <summary>
        /// Writes the bytes as base64url without padding.
        /// </summary>
        /// <param name="data">Bytes to write.</param>
        /// <returns>The base64url text.</returns>
        public static string Encode(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), LogMessages.NullDataMessage);
            }

            return Convert.ToBase64String(data)
                .TrimEnd(Padding)
                .Replace(StandardPlus, UrlMinus)
                .Replace(StandardSlash, UrlUnderscore);
        }

        /// <summary>
        /// Reads a base64url text, with or without padding.
        /// </summary>
        /// <param name="text">Text to read.</param>
        /// <param name="data">The bytes, or null if the text is not valid.</param>
        /// <returns>True if the text is valid base64url.</returns>
        public static bool TryDecode(string text, out byte[] data)
        {
            bool isValid = false;

            data = null;

            if (!string.IsNullOrEmpty(text))
            {
                string standard = text.Replace(UrlMinus, StandardPlus).Replace(UrlUnderscore, StandardSlash);
                int missing = (GroupSize - (standard.Length % GroupSize)) % GroupSize;

                try
                {
                    data = Convert.FromBase64String(standard.PadRight(standard.Length + missing, Padding));
                    isValid = true;
                }
                catch (FormatException)
                {
                    data = null;
                }
            }

            return isValid;
        }
    }
}
