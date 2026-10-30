using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YachtDice.Gateway;
using YachtDice.Resources;
using YachtDice.Utils;
using AuthResendResult = YachtDice.AuthenticationServiceReference.ResendResultCode;
using AuthVerificationResult = YachtDice.AuthenticationServiceReference.VerificationResultCode;
using TwoFactorResendResult = YachtDice.TwoFactorManagementServiceReference.ResendResultCode;
using TwoFactorVerificationResult = YachtDice.TwoFactorManagementServiceReference.VerificationResultCode;

namespace YachtDice.Views
{
    /// <summary>
    /// Window where the player types the verification code received by email.
    /// </summary>
    public partial class TwoFactorWindow : Window
    {
        private const int VisibleLocalPartChars = 2;
        private const char MaskCharacter = '*';

        private readonly string _email;
        private readonly string _playerDisplayName;
        private readonly string _challengeId;
        private readonly bool _isActivation;

        // Se instancian ambos gateways para cubrir los dos casos de uso de la ventana
        private readonly AuthenticationGateway _authGateway = new AuthenticationGateway();
        private readonly TwoFactorManagementGateway _twoFactorGateway = new TwoFactorManagementGateway();

        private readonly TextBox[] _digitBoxes;

        /// <summary>
        /// Initializes the verification window for a pending login.
        /// </summary>
        /// <param name="challenge">Data of the verification code that was sent to the player.</param>
        public TwoFactorWindow(VerificationChallenge challenge)
        {
            InitializeComponent();

            _email = challenge.Email;
            _playerDisplayName = challenge.PlayerDisplayName;
            _challengeId = challenge.ChallengeId;
            _isActivation = challenge.IsActivation;
            _digitBoxes = new[]
            {
                Digit1TextBox, Digit2TextBox, Digit3TextBox,
                Digit4TextBox, Digit5TextBox, Digit6TextBox
            };

            DestinationEmailTextBlock.Text = _isActivation ? _email : MaskEmail(_email);
            LanguageSwitcherControl.ReopenWindowFunc = () => new TwoFactorWindow(challenge);
            Digit1TextBox.Focus();
        }

        private void DigitTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void DigitTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var box = (TextBox)sender;
            int index = Array.IndexOf(_digitBoxes, box);
            bool isPaste = e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control;
            bool isBackOnEmptyBox = e.Key == Key.Back && box.Text.Length == 0 && index > 0;

            if (isPaste)
            {
                PasteCodeFromClipboard();
                e.Handled = true;
            }
            else if (isBackOnEmptyBox)
            {
                _digitBoxes[index - 1].Clear();
                _digitBoxes[index - 1].Focus();
                e.Handled = true;
            }
        }

        private async void DigitTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var box = (TextBox)sender;
            int index = Array.IndexOf(_digitBoxes, box);
            bool hasNextBox = index < _digitBoxes.Length - 1;

            if (box.Text.Length > 0 && hasNextBox)
            {
                _digitBoxes[index + 1].Focus();
            }
            else if (IsCodeComplete())
            {
                await VerifyCodeAsync();
            }
        }

        private async void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsCodeComplete())
            {
                await VerifyCodeAsync();
            }
            else
            {
                ShowMessage(Strings.TwoFactor_ErrorIncompleteCode);
            }
        }

        private async void ResendLink_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (_isActivation)
                {
                    TwoFactorResendResult result = await _twoFactorGateway.ResendActivationCodeAsync(_playerDisplayName, _challengeId, App.CurrentCultureCode);
                    HandleResendResult((AuthResendResult)(int)result);
                }
                else
                {
                    AuthResendResult result = await _authGateway.ResendCodeAsync(_email, _challengeId, App.CurrentCultureCode);
                    HandleResendResult(result);
                }
            }
            catch (GatewayException ex)
            {
                ShowMessage(GatewayErrorMessages.GetMessage(ex.ErrorCode));
            }
        }

        private void BackLink_Click(object sender, MouseButtonEventArgs e)
        {
            if (_isActivation)
            {
                OpenSettingsWindow();
            }
            else
            {
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }

        private static string MaskEmail(string email)
        {
            int atIndex = email.IndexOf('@');
            string maskedEmail = email;

            if (atIndex > VisibleLocalPartChars)
            {
                string visiblePart = email.Substring(0, VisibleLocalPartChars);
                string hiddenPart = new string(MaskCharacter, atIndex - VisibleLocalPartChars);

                maskedEmail = visiblePart + hiddenPart + email.Substring(atIndex);
            }

            return maskedEmail;
        }

        private static void ShowMessage(string message)
        {
            new CustomDialogWindow(new DialogContent { Message = message }).ShowDialog();
        }

        private bool IsCodeComplete()
        {
            return _digitBoxes.All(box => box.Text.Length > 0);
        }

        private async Task VerifyCodeAsync()
        {
            string code = string.Concat(_digitBoxes.Select(box => box.Text));

            try
            {
                if (_isActivation)
                {
                    TwoFactorVerificationResult result = await _twoFactorGateway.ConfirmActivationAsync(_playerDisplayName, code, _challengeId);
                    HandleVerificationResult((AuthVerificationResult)(int)result);
                }
                else
                {
                    AuthVerificationResult result = await _authGateway.VerifyCodeAsync(_email, code, _challengeId);
                    HandleVerificationResult(result);
                }
            }
            catch (GatewayException ex)
            {
                ShowMessage(GatewayErrorMessages.GetMessage(ex.ErrorCode));
            }
        }

        private void HandleVerificationResult(AuthVerificationResult result)
        {
            switch (result)
            {
                case AuthVerificationResult.Valid:
                    CompleteVerification();
                    break;
                case AuthVerificationResult.Expired:
                    ShowMessage(Strings.TwoFactor_ErrorCodeExpired);
                    ClearDigits();
                    break;
                case AuthVerificationResult.Locked:
                    ShowMessage(Strings.Dialogs_D19_Validation2FA);
                    ClearDigits();
                    break;
                default:
                    ShowMessage(Strings.Dialogs_D18_Validation2FA);
                    ClearDigits();
                    break;
            }
        }

        private void HandleResendResult(AuthResendResult result)
        {
            switch (result)
            {
                case AuthResendResult.CodeSent:
                    ClearDigits();
                    ShowMessage(Strings.TwoFactor_CodeResentMessage);
                    break;
                case AuthResendResult.ResendTooSoon:
                    ShowMessage(Strings.TwoFactor_ErrorResendTooSoon);
                    break;
                case AuthResendResult.VerificationLocked:
                    ShowMessage(Strings.Dialogs_D19_Validation2FA);
                    break;
                default:
                    ShowMessage(Strings.TwoFactor_ErrorSendFailed);
                    break;
            }
        }

        private void CompleteVerification()
        {
            if (_isActivation)
            {
                ShowMessage(Strings.Settings_Security_TwoFactorEnabledMessage);
                OpenSettingsWindow();
            }
            else
            {
                OpenMenuWindow();
            }
        }

        private void OpenSettingsWindow()
        {
            var settingsWindow = new SettingsWindow(_playerDisplayName, SettingsWindow.SecuritySection);
            settingsWindow.Show();
            this.Close();
        }

        private void OpenMenuWindow()
        {
            var menuWindow = new MenuWindow(_playerDisplayName);
            menuWindow.Show();
            this.Close();
        }

        private void PasteCodeFromClipboard()
        {
            string clipboardText = Clipboard.ContainsText() ? Clipboard.GetText() : string.Empty;
            var digits = new string(clipboardText.Where(char.IsDigit).Take(_digitBoxes.Length).ToArray());

            ClearDigits();

            for (int digitIndex = 0; digitIndex < digits.Length; digitIndex++)
            {
                _digitBoxes[digitIndex].Text = digits[digitIndex].ToString();
            }
        }

        private void ClearDigits()
        {
            foreach (TextBox box in _digitBoxes)
            {
                box.Clear();
            }

            Digit1TextBox.Focus();
        }
    }
}