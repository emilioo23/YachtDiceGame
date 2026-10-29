using System.Windows;
using System.Windows.Input;

namespace YachtDice.Views
{
    /// <summary>
    /// Welcome window shown when the application starts, before the login.
    /// </summary>
    public partial class SplashWindow : Window
    {
        private bool _hasContinued;

        /// <summary>
        /// Initializes the welcome window.
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
            if (!_hasContinued)
            {
                _hasContinued = true;

                var loginWindow = new LoginWindow();
                loginWindow.Show();
                Close();
            }
        }
    }
}