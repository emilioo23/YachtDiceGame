using System.Windows;
using System.Windows.Input;
using YachtDice.Resources;

namespace YachtDice.Views
{
    public partial class LoginWindow : Window
    {
        /// <summary>
        /// Inicializa la ventana de inicio de sesion y registro.
        /// </summary>
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginTabButton_Click(object sender, RoutedEventArgs e)
        {
            LoginPanel.Visibility = Visibility.Visible;
            RegisterPanel.Visibility = Visibility.Collapsed;
            LoginTabButton.Style = (Style)FindResource("TabButtonActive");
            RegisterTabButton.Style = (Style)FindResource("TabButtonInactive");
            AuthSubmitButton.Content = Strings.Auth_LoginButton;
        }

        private void RegisterTabButton_Click(object sender, RoutedEventArgs e)
        {
            LoginPanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Visible;
            RegisterTabButton.Style = (Style)FindResource("TabButtonActive");
            LoginTabButton.Style = (Style)FindResource("TabButtonInactive");
            AuthSubmitButton.Content = Strings.Auth_CreateAccountButton;
        }

        private void AuthSubmitButton_Click(object sender, RoutedEventArgs e)
        {
            bool isLogin = LoginPanel.Visibility == Visibility.Visible;

            if (isLogin)
            {
                string email = string.IsNullOrWhiteSpace(EmailTextBox.Text) ? "usuario@ejemplo.com" : EmailTextBox.Text;
                var twoFactorWindow = new TwoFactorWindow(email);
                twoFactorWindow.Show();
                this.Close();
            }
            else
            {
                // Aqui, mas adelante, se validara el registro real.
                MessageBox.Show("Aqui se conectara la logica de registro mas adelante.");
            }
        }

        private void GuestButton_Click(object sender, RoutedEventArgs e)
        {
            var menuWindow = new MenuWindow("Invitado");
            menuWindow.Show();
            this.Close();
        }

        private void ForgotPasswordLink_Click(object sender, MouseButtonEventArgs e)
        {
            var forgotPasswordWindow = new ForgotPasswordWindow();
            forgotPasswordWindow.Show();
            this.Close();
        }
    }
}