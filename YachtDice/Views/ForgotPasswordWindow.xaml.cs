using System.Windows;
using System.Windows.Input;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Ventana que gestiona el flujo de recuperación de contraseña de la cuenta del usuario.
    /// </summary>
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
                new CustomDialogWindow(new DialogContentDto { Message = Strings.ForgotPassword_ErrorRequiredEmail }).ShowDialog();
                return;
            }

            // Aqui, mas adelante, se conectara el envio real del correo de recuperacion.
            new CustomDialogWindow(new DialogContentDto { Message = string.Format(Strings.ForgotPassword_LinkSentMessage, EmailTextBox.Text) }).ShowDialog();
        }

        private void BackToLoginLink_Click(object sender, MouseButtonEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}