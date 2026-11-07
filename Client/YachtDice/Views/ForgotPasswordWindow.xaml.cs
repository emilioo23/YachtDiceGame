using System.Windows;
using System.Windows.Input;
using YachtDice.Navigation;
using YachtDice.Resources;

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
            LanguageSwitcherControl.ReopenWindowFunc = () => new ForgotPasswordWindow();
        }

        private void SendLinkButton_Click(object sender, RoutedEventArgs e)
        {
            string message = Strings.ForgotPassword_ErrorRequiredEmail;

            if (!string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                // TODO: Connect the real sending of the recovery email.
                message = string.Format(Strings.ForgotPassword_LinkSentMessage, EmailTextBox.Text);
            }

            CustomDialogWindow.ShowMessage(this, message);
        }

        private void BackToLoginLink_Click(object sender, MouseButtonEventArgs e)
        {
            WindowNavigator.SwitchTo(this, new LoginWindow());
        }
    }
}