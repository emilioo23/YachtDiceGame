using System;

namespace YachtDice.Server.Logic.Passkeys
{
    /// <summary>
    /// Converts bytes to and from base64url, the text format that WebAuthn uses for binary data.
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
        /// <exception cref="ArgumentNullException">The bytes are null.</exception>
        public static string Encode(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "The bytes to encode cannot be null.");
            }

            string text = Convert.ToBase64String(data)
                .TrimEnd(Padding)
                .Replace(StandardPlus, UrlMinus)
                .Replace(StandardSlash, UrlUnderscore);

            return text;
        }

        /// <summary>
        /// Reads a base64url text, with or without padding.
        /// </summary>
        /// <param name="text">Text to read.</param>
        /// <param name="data">Bytes of the text; null if the text is not valid.</param>
        /// <returns>True if the text is valid base64url.</returns>
        public static bool TryDecode(string text, out byte[] data)
        {
            bool isValid = false;

            data = null;

            if (!string.IsNullOrEmpty(text) && text.Length <= PasskeyRules.MaxEncodedLength)
            {
                string standard = text.Replace(UrlMinus, StandardPlus).Replace(UrlUnderscore, StandardSlash);
                int missing = (GroupSize - standard.Length % GroupSize) % GroupSize;
                string padded = standard.PadRight(standard.Length + missing, Padding);

                isValid = TryConvert(padded, out data);
            }

            return isValid;
        }

        private static bool TryConvert(string padded, out byte[] data)
        {
            bool isValid;

            try
            {
                data = Convert.FromBase64String(padded);
                isValid = true;
            }
            catch (FormatException)
            {
                data = null;
                isValid = false;
            }

            return isValid;
        }
    }
}
