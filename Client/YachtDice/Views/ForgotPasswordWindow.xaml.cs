using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using YachtDice.AuthenticationServiceReference;
using YachtDice.Client.Logic.Authentication;
using YachtDice.Client.Logic.Logging;
using YachtDice.Composition;
using YachtDice.Gateway;
using YachtDice.Navigation;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Window where the player asks for the code that lets them recover the password of the account.
    /// </summary>
    public partial class ForgotPasswordWindow : Window
    {
        private readonly AppLogger _logger;
        private readonly ClientServices _services;
        private readonly AuthenticationGateway _gateway = new AuthenticationGateway();

        /// <summary>
        /// Initializes the password recovery window.
        /// </summary>
        /// <param name="services">Components that the windows share while the application is open.</param>
        public ForgotPasswordWindow(ClientServices services)
        {
            InitializeComponent();
            EmailTextBox.MaxLength = RegistrationLimits.EmailMaxLength;
            _logger = new AppLogger(GetType());
            _services = services ?? throw new ArgumentNullException(nameof(services), LogMessages.NullServicesMessage);
            LanguageSwitcherControl.ReopenWindowFunc = () => new ForgotPasswordWindow(services);
        }

        private static string GetRequestMessage(PasswordResetRequestResultCode code)
        {
            string message;

            switch (code)
            {
                case PasswordResetRequestResultCode.MissingRequiredField:
                    message = Strings.ForgotPassword_ErrorRequiredEmail;
                    break;
                case PasswordResetRequestResultCode.CodeSendFailed:
                    message = Strings.ForgotPassword_ErrorSendFailed;
                    break;
                default:
                    message = Strings.Dialogs_UnexpectedError;
                    break;
            }

            return message;
        }

        private async void SendCodeButton_Click(object sender, RoutedEventArgs e)
        {
            SendCodeButton.IsEnabled = false;
            await GatewayCallRunner.RunAsync(this, _logger, RequestCodeAsync);
            SendCodeButton.IsEnabled = true;
        }

        private void BackToLoginLink_Click(object sender, MouseButtonEventArgs e)
        {
            WindowNavigator.SwitchTo(this, new LoginWindow(_services));
        }

        private async Task RequestCodeAsync()
        {
            string email = EmailTextBox.Text.Trim();
            PasswordResetRequestResultCode result =
                await _gateway.RequestPasswordResetAsync(email, App.CurrentCultureCode);

            if (result == PasswordResetRequestResultCode.CodeSent)
            {
                CustomDialogWindow.ShowMessage(this, Strings.ForgotPassword_CodeSentMessage);
                WindowNavigator.SwitchTo(this, new ResetPasswordWindow(email, _services));
            }
            else
            {
                CustomDialogWindow.ShowMessage(this, GetRequestMessage(result));
            }
        }
    }
}
