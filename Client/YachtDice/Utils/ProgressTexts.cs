using YachtDice.Resources;

namespace YachtDice.Utils
{
    /// <summary>
    /// Gives the translated names of the achievements and titles from their identifiers.
    /// </summary>
    public static class ProgressTexts
    {
        private const string AchievementNameKey = "Achievement_{0}_Name";
        private const string AchievementDescriptionKey = "Achievement_{0}_Description";
        private const string TitleKey = "Title_{0}";
        private const string FlagKey = "Flag_{0}";

        /// <summary>
        /// Gets the name of an achievement.
        /// </summary>
        /// <param name="achievementId">Identifier of the achievement.</param>
        /// <returns>The name in the current language, or an empty text if it does not exist.</returns>
        public static string GetAchievementName(int achievementId)
        {
            return Read(string.Format(AchievementNameKey, achievementId));
        }

        /// <summary>
        /// Gets what a player must do to unlock an achievement.
        /// </summary>
        /// <param name="achievementId">Identifier of the achievement.</param>
        /// <returns>The description in the current language, or an empty text if it does not exist.</returns>
        public static string GetAchievementDescription(int achievementId)
        {
            return Read(string.Format(AchievementDescriptionKey, achievementId));
        }

        /// <summary>
        /// Gets the name of a title.
        /// </summary>
        /// <param name="titleId">Identifier of the title.</param>
        /// <returns>The title in the current language, or an empty text if it does not exist.</returns>
        public static string GetTitleName(int titleId)
        {
            return Read(string.Format(TitleKey, titleId));
        }

        /// <summary>
        /// Gets the name of a country from the code of its flag.
        /// </summary>
        /// <param name="flagCode">Code of the country with two lowercase letters.</param>
        /// <returns>The name in the current language, or an empty text if it does not exist.</returns>
        public static string GetFlagName(string flagCode)
        {
            return Read(string.Format(FlagKey, flagCode));
        }

        private static string Read(string key)
        {
            return Strings.ResourceManager.GetString(key, Strings.Culture) ?? string.Empty;
        }
    }
}
