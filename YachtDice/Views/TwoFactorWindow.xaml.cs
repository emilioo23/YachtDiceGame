using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YachtDice.Resources;

namespace YachtDice.Views
{
    /// <summary>
    /// Ventana de verificacion en dos pasos, mostrada despues de un login exitoso.
    /// </summary>
    /// <summary>
    /// Ventana para la validación del código de autenticación de dos factores (2FA).
    /// </summary>
    public partial class TwoFactorWindow : Window
    {
        private const int CodeLength = 4;

        private readonly string _playerDisplayName;

        /// <summary>
        /// Inicializa la ventana de verificacion en dos pasos para el jugador indicado.
        /// </summary>
        /// <param name="email">Correo al que se envio el codigo de verificacion.</param>
        /// <param name="playerDisplayName">Nombre para mostrar del jugador que inicio sesion.</param>
        public TwoFactorWindow(string email, string playerDisplayName)
        {
            InitializeComponent();
            _playerDisplayName = playerDisplayName;
            DestinationEmailTextBlock.Text = email;
            LanguageSwitcherControl.ReopenWindowFunc = () => new TwoFactorWindow(email, playerDisplayName);
            Digit1.Focus();
        }

        private void Digit_TextChanged(object sender, TextChangedEventArgs e)
        {
            var box = sender as TextBox;
            if (box == null || box.Text.Length == 0)
            {
                return;
            }

            if (box == Digit1)
            {
                Digit2.Focus();
            }
            else if (box == Digit2)
            {
                Digit3.Focus();
            }
            else if (box == Digit3)
            {
                Digit4.Focus();
            }
        }

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            string code = Digit1.Text + Digit2.Text + Digit3.Text + Digit4.Text;

            if (code.Length < CodeLength)
            {
                MessageBox.Show(Strings.TwoFactor_ErrorIncompleteCode);
                return;
            }

            // Aqui, mas adelante, se validara el codigo real contra el backend.
            var menuWindow = new MenuWindow(_playerDisplayName);
            menuWindow.Show();
            this.Close();
        }

        private void ResendLink_Click(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show(Strings.TwoFactor_CodeResentMessage);
        }

        private void BackLink_Click(object sender, MouseButtonEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}