using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using YachtDice.AuthenticationServiceReference;
using YachtDice.Gateway;
using YachtDice.RegistrationServiceReference;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Window where the player logs in or creates an account.
    /// </summary>
    public partial class LoginWindow : Window
    {
        private const string UnexpectedErrorLogMessage = "Unexpected exception during authentication.";

        private readonly AppLogger _logger = new AppLogger(typeof(LoginWindow));
        private readonly AuthenticationGateway _gateway = new AuthenticationGateway();

        /// <summary>
        /// Initializes the login and registration window.
        /// </summary>
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginTabButton_Click(object sender, RoutedEventArgs e)
        {
            LoginPanel.Visibility = Visibility.Visible;
            RegisterPanel.Visibility = Visibility.Collapsed;
            LoginTabButton.Style = (Style)FindResource(ResourceKeys.ActiveTabStyleKey);
            RegisterTabButton.Style = (Style)FindResource(ResourceKeys.InactiveTabStyleKey);
            AuthSubmitButton.Content = Strings.Auth_LoginButton;
        }

        private void RegisterTabButton_Click(object sender, RoutedEventArgs e)
        {
            LoginPanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Visible;
            RegisterTabButton.Style = (Style)FindResource(ResourceKeys.ActiveTabStyleKey);
            LoginTabButton.Style = (Style)FindResource(ResourceKeys.InactiveTabStyleKey);
            AuthSubmitButton.Content = Strings.Auth_CreateAccountButton;
        }

        private async void AuthSubmitButton_Click(object sender, RoutedEventArgs e)
        {
            AuthSubmitButton.IsEnabled = false;

            try
            {
                bool isLogin = LoginPanel.Visibility == Visibility.Visible;

                if (isLogin)
                {
                    await ProcessLoginAsync();
                }
                else
                {
                    await ProcessRegisterAsync();
                }
            }
            catch (GatewayException ex)
            {
                ShowDialogMessage(GatewayErrorMessages.GetMessage(ex.ErrorCode));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, UnexpectedErrorLogMessage);
                ShowDialogMessage(Strings.Dialogs_UnexpectedError);
            }
            finally
            {
                AuthSubmitButton.IsEnabled = true;
            }
        }

        private void GuestButton_Click(object sender, RoutedEventArgs e)
        {
            var menuWindow = new MenuWindow(Strings.Auth_GuestDisplayName);
            menuWindow.Show();
            this.Close();
        }

        private void ForgotPasswordLink_Click(object sender, MouseButtonEventArgs e)
        {
            var forgotPasswordWindow = new ForgotPasswordWindow();
            forgotPasswordWindow.Show();
            this.Close();
        }

        private static string GetLoginMessage(LoginResultCode code)
        {
            string message;

            switch (code)
            {
                case LoginResultCode.MissingRequiredField:
                    message = Strings.Auth_ErrorRequiredFields;
                    break;
                case LoginResultCode.InvalidCredentials:
                    message = Strings.Dialogs_D13_Login;
                    break;
                case LoginResultCode.VerificationLocked:
                    message = Strings.Dialogs_D19_Validation2FA;
                    break;
                case LoginResultCode.CodeSendFailed:
                    message = Strings.TwoFactor_ErrorSendFailed;
                    break;
                default:
                    message = Strings.Dialogs_UnexpectedError;
                    break;
            }

            return message;
        }

        private static string GetRegisterMessage(RegisterResultCode code)
        {
            string message;

            switch (code)
            {
                case RegisterResultCode.MissingRequiredField:
                    message = Strings.Auth_ErrorRequiredFields;
                    break;
                case RegisterResultCode.FieldTooLong:
                    message = Strings.Auth_ErrorFieldTooLong;
                    break;
                case RegisterResultCode.InvalidEmail:
                    message = Strings.Auth_ErrorInvalidEmail;
                    break;
                case RegisterResultCode.EmailAlreadyRegistered:
                    message = Strings.Dialogs_D16_Register;
                    break;
                case RegisterResultCode.UsernameAlreadyTaken:
                    message = Strings.Dialogs_D15_Register;
                    break;
                default:
                    message = Strings.Dialogs_UnexpectedError;
                    break;
            }

            return message;
        }

        private async Task ProcessLoginAsync()
        {
            var credentials = new LoginCredentials
            {
                Email = EmailTextBox.Text.Trim(),
                Password = LoginPasswordBox.Password,
                CultureName = App.CurrentCultureCode
            };

            LoginData response = await _gateway.LoginAsync(credentials);

            if (response.Code == LoginResultCode.VerificationCodeSent)
            {
                OpenVerificationWindow(credentials.Email, response.DisplayName, response.ChallengeId);
            }
            else
            {
                ShowDialogMessage(GetLoginMessage(response.Code));
            }
        }

        private async Task ProcessRegisterAsync()
        {
            var newPlayer = new RegistrationServiceReference.Player
            {
                FirstName = FirstNameTextBox.Text.Trim(),
                LastName = LastNameTextBox.Text.Trim(),
                Username = UsernameTextBox.Text.Trim(),
                Email = RegisterEmailTextBox.Text.Trim(),
                Password = RegisterPasswordBox.Password
            };

            var registrationGateway = new RegistrationGateway();
            RegisterResultCode result = await registrationGateway.RegisterPlayerAsync(newPlayer);

            if (result == RegisterResultCode.Success)
            {
                ShowDialogMessage(Strings.Auth_SuccessRegister);
                LoginTabButton_Click(this, new RoutedEventArgs());
            }
            else
            {
                ShowDialogMessage(GetRegisterMessage(result));
            }
        }

        private void OpenVerificationWindow(string email, string playerDisplayName, string challengeId)
        {
            var twoFactorWindow = new TwoFactorWindow(email, playerDisplayName, challengeId);
            twoFactorWindow.Show();
            this.Close();
        }

        private void ShowDialogMessage(string message)
        {
            var dialog = new CustomDialogWindow(new DialogContent { Message = message })
            {
                Owner = this
            };

            dialog.ShowDialog();
        }
    }
}