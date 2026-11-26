using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using YachtDice.AuthenticationServiceReference;
using YachtDice.Client.Logic.Authentication;
using YachtDice.Client.Logic.Logging;
using YachtDice.Composition;
using YachtDice.Gateway;
using YachtDice.Models;
using YachtDice.Navigation;
using YachtDice.Passkeys;
using YachtDice.RegistrationServiceReference;
using YachtDice.Resources;
using YachtDice.Utils;
using PasskeyAssertionData = YachtDice.TwoFactorManagementServiceReference.PasskeyAssertionData;
using PasskeyChallengeData = YachtDice.TwoFactorManagementServiceReference.PasskeyChallengeData;
using PasskeyStartResultCode = YachtDice.TwoFactorManagementServiceReference.PasskeyStartResultCode;
using TwoFactorLoginData = YachtDice.TwoFactorManagementServiceReference.LoginData;
using TwoFactorLoginResultCode = YachtDice.TwoFactorManagementServiceReference.LoginResultCode;

namespace YachtDice.Views
{
    /// <summary>
    /// Window where the player logs in or creates an account.
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly AppLogger _logger;
        private readonly ClientServices _services;
        private readonly AuthenticationGateway _gateway = new AuthenticationGateway();
        private readonly TwoFactorManagementGateway _passkeyGateway = new TwoFactorManagementGateway();
        private readonly WindowsPasskeyAuthenticator _passkeys = new WindowsPasskeyAuthenticator();

        /// <summary>
        /// Initializes the login and registration window.
        /// </summary>
        /// <param name="services">Components that the windows share while the application is open.</param>
        public LoginWindow(ClientServices services)
        {
            InitializeComponent();
            _logger = new AppLogger(GetType());
            _services = services ?? throw new ArgumentNullException(nameof(services), LogMessages.NullServicesMessage);
            LanguageSwitcherControl.ReopenWindowFunc = () => new LoginWindow(services);
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
                _logger.Warning(LogMessages.WarmUpFailedLogMessage);
            }
        }

        private void ApplyFieldLimits()
        {
            LoginIdentifierTextBox.MaxLength = RegistrationLimits.EmailMaxLength;
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
            LoginTabButton.Style = (Style)FindResource(ThemeResourceKeys.ActiveTabStyleKey);
            RegisterTabButton.Style = (Style)FindResource(ThemeResourceKeys.InactiveTabStyleKey);
            PasskeyPanel.Visibility = Visibility.Visible;
            AuthSubmitButton.Content = Strings.Auth_ButtonLogin;
            LoginIdentifierTextBox.Focus();
        }

        private void RegisterTabButton_Click(object sender, RoutedEventArgs e)
        {
            LoginPanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Visible;
            RegisterTabButton.Style = (Style)FindResource(ThemeResourceKeys.ActiveTabStyleKey);
            LoginTabButton.Style = (Style)FindResource(ThemeResourceKeys.InactiveTabStyleKey);
            PasskeyPanel.Visibility = Visibility.Collapsed;
            AuthSubmitButton.Content = Strings.Auth_ButtonCreateAccount;
            FirstNameTextBox.Focus();
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

        private async void PasskeyButton_Click(object sender, RoutedEventArgs e)
        {
            PasskeyButton.IsEnabled = false;

            try
            {
                await GatewayCallRunner.RunAsync(this, _logger, LoginWithPasskeyAsync);
            }
            finally
            {
                PasskeyButton.IsEnabled = true;
            }
        }

        private async Task LoginWithPasskeyAsync()
        {
            if (!_passkeys.IsAvailable())
            {
                CustomDialogWindow.ShowMessage(this, Strings.Passkey_ErrorUnavailable);
            }
            else
            {
                PasskeyChallengeData challenge = await _passkeyGateway.BeginPasskeyLoginAsync();

                if (challenge.Code == PasskeyStartResultCode.Created)
                {
                    await SendPasskeyAnswerAsync(challenge);
                }
                else
                {
                    CustomDialogWindow.ShowMessage(this, PasskeyMessages.GetStartMessage(challenge.Code));
                }
            }
        }

        private async Task SendPasskeyAnswerAsync(PasskeyChallengeData challenge)
        {
            PasskeyAssertionData assertion = await AskWindowsForAssertionAsync(challenge);

            if (assertion != null)
            {
                TwoFactorLoginData loginData = await _passkeyGateway.CompletePasskeyLoginAsync(assertion);

                if (loginData.Code == TwoFactorLoginResultCode.LoggedIn)
                {
                    var session = new PlayerSession(loginData.DisplayName, loginData.FirstName, _services)
                    {
                        SessionToken = loginData.SessionToken ?? string.Empty
                    };

                    WindowNavigator.SwitchTo(this, new MenuWindow(session));
                }
                else
                {
                    CustomDialogWindow.ShowMessage(this, PasskeyMessages.GetLoginMessage(loginData.Code));
                }
            }
        }

        private async Task<PasskeyAssertionData> AskWindowsForAssertionAsync(PasskeyChallengeData challenge)
        {
            IntPtr windowHandle = new WindowInteropHelper(this).Handle;
            PasskeyAssertionData assertion = null;

            try
            {
                assertion = await Task.Run(() => _passkeys.GetAssertion(windowHandle, challenge));

                if (assertion == null)
                {
                    CustomDialogWindow.ShowMessage(this, Strings.Passkey_LoginNotCompleted);
                }
            }
            catch (PasskeyException ex)
            {
                _logger.Error(ex, LogMessages.LoginPasskeyFailedLogMessage);
                CustomDialogWindow.ShowMessage(this, Strings.Passkey_ErrorWindows);
            }

            return assertion;
        }

        private void GuestButton_Click(object sender, RoutedEventArgs e)
        {
            var session = new PlayerSession(Strings.Auth_GuestDisplayName, string.Empty, _services)
            {
                IsGuest = true
            };

            WindowNavigator.SwitchTo(this, new MenuWindow(session));
        }

        private void ForgotPasswordLink_Click(object sender, MouseButtonEventArgs e)
        {
            WindowNavigator.SwitchTo(this, new ForgotPasswordWindow(_services));
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
                case RegisterResultCode.InvalidUsername:
                    message = Strings.Auth_ErrorInvalidUsername;
                    break;
                case RegisterResultCode.InvalidName:
                    message = Strings.Auth_ErrorInvalidName;
                    break;
                case RegisterResultCode.WeakPassword:
                    message = Strings.Auth_ErrorWeakPassword;
                    break;
                default:
                    message = GetAccountConflictMessage(code);
                    break;
            }

            return message;
        }

        private static string GetAccountConflictMessage(RegisterResultCode code)
        {
            string message;

            switch (code)
            {
                case RegisterResultCode.EmailAlreadyRegistered:
                    message = Strings.Dialogs_EmailRegistered;
                    break;
                case RegisterResultCode.UsernameAlreadyTaken:
                    message = Strings.Dialogs_UsernameTaken;
                    break;
                case RegisterResultCode.OffensiveUsername:
                    message = Strings.Auth_ErrorOffensiveUsername;
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
                Identifier = LoginIdentifierTextBox.Text.Trim(),
                Password = LoginPasswordBox.Password,
                CultureName = App.CurrentCultureCode
            };

            LoginData loginData = await _gateway.LoginAsync(credentials);
            var session = new PlayerSession(loginData.DisplayName, loginData.FirstName, _services)
            {
                SessionToken = loginData.SessionToken ?? string.Empty
            };

            if (loginData.Code == LoginResultCode.VerificationCodeSent)
            {
                OpenVerificationWindow(loginData.Email, session, loginData.ChallengeId);
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