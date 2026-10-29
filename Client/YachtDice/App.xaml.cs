using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using YachtDice.Gateway;
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
        private const string DefaultCultureCode = "es-MX";

        /// <summary>
        /// Gets a value indicating whether the dark theme is currently active in the application.
        /// </summary>
        public static bool IsDarkTheme { get; private set; }

        /// <summary>
        /// Gets the current active culture code, used for internationalization.
        /// </summary>
        public static string CurrentCultureCode { get; private set; }

        /// <summary>
        /// Executes when the application starts: sets the language, the theme, and opens the initial window.
        /// </summary>
        /// <param name="e">Startup event arguments.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SetCulture(DefaultCultureCode);
            bool defaultToDarkTheme = false;
            SetTheme(isDark: defaultToDarkTheme);

            var splashWindow = new SplashWindow();
            splashWindow.Show();
        }

        /// <summary>
        /// Executes when the application closes: tells the server that the player is no longer online.
        /// </summary>
        /// <param name="e">Exit event arguments.</param>
        protected override void OnExit(ExitEventArgs e)
        {
            PresenceGateway.Current.Disconnect();
            base.OnExit(e);
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
    }
}