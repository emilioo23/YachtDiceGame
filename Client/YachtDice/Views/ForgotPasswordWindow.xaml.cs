using System.Windows;
using System.Windows.Input;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Window that handles the password recovery flow of the user's account.
    /// </summary>
    public partial class ForgotPasswordWindow : Window
    {
        /// <summary>
        /// Initializes the password recovery window.
        /// </summary>
        public ForgotPasswordWindow()
        {
            InitializeComponent();
        }

        private void SendLinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                new CustomDialogWindow(new DialogContent { Message = Strings.ForgotPassword_ErrorRequiredEmail }).ShowDialog();
                return;
            }

            // The real sending of the recovery email will be connected here later.
            new CustomDialogWindow(new DialogContent { Message = string.Format(Strings.ForgotPassword_LinkSentMessage, EmailTextBox.Text) }).ShowDialog();
        }

        private void BackToLoginLink_Click(object sender, MouseButtonEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}