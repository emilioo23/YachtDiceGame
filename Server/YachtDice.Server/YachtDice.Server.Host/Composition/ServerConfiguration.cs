using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using YachtDice.Server.Logic.Game;
using YachtDice.Server.Logic.Moderation;
using YachtDice.Server.Logic.Players;
using YachtDice.Server.Logic.Rooms;

namespace YachtDice.Server.Host.Composition
{
    /// <summary>
    /// Reads from the configuration of the server the rules and limits that the components need.
    /// </summary>
    internal static class ServerConfiguration
    {
        private const string TurnSecondsKeyFormat = "TurnSeconds.{0}";
        private const string InvalidNumberFormat = "The setting {0} is missing or is not a positive whole number.";
        private const int SmallestAllowedNumber = 1;
        private const string LevelExperienceKey = "LevelExperience";
        private const string OffensiveWordsPath = "Resources\\OffensiveWords.txt";
        private const char LevelExperienceSeparator = ',';
        private const string InvalidLevelExperienceMessage =
            "The setting LevelExperience is missing or is not a list of numbers separated by commas.";

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
        /// Reads the seconds that a turn lasts for each speed of the game.
        /// </summary>
        /// <returns>The durations of the turns.</returns>
        /// <exception cref="InvalidOperationException">A speed has no valid seconds in the configuration.</exception>
        public static TurnDuration CreateTurnDuration()
        {
            var secondsBySpeed = new Dictionary<GameSpeed, int>();

            foreach (GameSpeed speed in Enum.GetValues(typeof(GameSpeed)))
            {
                string key = string.Format(CultureInfo.InvariantCulture, TurnSecondsKeyFormat, speed);

                secondsBySpeed[speed] = ReadPositiveNumber(key);
            }

            return new TurnDuration(secondsBySpeed);
        }

        /// <summary>
        /// Reads the experience that each level needs, from the first level to the last.
        /// </summary>
        /// <returns>The table of levels.</returns>
        /// <exception cref="InvalidOperationException">The setting is missing or has a value that is not a
        /// number.</exception>
        public static LevelTable CreateLevelTable()
        {
            string configuredExperience = ConfigurationManager.AppSettings[LevelExperienceKey] ?? string.Empty;
            var thresholds = new List<int>();

            foreach (string value in configuredExperience.Split(LevelExperienceSeparator))
            {
                if (!int.TryParse(value.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out int experience))
                {
                    throw new InvalidOperationException(InvalidLevelExperienceMessage);
                }

                thresholds.Add(experience);
            }

            return new LevelTable(thresholds);
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
