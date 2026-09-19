using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using YachtDice.Views;

namespace YachtDice
{
    /// <summary>
    /// Clase principal que representa la aplicación WPF y gestiona su ciclo de vida y estado global.
    /// </summary>
    public partial class App : Application
    {
        private const string LightThemeSource = "Styles/Theme.xaml";
        private const string DarkThemeSource = "Styles/Theme.Dark.xaml";
        private const string DefaultCultureCode = "es-MX";

        /// <summary>
        /// Obtiene un valor que indica si el tema oscuro está activo actualmente en la aplicación.
        /// </summary>
        public static bool IsDarkTheme { get; private set; }
        /// <summary>
        /// Obtiene el código de la cultura activa actual para la internacionalización.
        /// </summary>
        public static string CurrentCultureCode { get; private set; }

        /// <summary>
        /// Se ejecuta al iniciar la aplicación, configurando el idioma, el tema y abriendo la ventana principal.
        /// </summary>
        /// <param name="e">Argumentos del evento de inicio.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

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