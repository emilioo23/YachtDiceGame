using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using YachtDice.Client.Logic.Audio;
using YachtDice.Client.Logic.Settings;
using YachtDice.Utils;
using YachtDice.Views;

namespace YachtDice
{
    /// <summary>
    /// Main class of the WPF application; manages its life cycle and global state.
    /// </summary>
    public partial class App : Application
    {
        private const string LightThemeSource = "Styles/Theme.xaml";
        private const string DarkThemeSource = "Styles/Theme.Dark.xaml";
        private readonly SoundEffectPlayer _clickSounds = new SoundEffectPlayer();
        private readonly PreferencesStore _preferencesStore = new PreferencesStore();

        /// <summary>
        /// Gets a value indicating whether the dark theme is currently active in the application.
        /// </summary>
        public static bool IsDarkTheme { get; private set; }

        /// <summary>
        /// Gets what the player chose about the sound effects and the music.
        /// </summary>
        public static AudioSettings Audio { get; } = new AudioSettings();

        /// <summary>
        /// Gets the player of the background music, which keeps playing while the windows change.
        /// </summary>
        public static MusicPlayer Music { get; private set; }

        /// <summary>
        /// Gets the current active culture code, used for internationalization.
        /// </summary>
        public static string CurrentCultureCode { get; private set; }

        /// <summary>
        /// Executes when the application starts: applies the language, the theme and the audio that the player
        /// chose in the last session, and opens the initial window.
        /// </summary>
        /// <param name="e">Startup event arguments.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ApplyPreferences(_preferencesStore.Load());

            Music = new MusicPlayer(Audio);
            EventManager.RegisterClassHandler(
                typeof(ButtonBase),
                ButtonBase.ClickEvent,
                new RoutedEventHandler(HandleButtonClick));

            var splashWindow = new SplashWindow();
            splashWindow.Show();
        }

        /// <summary>
        /// Changes the active culture of the application (the language of the texts).
        /// </summary>
        /// <param name="cultureCode">Culture code, e.g., "es-MX" or "en-US".</param>
        public static void SetCulture(string cultureCode)
        {
            var culture = new CultureInfo(cultureCode);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CurrentCultureCode = cultureCode;
        }

        /// <summary>
        /// Changes the active visual theme of the application (light or dark).
        /// </summary>
        /// <param name="isDark">True to apply the dark theme; false for the light theme.</param>
        public static void SetTheme(bool isDark)
        {
            IsDarkTheme = isDark;
            string themeSource = isDark ? DarkThemeSource : LightThemeSource;

            var themeDictionary = new ResourceDictionary { Source = new Uri(themeSource, UriKind.Relative) };
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(themeDictionary);
        }

        /// <summary>
        /// Executes when the application closes: saves the language, the theme and the audio of the player.
        /// </summary>
        /// <param name="e">Exit event arguments.</param>
        protected override void OnExit(ExitEventArgs e)
        {
            _preferencesStore.Save(CreateCurrentPreferences());
            base.OnExit(e);
        }

        private static void ApplyPreferences(UserPreferences preferences)
        {
            SetCulture(preferences.CultureCode);
            SetTheme(preferences.IsDarkTheme);
            Audio.SetEffectsEnabled(preferences.AreEffectsEnabled);
            Audio.SetMusicMuted(preferences.IsMusicMuted);
            Audio.SetMusicVolume(preferences.MusicVolume);
        }

        private static UserPreferences CreateCurrentPreferences()
        {
            return new UserPreferences
            {
                CultureCode = CurrentCultureCode,
                IsDarkTheme = IsDarkTheme,
                AreEffectsEnabled = Audio.AreEffectsEnabled,
                IsMusicMuted = Audio.IsMusicMuted,
                MusicVolume = Audio.MusicVolume
            };
        }

        private void HandleButtonClick(object sender, RoutedEventArgs e)
        {
            _clickSounds.Play(SoundEffect.Click);
        }
    }
}