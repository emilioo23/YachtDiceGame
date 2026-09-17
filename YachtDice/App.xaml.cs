using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using YachtDice.Views;

namespace YachtDice
{
    public partial class App : Application
    {
        private const string LightThemeSource = "Styles/Theme.xaml";
        private const string DarkThemeSource = "Styles/Theme.Dark.xaml";
        private const string DefaultCultureCode = "es-MX";

        public static bool IsDarkTheme { get; private set; }
        public static string CurrentCultureCode { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Cultura y tema por defecto al arrancar: espanol (Mexico) y tema claro.
            SetCulture(DefaultCultureCode);
            SetTheme(isDark: false);

            var loginWindow = new LoginWindow();
            loginWindow.Show();
        }

        /// <summary>
        /// Cambia la cultura activa de la aplicacion (idioma de los textos).
        /// </summary>
        /// <param name="cultureCode">Codigo de cultura, por ejemplo "es-MX" o "en-US".</param>
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
        /// Cambia el tema visual activo de la aplicacion (claro u oscuro).
        /// </summary>
        /// <param name="isDark">Verdadero para aplicar el tema oscuro; falso para el tema claro.</param>
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