using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Waiting room window where the players get ready and connect before a game starts.
    /// </summary>
    public partial class LobbyWindow : Window
    {
        private const int EmptyLength = 0;
        private const int FirstCharacterIndex = 0;
        private const int SingleCharacterLength = 1;
        private const string DefaultPlayerInitial = "U";
        private const double SelectedBorderThickness = 2.0;
        private const double UnselectedBorderThickness = 1.5;

        private readonly string _playerName;

        /// <summary>
        /// Initializes the waiting room window for the given player.
        /// </summary>
        /// <param name="playerName">Name of the player who enters the room.</param>
        public LobbyWindow(string playerName)
        {
            InitializeComponent();
            _playerName = playerName;
            PlayerNameTextBlock.Text = playerName;
            PlayerInitialTextBlock.Text = playerName.Length > EmptyLength
                ? playerName.Substring(FirstCharacterIndex, SingleCharacterLength).ToUpper()
                : DefaultPlayerInitial;
            LanguageSwitcherControl.ReopenWindowFunc = () => new LobbyWindow(playerName);
        }

        private void SelectModeBorder(Border selected, Border unselected)
        {
            selected.Background = (SolidColorBrush)FindResource(ResourceKeys.Cream2BrushKey);
            selected.BorderBrush = (SolidColorBrush)FindResource(ResourceKeys.FeltLightBrushKey);
            selected.BorderThickness = new Thickness(SelectedBorderThickness);

            unselected.Background = Brushes.Transparent;
            unselected.BorderBrush = (SolidColorBrush)FindResource(ResourceKeys.BorderBrushKey);
            unselected.BorderThickness = new Thickness(UnselectedBorderThickness);
        }

        private void NormalMode_Click(object sender, MouseButtonEventArgs e)
        {
            SelectModeBorder(NormalModeBorder, FastModeBorder);
        }

        private void FastMode_Click(object sender, MouseButtonEventArgs e)
        {
            SelectModeBorder(FastModeBorder, NormalModeBorder);
        }

        private void BackToPlayLink_Click(object sender, MouseButtonEventArgs e)
        {
            var menuWindow = new MenuWindow(_playerName);
            menuWindow.Show();
            this.Close();
        }

        private void CopyCodeButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(RoomCodeTextBlock.Text);
            string message = string.Format(Strings.Lobby_NotificationCodeCopied, RoomCodeTextBlock.Text);
            new CustomDialogWindow(new DialogContent { Message = message }).ShowDialog();
        }

        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            var gameWindow = new GameWindow(_playerName);
            gameWindow.Show();
            this.Close();
        }
    }
}