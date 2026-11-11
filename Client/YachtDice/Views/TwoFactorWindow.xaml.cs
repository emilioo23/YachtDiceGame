using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YachtDice.Gateway;
using YachtDice.Models;
using YachtDice.Navigation;
using YachtDice.Resources;
using YachtDice.Utils;
using AuthResendResult = YachtDice.AuthenticationServiceReference.ResendResultCode;
using AuthVerificationCode = YachtDice.AuthenticationServiceReference.VerificationCode;
using AuthVerificationResult = YachtDice.AuthenticationServiceReference.VerificationResultCode;
using TwoFactorResendResult = YachtDice.TwoFactorManagementServiceReference.ResendResultCode;
using TwoFactorVerificationCode = YachtDice.TwoFactorManagementServiceReference.VerificationCode;
using TwoFactorVerificationResult = YachtDice.TwoFactorManagementServiceReference.VerificationResultCode;

namespace YachtDice.Views
{
    /// <summary>
    /// Window where the player types the verification code received by email.
    /// </summary>
    public partial class TwoFactorWindow : Window
    {
        private const int VisibleLocalPartChars = 2;
        private const int FirstCharacterIndex = 0;
        private const char MaskCharacter = '*';
        private const char AtSign = '@';
        private const int MaxDigitsPerBox = 1;

        private readonly string _email;
        private readonly PlayerSession _session;
        private readonly string _challengeId;
        private readonly bool _isActivation;

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
            _session = challenge.Session;
            _challengeId = challenge.ChallengeId;
            _isActivation = challenge.IsActivation;
            _digitBoxes = new[]
            {
                Digit1TextBox, Digit2TextBox, Digit3TextBox,
                Digit4TextBox, Digit5TextBox, Digit6TextBox
            };

            foreach (TextBox box in _digitBoxes)
            {
                box.MaxLength = MaxDigitsPerBox;
            }

            DestinationEmailTextBlock.Text = _email;

            if (!_isActivation)
            {
                DestinationEmailTextBlock.Text = MaskEmail(_email);
            }

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
            bool isEmptyBoxAfterFirst = box.Text.Length == 0 && index > 0;
            bool isBackOnEmptyBox = e.Key == Key.Back && isEmptyBoxAfterFirst;

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

            if (!string.IsNullOrEmpty(box.Text) && hasNextBox)
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
                CustomDialogWindow.ShowMessage(this, Strings.TwoFactor_ErrorIncompleteCode);
            }
        }

        private async void ResendLink_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (_isActivation)
                {
                    TwoFactorResendResult result = await _twoFactorGateway.ResendActivationCodeAsync(
                        _session.DisplayName,
                        _challengeId,
                        App.CurrentCultureCode);

                    HandleResendResult((AuthResendResult)(int)result);
                }
                else
                {
                    AuthResendResult result = await _authGateway.ResendCodeAsync(
                        _email,
                        _challengeId,
                        App.CurrentCultureCode);

                    HandleResendResult(result);
                }
            }
            catch (GatewayException ex)
            {
                CustomDialogWindow.ShowMessage(this, GatewayErrorMessages.GetMessage(ex.ErrorCode));
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
                WindowNavigator.SwitchTo(this, new LoginWindow());
            }
        }

        private static string MaskEmail(string email)
        {
            int atIndex = email.IndexOf(AtSign);
            string maskedEmail = email;

            if (atIndex > VisibleLocalPartChars)
            {
                string visiblePart = email.Substring(FirstCharacterIndex, VisibleLocalPartChars);
                string hiddenPart = new string(MaskCharacter, atIndex - VisibleLocalPartChars);

                maskedEmail = visiblePart + hiddenPart + email.Substring(atIndex);
            }

            return maskedEmail;
        }

        private bool IsCodeComplete()
        {
            Func<TextBox, bool> hasDigit = box => !string.IsNullOrEmpty(box.Text);
            bool isComplete = _digitBoxes.All(hasDigit);

            return isComplete;
        }

        private async Task VerifyCodeAsync()
        {
            string code = string.Concat(_digitBoxes.Select(box => box.Text));

            try
            {
                if (_isActivation)
                {
                    var verificationCode = new TwoFactorVerificationCode { Code = code, ChallengeId = _challengeId };
                    TwoFactorVerificationResult result = await _twoFactorGateway.ConfirmActivationAsync(
                        _session.DisplayName,
                        verificationCode);

                    HandleVerificationResult((AuthVerificationResult)(int)result);
                }
                else
                {
                    var verificationCode = new AuthVerificationCode { Code = code, ChallengeId = _challengeId };
                    AuthVerificationResult result = await _authGateway.VerifyCodeAsync(_email, verificationCode);
                    HandleVerificationResult(result);
                }
            }
            catch (GatewayException ex)
            {
                CustomDialogWindow.ShowMessage(this, GatewayErrorMessages.GetMessage(ex.ErrorCode));
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
                    CustomDialogWindow.ShowMessage(this, Strings.TwoFactor_ErrorCodeExpired);
                    ClearDigits();
                    break;
                case AuthVerificationResult.Locked:
                    CustomDialogWindow.ShowMessage(this, Strings.Dialogs_TooManyAttempts);
                    ClearDigits();
                    break;
                default:
                    CustomDialogWindow.ShowMessage(this, Strings.Dialogs_IncorrectCode);
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
                    CustomDialogWindow.ShowMessage(this, Strings.TwoFactor_CodeResentMessage);
                    break;
                case AuthResendResult.ResendTooSoon:
                    CustomDialogWindow.ShowMessage(this, Strings.TwoFactor_ErrorResendTooSoon);
                    break;
                case AuthResendResult.VerificationLocked:
                    CustomDialogWindow.ShowMessage(this, Strings.Dialogs_TooManyAttempts);
                    break;
                default:
                    CustomDialogWindow.ShowMessage(this, Strings.TwoFactor_ErrorSendFailed);
                    break;
            }
        }

        private void CompleteVerification()
        {
            if (_isActivation)
            {
                CustomDialogWindow.ShowMessage(this, Strings.Settings_Security_TwoFactorEnabledMessage);
                OpenSettingsWindow();
            }
            else
            {
                OpenMenuWindow();
            }
        }

        private void OpenSettingsWindow()
        {
            WindowNavigator.SwitchTo(this, new SettingsWindow(_session, SettingsWindow.SecuritySection));
        }

        private void OpenMenuWindow()
        {
            WindowNavigator.SwitchTo(this, new MenuWindow(_session));
        }

        private void PasteCodeFromClipboard()
        {
            string clipboardText = string.Empty;

            if (Clipboard.ContainsText())
            {
                clipboardText = Clipboard.GetText();
            }

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