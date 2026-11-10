using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using YachtDice.AuthenticationServiceReference;
using YachtDice.Client.Logic.Authentication;
using YachtDice.Gateway;
using YachtDice.Navigation;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Window where the player types the recovery code received by email and chooses the new password.
    /// </summary>
    public partial class ResetPasswordWindow : Window
    {
        private readonly string _email;
        private readonly AppLogger _logger;
        private readonly AuthenticationGateway _gateway = new AuthenticationGateway();

        /// <summary>
        /// Initializes the window for the account that asked for the recovery code.
        /// </summary>
        /// <param name="email">Email of the account to recover.</param>
        public ResetPasswordWindow(string email)
        {
            InitializeComponent();
            _email = email;
            _logger = new AppLogger(GetType());
            EmailTextBlock.Text = email;
            CodeTextBox.MaxLength = ResetPasswordInputValidator.CodeLength;
            LanguageSwitcherControl.ReopenWindowFunc = () => new ResetPasswordWindow(email);
            CodeTextBox.Focus();
        }

        private static string GetInputMessage(ResetPasswordInputStatus status)
        {
            string message;

            switch (status)
            {
                case ResetPasswordInputStatus.IncompleteCode:
                    message = Strings.ResetPassword_ErrorIncompleteCode;
                    break;
                case ResetPasswordInputStatus.EmptyPassword:
                    message = Strings.ResetPassword_ErrorEmptyPassword;
                    break;
                case ResetPasswordInputStatus.PasswordsDoNotMatch:
                    message = Strings.ResetPassword_ErrorPasswordsDoNotMatch;
                    break;
                default:
                    message = Strings.Dialogs_UnexpectedError;
                    break;
            }

            return message;
        }

        private static string GetResultMessage(PasswordResetResultCode code)
        {
            string message;

            switch (code)
            {
                case PasswordResetResultCode.MissingRequiredField:
                    message = Strings.Auth_ErrorRequiredFields;
                    break;
                case PasswordResetResultCode.IncorrectCode:
                    message = Strings.ResetPassword_ErrorIncorrectCode;
                    break;
                case PasswordResetResultCode.CodeExpired:
                    message = Strings.ResetPassword_ErrorCodeExpired;
                    break;
                case PasswordResetResultCode.TooManyAttempts:
                    message = Strings.ResetPassword_ErrorTooManyAttempts;
                    break;
                default:
                    message = Strings.Dialogs_UnexpectedError;
                    break;
            }

            return message;
        }

        private void CodeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private async void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePasswordButton.IsEnabled = false;
            await GatewayCallRunner.RunAsync(this, _logger, ChangePasswordAsync);
            ChangePasswordButton.IsEnabled = true;
        }

        private async void ResendLink_Click(object sender, MouseButtonEventArgs e)
        {
            await GatewayCallRunner.RunAsync(this, _logger, ResendCodeAsync);
        }

        private void BackToLoginLink_Click(object sender, MouseButtonEventArgs e)
        {
            WindowNavigator.SwitchTo(this, new LoginWindow());
        }

        private async Task ChangePasswordAsync()
        {
            ResetPasswordInputStatus input = ResetPasswordInputValidator.Validate(
                CodeTextBox.Text.Trim(),
                NewPasswordBox.Password,
                ConfirmPasswordBox.Password);

            if (input == ResetPasswordInputStatus.Valid)
            {
                await SendNewPasswordAsync();
            }
            else
            {
                CustomDialogWindow.ShowMessage(this, GetInputMessage(input));
            }
        }

        private async Task SendNewPasswordAsync()
        {
            var data = new PasswordResetData
            {
                Email = _email,
                Code = CodeTextBox.Text.Trim(),
                NewPassword = NewPasswordBox.Password
            };

            PasswordResetResultCode result = await _gateway.ConfirmPasswordResetAsync(data);

            if (result == PasswordResetResultCode.PasswordChanged)
            {
                CustomDialogWindow.ShowMessage(this, Strings.ResetPassword_SuccessMessage);
                WindowNavigator.SwitchTo(this, new LoginWindow());
            }
            else
            {
                CustomDialogWindow.ShowMessage(this, GetResultMessage(result));
            }
        }

        private async Task ResendCodeAsync()
        {
            PasswordResetRequestResultCode result =
                await _gateway.RequestPasswordResetAsync(_email, App.CurrentCultureCode);
            string message = Strings.ForgotPassword_CodeSentMessage;

            if (result != PasswordResetRequestResultCode.CodeSent)
            {
                message = Strings.ForgotPassword_ErrorSendFailed;
            }

            CustomDialogWindow.ShowMessage(this, message);
        }
    }
}
