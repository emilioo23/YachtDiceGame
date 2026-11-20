using System;
using System.Windows;
using YachtDice.Composition;
using YachtDice.Navigation;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Welcome window shown when the application starts, before the login.
    /// </summary>
    public partial class SplashWindow : Window
    {
        private const string NullServicesMessage = "The shared components of the client cannot be null.";

        private readonly ClientServices _services;
        private bool _hasContinued;

        /// <summary>
        /// Initializes the welcome window.
        /// </summary>
        /// <param name="services">Components that the windows share while the application is open.</param>
        public SplashWindow(ClientServices services)
        {
            InitializeComponent();
            _services = services ?? throw new ArgumentNullException(nameof(services), NullServicesMessage);
            _services.Music.Play(MusicTrack.MenuTheme);
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

                WindowNavigator.SwitchTo(this, new LoginWindow(_services));
            }
        }
    }
}