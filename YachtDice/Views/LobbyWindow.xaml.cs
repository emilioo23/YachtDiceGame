using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace YachtDice.Views
{
    public partial class LobbyWindow : Window
    {
        private readonly string _playerName;

        /// <summary>
        /// Inicializa la ventana de sala de espera para el jugador indicado.
        /// </summary>
        /// <param name="playerName">Nombre del jugador que entra a la sala.</param>
        public LobbyWindow(string playerName)
        {
            InitializeComponent();
            _playerName = playerName;
            PlayerNameTextBlock.Text = playerName;
            PlayerInitialTextBlock.Text = playerName.Length > 0 ? playerName.Substring(0, 1).ToUpper() : "U";
            LanguageSwitcherControl.ReopenWindowFunc = () => new LobbyWindow(playerName);
        }

        private void NormalMode_Click(object sender, MouseButtonEventArgs e)
        {
            NormalModeBorder.Background = (SolidColorBrush)FindResource("BrushCream2");
            NormalModeBorder.BorderBrush = (SolidColorBrush)FindResource("BrushFeltLight");
            NormalModeBorder.BorderThickness = new Thickness(2);

            FastModeBorder.Background = Brushes.Transparent;
            FastModeBorder.BorderBrush = (SolidColorBrush)FindResource("BrushBorder");
            FastModeBorder.BorderThickness = new Thickness(1.5);
        }

        private void FastMode_Click(object sender, MouseButtonEventArgs e)
        {
            FastModeBorder.Background = (SolidColorBrush)FindResource("BrushCream2");
            FastModeBorder.BorderBrush = (SolidColorBrush)FindResource("BrushFeltLight");
            FastModeBorder.BorderThickness = new Thickness(2);

            NormalModeBorder.Background = Brushes.Transparent;
            NormalModeBorder.BorderBrush = (SolidColorBrush)FindResource("BrushBorder");
            NormalModeBorder.BorderThickness = new Thickness(1.5);
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
            MessageBox.Show("Codigo copiado: " + RoomCodeTextBlock.Text);
        }

        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            var gameWindow = new GameWindow(_playerName);
            gameWindow.Show();
            this.Close();
        }
    }
}