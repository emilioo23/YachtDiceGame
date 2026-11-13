using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using YachtDice.AuthenticationServiceReference;
using YachtDice.Client.Logic.Authentication;
using YachtDice.Gateway;
using YachtDice.Models;
using YachtDice.Navigation;
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
        private const string WarmUpFailedLogMessage = "The connection to the server could not be warmed up.";

        private readonly AppLogger _logger;
        private readonly AuthenticationGateway _gateway = new AuthenticationGateway();

        /// <summary>
        /// Initializes the login and registration window.
        /// </summary>
        public LoginWindow()
        {
            InitializeComponent();
            _logger = new AppLogger(GetType());
            LanguageSwitcherControl.ReopenWindowFunc = () => new LoginWindow();
            ApplyFieldLimits();
            _ = WarmUpConnectionAsync();
        }

        private async Task WarmUpConnectionAsync()
        {
            try
            {
                await _gateway.WarmUpAsync();
            }
            catch (GatewayException)
            {
                _logger.Warning(WarmUpFailedLogMessage);
            }
        }

        private void ApplyFieldLimits()
        {
            EmailTextBox.MaxLength = RegistrationLimits.EmailMaxLength;
            FirstNameTextBox.MaxLength = RegistrationLimits.NameMaxLength;
            LastNameTextBox.MaxLength = RegistrationLimits.NameMaxLength;
            UsernameTextBox.MaxLength = RegistrationLimits.UsernameMaxLength;
            RegisterEmailTextBox.MaxLength = RegistrationLimits.EmailMaxLength;
        }

        private void LoginTabButton_Click(object sender, RoutedEventArgs e)
        {
            ShowLoginPanel();
        }

        private void ShowLoginPanel()
        {
            LoginPanel.Visibility = Visibility.Visible;
            RegisterPanel.Visibility = Visibility.Collapsed;
            LoginTabButton.Style = (Style)FindResource(ResourceKeys.ActiveTabStyleKey);
            RegisterTabButton.Style = (Style)FindResource(ResourceKeys.InactiveTabStyleKey);
            AuthSubmitButton.Content = Strings.Auth_ButtonLogin;
        }

        private void RegisterTabButton_Click(object sender, RoutedEventArgs e)
        {
            LoginPanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Visible;
            RegisterTabButton.Style = (Style)FindResource(ResourceKeys.ActiveTabStyleKey);
            LoginTabButton.Style = (Style)FindResource(ResourceKeys.InactiveTabStyleKey);
            AuthSubmitButton.Content = Strings.Auth_ButtonCreateAccount;
        }

        private async void AuthSubmitButton_Click(object sender, RoutedEventArgs e)
        {
            AuthSubmitButton.IsEnabled = false;

            try
            {
                await GatewayCallRunner.RunAsync(this, _logger, SubmitAuthFormAsync);
            }
            finally
            {
                AuthSubmitButton.IsEnabled = true;
            }
        }

        private async Task SubmitAuthFormAsync()
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

        private void GuestButton_Click(object sender, RoutedEventArgs e)
        {
            var session = new PlayerSession(Strings.Auth_GuestDisplayName, string.Empty, isGuest: true);

            WindowNavigator.SwitchTo(this, new MenuWindow(session));
        }

        private void ForgotPasswordLink_Click(object sender, MouseButtonEventArgs e)
        {
            WindowNavigator.SwitchTo(this, new ForgotPasswordWindow());
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
                    message = Strings.Dialogs_EmailRegistered;
                    break;
                case RegisterResultCode.UsernameAlreadyTaken:
                    message = Strings.Dialogs_UsernameTaken;
                    break;
                case RegisterResultCode.InvalidUsername:
                    message = Strings.Auth_ErrorInvalidUsername;
                    break;
                case RegisterResultCode.WeakPassword:
                    message = Strings.Auth_ErrorWeakPassword;
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

            LoginData loginData = await _gateway.LoginAsync(credentials);
            var session = new PlayerSession(loginData.DisplayName, loginData.FirstName, isGuest: false);

            if (loginData.Code == LoginResultCode.VerificationCodeSent)
            {
                OpenVerificationWindow(credentials.Email, session, loginData.ChallengeId);
            }
            else if (loginData.Code == LoginResultCode.LoggedIn)
            {
                WindowNavigator.SwitchTo(this, new MenuWindow(session));
            }
            else
            {
                CustomDialogWindow.ShowMessage(this, LoginResultMessages.GetMessage(loginData.Code));
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
                CustomDialogWindow.ShowMessage(this, Strings.Auth_SuccessRegister);
                ShowLoginPanel();
            }
            else
            {
                CustomDialogWindow.ShowMessage(this, GetRegisterMessage(result));
            }
        }

        private void OpenVerificationWindow(string email, PlayerSession session, string challengeId)
        {
            var challenge = new VerificationChallenge
            {
                Email = email,
                Session = session,
                ChallengeId = challengeId,
                IsActivation = false
            };

            WindowNavigator.SwitchTo(this, new TwoFactorWindow(challenge));
        }
    }
}