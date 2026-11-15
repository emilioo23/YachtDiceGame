using System;
using YachtDice.Client.Logic.Audio;

namespace YachtDice.Client.Logic.Settings
{
    /// <summary>
    /// Choices of the player that the game keeps between one session and the next: the language, the theme
    /// and the audio. The values come from a file that the player could edit, so they are checked before they
    /// are used.
    /// </summary>
    public sealed class UserPreferences
    {
        /// <summary>
        /// Gets or sets the culture code of the language of the interface.
        /// </summary>
        public string CultureCode { get; set; } = LanguageCatalog.DefaultCultureCode;

        /// <summary>
        /// Gets or sets a value indicating whether the dark theme is used.
        /// </summary>
        public bool IsDarkTheme { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the sound effects are turned on.
        /// </summary>
        public bool AreEffectsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the background music is muted.
        /// </summary>
        public bool IsMusicMuted { get; set; }

        /// <summary>
        /// Gets or sets the volume of the background music, from 0 to 100.
        /// </summary>
        public int MusicVolume { get; set; } = AudioSettings.DefaultMusicVolume;

        /// <summary>
        /// Creates a copy that the game can use as it is: a language that the game does not have is changed
        /// for the default one and the volume is kept between its limits.
        /// </summary>
        /// <returns>The checked copy of the preferences.</returns>
        public UserPreferences Normalize()
        {
            string cultureCode = LanguageCatalog.IsSupported(CultureCode)
                ? CultureCode
                : LanguageCatalog.DefaultCultureCode;

            return new UserPreferences
            {
                CultureCode = cultureCode,
                IsDarkTheme = IsDarkTheme,
                AreEffectsEnabled = AreEffectsEnabled,
                IsMusicMuted = IsMusicMuted,
                MusicVolume = Math.Max(AudioSettings.MinVolume, Math.Min(AudioSettings.MaxVolume, MusicVolume))
            };
        }
    }
}
