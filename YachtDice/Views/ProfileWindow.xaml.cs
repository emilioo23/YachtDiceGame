using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using YachtDice.Resources;

namespace YachtDice.Views
{
    /// <summary>
    /// Ventana de edicion de perfil del jugador.
    /// </summary>
    /// <summary>
    /// Ventana que permite visualizar y editar la información del perfil del jugador y sus estadísticas.
    /// </summary>
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

            var avatarBorder = (Border)AvatarInitialTextBlock.Parent;
            avatarBorder.Background = swatch.Background;
        }

        private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            // Aqui, mas adelante, se guardaran los cambios reales en el backend.
            MessageBox.Show(Strings.Profile_SavedMessage);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var menuWindow = new MenuWindow(_playerName);
            menuWindow.Show();
            this.Close();
        }
    }
}