using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace YachtDice.Views
{
    public partial class ProfileWindow : Window
    {
        private readonly string _playerName;

        /// <summary>
        /// Inicializa la ventana de perfil para el jugador indicado.
        /// </summary>
        /// <param name="playerName">Nombre del jugador cuyo perfil se esta editando.</param>
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

            // Cambia el color de fondo del avatar grande al color elegido.
            var avatarBorder = (Border)AvatarInitialTextBlock.Parent;
            avatarBorder.Background = swatch.Background;
        }

        private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            // Aqui, mas adelante, se guardaran los cambios reales en el backend.
            MessageBox.Show("Perfil guardado correctamente.");
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var menuWindow = new MenuWindow(_playerName);
            menuWindow.Show();
            this.Close();
        }
    }
}