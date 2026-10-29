using System.Windows;
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

        private void NormalMode_Click(object sender, MouseButtonEventArgs e)
        {
            NormalModeBorder.Background = (SolidColorBrush)FindResource("BrushCream2");
            NormalModeBorder.BorderBrush = (SolidColorBrush)FindResource("BrushFeltLight");
            NormalModeBorder.BorderThickness = new Thickness(SelectedBorderThickness);

            FastModeBorder.Background = Brushes.Transparent;
            FastModeBorder.BorderBrush = (SolidColorBrush)FindResource("BrushBorder");
            FastModeBorder.BorderThickness = new Thickness(UnselectedBorderThickness);
        }

        private void FastMode_Click(object sender, MouseButtonEventArgs e)
        {
            FastModeBorder.Background = (SolidColorBrush)FindResource("BrushCream2");
            FastModeBorder.BorderBrush = (SolidColorBrush)FindResource("BrushFeltLight");
            FastModeBorder.BorderThickness = new Thickness(SelectedBorderThickness);

            NormalModeBorder.Background = Brushes.Transparent;
            NormalModeBorder.BorderBrush = (SolidColorBrush)FindResource("BrushBorder");
            NormalModeBorder.BorderThickness = new Thickness(UnselectedBorderThickness);
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
            new CustomDialogWindow(new DialogContent { Message = string.Format(Strings.Lobby_NotificationCodeCopied, RoomCodeTextBlock.Text) }).ShowDialog();
        }

        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            var gameWindow = new GameWindow(_playerName);
            gameWindow.Show();
            this.Close();
        }
    }
}