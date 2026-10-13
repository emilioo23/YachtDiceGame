using System.Windows;
using System.Windows.Controls;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Window to view and edit the profile information and the statistics of the player.
    /// </summary>
    public partial class ProfileWindow : Window
    {
        private readonly string _playerName;

        /// <summary>
        /// Initializes the profile window for the given player.
        /// </summary>
        /// <param name="playerName">Name of the player whose profile is being edited.</param>
        public ProfileWindow(string playerName)
        {
            InitializeComponent();
            _playerName = playerName;
            UsernameTextBox.Text = playerName;
            AvatarInitialTextBlock.Text = playerName.Length > 0 ? playerName.Substring(0, 1).ToUpper() : "J";
            LanguageSwitcherControl.ReopenWindowFunc = () => new ProfileWindow(playerName);
        }

        private void ColorSwatch_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var swatch = sender as Border;
            if (swatch == null)
            {
                return;
            }

            var avatarBorder = (Border)AvatarInitialTextBlock.Parent;
            avatarBorder.Background = swatch.Background;
        }

        private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            // The real changes will be saved in the backend here later.
            new CustomDialogWindow(new DialogContentDto { Message = Strings.Profile_SavedMessage }).ShowDialog();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var menuWindow = new MenuWindow(_playerName);
            menuWindow.Show();
            this.Close();
        }
    }
}