using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using YachtDice.Data;
using YachtDice.Models;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Ventana de inicio de sesion y registro de jugadores.
    /// </summary>
    /// <summary>
    /// Ventana inicial para la autenticación de usuarios y acceso al sistema.
    /// </summary>
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
                ProcessLogin();
            }
            else
            {
                ProcessRegister();
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

        private void ProcessLogin()
        {
            string email = EmailTextBox.Text.Trim();
            string password = LoginPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show(Strings.Auth_ErrorRequiredFields);
                return;
            }

            using (var context = new YachtDiceContext())
            {
                Player player = context.Players.FirstOrDefault(p => p.Email == email);

                if (player == null)
                {
                    MessageBox.Show(Strings.Auth_ErrorUserNotFound);
                    return;
                }

                if (!PasswordHasher.Verify(password, player.PasswordHash))
                {
                    MessageBox.Show(Strings.Auth_ErrorWrongPassword);
                    return;
                }

                var twoFactorWindow = new TwoFactorWindow(player.Email, player.DisplayName);
                twoFactorWindow.Show();
                this.Close();
            }
        }

        private void ProcessRegister()
        {
            string firstName = FirstNameTextBox.Text.Trim();
            string lastName = LastNameTextBox.Text.Trim();
            string username = UsernameTextBox.Text.Trim();
            string email = RegisterEmailTextBox.Text.Trim();
            string password = RegisterPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show(Strings.Auth_ErrorRequiredFields);
                return;
            }

            using (var context = new YachtDiceContext())
            {
                bool emailTaken = context.Players.Any(p => p.Email == email);

                if (emailTaken)
                {
                    MessageBox.Show(Strings.Auth_ErrorEmailTaken);
                    return;
                }

                bool usernameTaken = context.Players.Any(p => p.Username == username);

                if (usernameTaken)
                {
                    MessageBox.Show(Strings.Auth_ErrorUsernameTaken);
                    return;
                }

                var newPlayer = new Player
                {
                    AvatarId = 1,
                    Username = username,
                    Email = email,
                    PasswordHash = PasswordHasher.ComputeHash(password),
                    State = "Activo",
                    RegistrationDate = DateTime.Now,
                    Xp = 0,
                    Level = 1,
                    DisplayName = username,
                    FirstName = firstName,
                    LastName = lastName
                };

                context.Players.Add(newPlayer);
                context.SaveChanges();
            }

            MessageBox.Show(Strings.Auth_SuccessRegister);
            LoginTabButton_Click(this, new RoutedEventArgs());
        }
    }
}