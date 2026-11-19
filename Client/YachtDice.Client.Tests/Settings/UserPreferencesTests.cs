using Xunit;
using YachtDice.Client.Logic.Audio;
using YachtDice.Client.Logic.Settings;

namespace YachtDice.Client.Tests.Settings
{
    /// <summary>
    /// The language, theme and audio that the game keeps between sessions, checked before they are used.
    /// </summary>
    public class UserPreferencesTests
    {
        private const string PortugueseCulture = "pt-BR";
        private const string UnknownCulture = "fr-FR";
        private const int TooHighVolume = 150;
        private const int TooLowVolume = -10;
        private const int QuietVolume = 15;

        [Fact]
        public void Normalize_NewPreferences_KeepsTheDefaultValues()
        {
            var defaultPreferencesExpected = new UserPreferences();

            UserPreferences normalized = new UserPreferences().Normalize();

            Assert.Equivalent(defaultPreferencesExpected, normalized);
        }

        [Fact]
        public void Normalize_SupportedCulture_KeepsTheCulture()
        {
            var preferences = new UserPreferences { CultureCode = PortugueseCulture };

            UserPreferences normalized = preferences.Normalize();

            Assert.Equal(PortugueseCulture, normalized.CultureCode);
        }

        [Fact]
        public void Normalize_UnknownCulture_UsesTheDefaultCulture()
        {
            var preferences = new UserPreferences { CultureCode = UnknownCulture };

            UserPreferences normalized = preferences.Normalize();

            Assert.Equal(LanguageCatalog.DefaultCultureCode, normalized.CultureCode);
        }

        [Fact]
        public void Normalize_NullCulture_UsesTheDefaultCulture()
        {
            var preferences = new UserPreferences { CultureCode = null };

            UserPreferences normalized = preferences.Normalize();

            Assert.Equal(LanguageCatalog.DefaultCultureCode, normalized.CultureCode);
        }

        [Fact]
        public void Normalize_VolumeAboveTheMaximum_UsesTheMaximum()
        {
            var preferences = new UserPreferences { MusicVolume = TooHighVolume };

            UserPreferences normalized = preferences.Normalize();

            Assert.Equal(AudioSettings.MaxVolume, normalized.MusicVolume);
        }

        [Fact]
        public void Normalize_VolumeBelowTheMinimum_UsesTheMinimum()
        {
            var preferences = new UserPreferences { MusicVolume = TooLowVolume };

            UserPreferences normalized = preferences.Normalize();

            Assert.Equal(AudioSettings.MinVolume, normalized.MusicVolume);
        }

        [Fact]
        public void Normalize_PotentialPointsTurnedOff_KeepsTheChoice()
        {
            var preferences = new UserPreferences { ShowPotentialPoints = false };

            UserPreferences normalized = preferences.Normalize();

            bool showPotentialPointsResult = normalized.ShowPotentialPoints;

            Assert.False(showPotentialPointsResult);
        }

        [Fact]
        public void Normalize_ChosenThemeAndAudio_KeepsTheChoices()
        {
            var preferences = new UserPreferences
            {
                IsDarkTheme = true,
                AreEffectsEnabled = false,
                IsMusicMuted = true,
                MusicVolume = QuietVolume
            };

            UserPreferences normalized = preferences.Normalize();

            Assert.Equivalent(preferences, normalized);
        }
    }
}
