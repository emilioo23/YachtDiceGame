using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using YachtDice.Server.Logic.Configuration;
using YachtDice.Server.Logic.Moderation;

namespace YachtDice.Server.Host.Composition
{
    /// <summary>
    /// Reads from the configuration of the server the limits, the rules of the game and the offensive words that the
    /// components need.
    /// </summary>
    internal static class ServerConfiguration
    {
        private const string InvalidNumberFormat = "The setting {0} is missing or is not a positive whole number.";
        private const int SmallestAllowedNumber = 1;
        private const string OffensiveWordsPath = "Resources\\OffensiveWords.txt";
        private const string GameRulesPath = "Resources\\GameRules.xml";

        /// <summary>
        /// Reads a whole number greater than zero, such as a limit or a time of a rule.
        /// </summary>
        /// <param name="key">Key of the setting.</param>
        /// <returns>The configured number.</returns>
        /// <exception cref="InvalidOperationException">The setting is missing or is not a positive whole
        /// number.</exception>
        public static int ReadPositiveNumber(string key)
        {
            string configuredValue = ConfigurationManager.AppSettings[key];
            bool isNumber = int.TryParse(
                configuredValue,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int value);

            if (!isNumber || value < SmallestAllowedNumber)
            {
                string message = string.Format(CultureInfo.InvariantCulture, InvalidNumberFormat, key);

                throw new InvalidOperationException(message);
            }

            return value;
        }

        /// <summary>
        /// Reads the game modes, the levels and the achievements from the file of the rules of the game.
        /// </summary>
        /// <returns>The rules of the game.</returns>
        /// <exception cref="IOException">The file could not be read.</exception>
        /// <exception cref="XmlException">The file is not a valid XML document.</exception>
        /// <exception cref="InvalidOperationException">A rule of the file is missing or is not valid.</exception>
        public static GameRules LoadGameRules()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, GameRulesPath);

            GameRules gameRules;

            using (XmlReader reader = XmlReader.Create(path))
            {
                XDocument document = XDocument.Load(reader);

                gameRules = GameRulesReader.Read(document);
            }

            return gameRules;
        }

        /// <summary>
        /// Reads the offensive words from the file of the server.
        /// </summary>
        /// <returns>The filter with the words of the file.</returns>
        /// <exception cref="IOException">The file could not be read.</exception>
        public static OffensiveWordFilter CreateOffensiveWordFilter()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, OffensiveWordsPath);

            return new OffensiveWordFilter(File.ReadAllLines(path));
        }
    }
}
