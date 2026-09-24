using System.Windows;
using System.Windows.Input;

namespace YachtDice.Views
{
    /// <summary>
    /// Ventana de bienvenida que se muestra al iniciar la aplicacion, antes del inicio de sesion.
    /// </summary>
    public partial class SplashWindow : Window
    {
        private bool _hasContinued;

        /// <summary>
        /// Inicializa la ventana de bienvenida.
        /// </summary>
        public SplashWindow()
        {
            InitializeComponent();
        }

        private void SplashWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Focusable = true;
            Focus();
        }

        private void SplashWindow_ContinueRequested(object sender, RoutedEventArgs e)
        {
            if (_hasContinued)
            {
                return;
            }

            _hasContinued = true;

            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
    }
}