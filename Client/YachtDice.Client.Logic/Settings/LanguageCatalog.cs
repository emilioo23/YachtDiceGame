using System;
using System.Collections.Generic;

namespace YachtDice.Client.Logic.Settings
{
    /// <summary>
    /// The languages of the interface, with the flag and the short name that each one shows.
    /// </summary>
    public static class LanguageCatalog
    {
        /// <summary>
        /// Culture code of the language that the game uses when the player has not chosen another one.
        /// </summary>
        public const string DefaultCultureCode = "es-MX";

        private const char CultureSeparator = '-';
        private const int NotFoundIndex = -1;
        private const int DefaultIndex = 0;

        private static readonly string[] _cultureCodes = { "es-MX", "en-US", "pt-BR", "zh-CN" };

        private static readonly Dictionary<string, string> _flagCodes = new Dictionary<string, string>
        {
            { "es-MX", "es" },
            { "en-US", "us" },
            { "pt-BR", "br" },
            { "zh-CN", "cn" }
        };

        /// <summary>
        /// Gets the culture codes of the languages, in the order that the lists show them.
        /// </summary>
        public static IReadOnlyList<string> CultureCodes => _cultureCodes;

        /// <summary>
        /// Gets the code of the flag that represents a language.
        /// </summary>
        /// <param name="cultureCode">Culture code, for example "pt-BR".</param>
        /// <returns>The code of the flag, or the one of the Spanish language when the culture is unknown.</returns>
        public static string GetFlagCode(string cultureCode)
        {
            string key = _flagCodes.ContainsKey(cultureCode) ? cultureCode : DefaultCultureCode;

            return _flagCodes[key];
        }

        /// <summary>
        /// Gets the short name of a language, made with the letters of the language of its culture code.
        /// </summary>
        /// <param name="cultureCode">Culture code, for example "pt-BR".</param>
        /// <returns>The short name in uppercase, for example "PT".</returns>
        public static string GetShortName(string cultureCode)
        {
            string language = cultureCode.Split(CultureSeparator)[DefaultIndex];

            return language.ToUpperInvariant();
        }

        /// <summary>
        /// Tells whether the game has the texts of a language.
        /// </summary>
        /// <param name="cultureCode">Culture code, for example "pt-BR".</param>
        /// <returns>True if the language is one of the languages of the game.</returns>
        public static bool IsSupported(string cultureCode)
        {
            return cultureCode != null && Array.IndexOf(_cultureCodes, cultureCode) != NotFoundIndex;
        }

        /// <summary>
        /// Gets the position of a language in the list of languages.
        /// </summary>
        /// <param name="cultureCode">Culture code, for example "pt-BR".</param>
        /// <returns>The position, or the one of the Spanish language when the culture is unknown.</returns>
        public static int IndexOf(string cultureCode)
        {
            int index = Array.IndexOf(_cultureCodes, cultureCode);

            return index == NotFoundIndex ? DefaultIndex : index;
        }
    }
}
