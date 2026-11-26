using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using YachtDice.Client.Logic.Audio;
using YachtDice.Client.Logic.Logging;
using YachtDice.Client.Logic.Settings;
using YachtDice.Composition;
using YachtDice.Resources;
using YachtDice.Utils;
using YachtDice.Views;

namespace YachtDice
{
    /// <summary>
    /// Main class of the WPF application; builds the dependency injection container and manages the life cycle.
    /// </summary>
    public partial class App : Application
    {
        private const string LightThemeSource = "Styles/Theme.xaml";
        private const string DarkThemeSource = "Styles/Theme.Dark.xaml";
        private const double PressedScale = 0.94;
        private const int PressMilliseconds = 80;

        private static readonly TimeSpan PressDuration = TimeSpan.FromMilliseconds(PressMilliseconds);

        private readonly ElementAnimator _animator = new ElementAnimator();
        private readonly PreferencesStore _preferencesStore = new PreferencesStore();
        private readonly AppLogger _logger = new AppLogger(typeof(App));
        private ServiceProvider _provider;
        private ClientServices _services;
        private SoundEffectPlayer _clickSounds;

        /// <summary>
        /// Gets a value indicating whether the dark theme is currently active in the application.
        /// </summary>
        public static bool IsDarkTheme => Current.Resources.MergedDictionaries.Any(IsDarkThemeDictionary);

        /// <summary>
        /// Gets the current active culture code, used for internationalization.
        /// </summary>
        public static string CurrentCultureCode => CultureInfo.CurrentUICulture.Name;

        /// <summary>
        /// Executes when the application starts: builds the container, applies the language, the theme and the
        /// audio that the player chose in the last session, and opens the initial window.
        /// </summary>
        /// <param name="e">Startup event arguments.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _logger.Info(LogMessages.StartedLogMessage);
            DispatcherUnhandledException += HandleDispatcherUnhandledException;
            TextLimitNotice.Register();
            TaskScheduler.UnobservedTaskException += HandleUnobservedTaskException;
            _provider = new ServiceCollection()
                .AddSingleton<AudioSettings>()
                .AddSingleton<MusicPlayer>()
                .AddSingleton<ClientServices>()
                .BuildServiceProvider();
            _services = _provider.GetRequiredService<ClientServices>();
            _clickSounds = new SoundEffectPlayer(_services.Audio);
            ApplyPreferences(_preferencesStore.Load());
            EventManager.RegisterClassHandler(
                typeof(ButtonBase),
                ButtonBase.ClickEvent,
                new RoutedEventHandler(HandleButtonClick));

            var splashWindow = new SplashWindow(_services);
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
        }

        /// <summary>
        /// Changes the active visual theme of the application (light or dark).
        /// </summary>
        /// <param name="isDark">True to apply the dark theme; false for the light theme.</param>
        public static void SetTheme(bool isDark)
        {
            string themeSource = isDark ? DarkThemeSource : LightThemeSource;

            var themeDictionary = new ResourceDictionary { Source = new Uri(themeSource, UriKind.Relative) };
            Current.Resources.MergedDictionaries.Clear();
            Current.Resources.MergedDictionaries.Add(themeDictionary);
        }

        /// <summary>
        /// Executes when the application closes: saves the language, the theme and the audio of the player.
        /// </summary>
        /// <param name="e">Exit event arguments.</param>
        protected override void OnExit(ExitEventArgs e)
        {
            _preferencesStore.Save(CreateCurrentPreferences());
            _provider.Dispose();
            _logger.Info(LogMessages.ClosedLogMessage);
            base.OnExit(e);
        }

        private void HandleDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.Error(e.Exception, LogMessages.UnhandledErrorLogMessage);
            e.Handled = true;
            CustomDialogWindow.ShowMessage(FindActiveWindow(), Strings.Dialogs_UnexpectedError);
        }

        private void HandleUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            _logger.Error(e.Exception, LogMessages.UnobservedTaskLogMessage);
            e.SetObserved();
        }

        private Window FindActiveWindow()
        {
            Func<Window, bool> isActive = window => window.IsActive;

            return Windows.OfType<Window>().FirstOrDefault(isActive) ?? MainWindow;
        }

        private static bool IsDarkThemeDictionary(ResourceDictionary dictionary)
        {
            return dictionary.Source != null && dictionary.Source.OriginalString == DarkThemeSource;
        }

        private void ApplyPreferences(UserPreferences preferences)
        {
            SetCulture(preferences.CultureCode);
            SetTheme(preferences.IsDarkTheme);
            _services.Audio.SetEffectsEnabled(preferences.AreEffectsEnabled);
            _services.Audio.SetMusicMuted(preferences.IsMusicMuted);
            _services.Audio.SetMusicVolume(preferences.MusicVolume);
            _services.Gameplay.SetShowPotentialPoints(preferences.ShowsPotentialPoints);
        }

        private UserPreferences CreateCurrentPreferences()
        {
            return new UserPreferences
            {
                CultureCode = CurrentCultureCode,
                IsDarkTheme = IsDarkTheme,
                AreEffectsEnabled = _services.Audio.AreEffectsEnabled,
                IsMusicMuted = _services.Audio.IsMusicMuted,
                MusicVolume = _services.Audio.MusicVolume,
                ShowsPotentialPoints = _services.Gameplay.ShowsPotentialPoints
            };
        }

        private void HandleButtonClick(object sender, RoutedEventArgs e)
        {
            _clickSounds.Play(SoundEffect.Click);
            _animator.Pulse((UIElement)sender, PressedScale, PressDuration);
        }
    }
}
