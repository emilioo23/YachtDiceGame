using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using YachtDice.Server.Logic.Game;
using YachtDice.Server.Logic.Players;
using YachtDice.Server.Logic.Rooms;

namespace YachtDice.Server.Logic.Configuration
{
    /// <summary>
    /// Reads the rules of the game from the XML document of the configuration. The server reads it once, when it
    /// starts, so a wrong value stops the server instead of breaking a game later.
    /// </summary>
    public static class GameRulesReader
    {
        private const string GameModesElement = "gameModes";
        private const string ModeElement = "mode";
        private const string SpeedAttribute = "speed";
        private const string TurnSecondsAttribute = "turnSeconds";
        private const string LevelsElement = "levels";
        private const string LevelElement = "level";
        private const string ExperienceAttribute = "experience";
        private const string AchievementsElement = "achievements";
        private const string AchievementElement = "achievement";
        private const string IdAttribute = "id";
        private const string MetricAttribute = "metric";
        private const string RequiredValueAttribute = "requiredValue";
        private const string MinimumGamesAttribute = "minimumGames";
        private const string HasTitleAttribute = "hasTitle";
        private const string NullDocumentMessage = "The document of the rules of the game cannot be null.";
        private const string MissingSectionFormat = "The rules of the game have no {0} section.";
        private const string InvalidNumberFormat =
            "The attribute {0} of the element {1} is missing or is not a whole number of zero or more.";
        private const string InvalidValueFormat = "The attribute {0} of the element {1} has a value that is not valid.";

        /// <summary>
        /// Reads the game modes, the levels and the achievements of the document.
        /// </summary>
        /// <param name="document">XML document with the rules of the game.</param>
        /// <returns>The rules of the game.</returns>
        /// <exception cref="ArgumentNullException">The document is null.</exception>
        /// <exception cref="InvalidOperationException">A section is missing or a value is not valid.</exception>
        /// <exception cref="ArgumentException">The levels, the modes or the achievements break their rules.</exception>
        public static GameRules Read(XDocument document)
        {
            XElement root = document?.Root ?? throw new ArgumentNullException(nameof(document), NullDocumentMessage);

            return new GameRules(
                ReadTurnDuration(GetSection(root, GameModesElement)),
                ReadLevelTable(GetSection(root, LevelsElement)),
                ReadAchievements(GetSection(root, AchievementsElement)));
        }

        private static XElement GetSection(XElement root, string sectionName)
        {
            XElement section = root.Element(sectionName);

            if (section == null)
            {
                string message = string.Format(CultureInfo.InvariantCulture, MissingSectionFormat, sectionName);

                throw new InvalidOperationException(message);
            }

            return section;
        }

        private static TurnDuration ReadTurnDuration(XElement gameModes)
        {
            var secondsBySpeed = new Dictionary<GameSpeed, int>();

            foreach (XElement mode in gameModes.Elements(ModeElement))
            {
                secondsBySpeed[ReadEnum<GameSpeed>(mode, SpeedAttribute)] = ReadNumber(mode, TurnSecondsAttribute);
            }

            return new TurnDuration(secondsBySpeed);
        }

        private static LevelTable ReadLevelTable(XElement levels)
        {
            List<int> thresholds = levels.Elements(LevelElement)
                .Select(level => ReadNumber(level, ExperienceAttribute))
                .ToList();

            return new LevelTable(thresholds);
        }

        private static AchievementCatalog ReadAchievements(XElement achievements)
        {
            List<AchievementRule> rules = achievements.Elements(AchievementElement)
                .Select(ReadAchievement)
                .ToList();

            return new AchievementCatalog(rules);
        }

        private static AchievementRule ReadAchievement(XElement achievement)
        {
            var rule = new AchievementRule(
                ReadNumber(achievement, IdAttribute),
                ReadEnum<AchievementMetric>(achievement, MetricAttribute),
                ReadNumber(achievement, RequiredValueAttribute));

            bool hasMinimumGames = achievement.Attribute(MinimumGamesAttribute) != null;

            if (hasMinimumGames)
            {
                rule.MinimumGames = ReadNumber(achievement, MinimumGamesAttribute);
            }

            rule.HasTitle = ReadHasTitle(achievement);

            return rule;
        }

        private static bool ReadHasTitle(XElement achievement)
        {
            string text = (string)achievement.Attribute(HasTitleAttribute);
            bool hasTitle = true;
            bool isValid = text == null || bool.TryParse(text, out hasTitle);

            if (!isValid)
            {
                throw new InvalidOperationException(FormatProblem(InvalidValueFormat, achievement, HasTitleAttribute));
            }

            return hasTitle;
        }

        private static int ReadNumber(XElement element, string attributeName)
        {
            string text = (string)element.Attribute(attributeName);
            bool isNumber = int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out int value);

            if (!isNumber)
            {
                throw new InvalidOperationException(FormatProblem(InvalidNumberFormat, element, attributeName));
            }

            return value;
        }

        private static TEnum ReadEnum<TEnum>(XElement element, string attributeName)
            where TEnum : struct
        {
            string text = (string)element.Attribute(attributeName);
            bool isName = Enum.TryParse(text, false, out TEnum value);
            bool isDefinedName = isName && Enum.IsDefined(typeof(TEnum), text);

            if (!isDefinedName)
            {
                throw new InvalidOperationException(FormatProblem(InvalidValueFormat, element, attributeName));
            }

            return value;
        }

        private static string FormatProblem(string format, XElement element, string attributeName)
        {
            return string.Format(CultureInfo.InvariantCulture, format, attributeName, element.Name.LocalName);
        }
    }
}
