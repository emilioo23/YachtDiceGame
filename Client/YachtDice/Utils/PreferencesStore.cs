using System.Configuration;
using YachtDice.Client.Logic.Settings;
using YachtDice.Properties;

namespace YachtDice.Utils
{
    /// <summary>
    /// Reads and saves the preferences of the player in the user settings of Windows, so the language, the
    /// theme, the audio and the help while playing stay the same the next time the game opens. If the settings file is damaged, the
    /// error is only written to the log and the game uses the default preferences: the player does not need
    /// to know, because the game works the same and the next save writes a good file.
    /// </summary>
    public sealed class PreferencesStore
    {
        private const string LoadFailedLogMessage = "The saved preferences could not be read, the defaults are used.";
        private const string SaveFailedLogMessage = "The preferences of the player could not be saved.";

        private readonly AppLogger _logger = new AppLogger(typeof(PreferencesStore));

        /// <summary>
        /// Reads the preferences saved in the last session.
        /// </summary>
        /// <returns>The saved preferences, checked, or the default ones when they cannot be read.</returns>
        public UserPreferences Load()
        {
            var preferences = new UserPreferences();

            try
            {
                Settings saved = Settings.Default;

                preferences = new UserPreferences
                {
                    CultureCode = saved.CultureCode,
                    IsDarkTheme = saved.IsDarkTheme,
                    AreEffectsEnabled = saved.AreEffectsEnabled,
                    IsMusicMuted = saved.IsMusicMuted,
                    MusicVolume = saved.MusicVolume,
                    ShowsPotentialPoints = saved.ShowsPotentialPoints
                };
            }
            catch (ConfigurationErrorsException ex)
            {
                _logger.Error(ex, LoadFailedLogMessage);
            }

            return preferences.Normalize();
        }

        /// <summary>
        /// Saves the preferences so the next session starts with them.
        /// </summary>
        /// <param name="preferences">Current preferences of the player.</param>
        public void Save(UserPreferences preferences)
        {
            try
            {
                Settings saved = Settings.Default;

                saved.CultureCode = preferences.CultureCode;
                saved.IsDarkTheme = preferences.IsDarkTheme;
                saved.AreEffectsEnabled = preferences.AreEffectsEnabled;
                saved.IsMusicMuted = preferences.IsMusicMuted;
                saved.MusicVolume = preferences.MusicVolume;
                saved.ShowsPotentialPoints = preferences.ShowsPotentialPoints;
                saved.Save();
            }
            catch (ConfigurationErrorsException ex)
            {
                _logger.Error(ex, SaveFailedLogMessage);
            }
        }
    }
}
