using System.Windows;
using System.Windows.Controls;
using YachtDice.Models;
using YachtDice.Navigation;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Window to view and edit the profile information and the statistics of the player.
    /// </summary>
    public partial class ProfileWindow : Window
    {
        private readonly PlayerSession _session;

        /// <summary>
        /// Initializes the profile window for the given player.
        /// </summary>
        /// <param name="session">Session of the player whose profile is being edited.</param>
        public ProfileWindow(PlayerSession session)
        {
            InitializeComponent();
            _session = session;
            UsernameTextBox.Text = session.DisplayName;
            AvatarInitialTextBlock.Text = PlayerInitials.GetInitial(session.DisplayName);
            LanguageSwitcherControl.ReopenWindowFunc = () => new ProfileWindow(session);
        }

        private void ColorSwatch_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var swatch = sender as Border;

            if (swatch != null)
            {
                var avatarBorder = (Border)AvatarInitialTextBlock.Parent;
                avatarBorder.Background = swatch.Background;
            }
        }

        private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: The real changes will be saved in the backend here later.
            CustomDialogWindow.ShowMessage(this, Strings.Profile_SavedMessage);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            WindowNavigator.SwitchTo(this, new MenuWindow(_session));
        }
    }
}