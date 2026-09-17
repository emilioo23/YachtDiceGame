using System.Windows;
using System.Windows.Input;

namespace YachtDice.Views
{
    public partial class ForgotPasswordWindow : Window
    {
        /// <summary>
        /// Inicializa la ventana de recuperacion de contrasena.
        /// </summary>
        public ForgotPasswordWindow()
        {
            InitializeComponent();
        }

        private void SendLinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                MessageBox.Show("Ingresa tu correo electronico.");
                return;
            }

            // Aqui, mas adelante, se conectara con el backend para enviar el enlace real.
            MessageBox.Show("Enlace de recuperacion enviado a " + EmailTextBox.Text);

            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void BackToLoginLink_Click(object sender, MouseButtonEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}