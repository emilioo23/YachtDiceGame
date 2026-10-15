using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YachtDice.AuthenticationServiceReference;
using YachtDice.Gateway;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Window to validate the two-factor authentication (2FA) code.
    /// </summary>
    public partial class TwoFactorWindow : Window
    {
        private const int VisibleLocalPartChars = 2;
        private const char MaskCharacter = '*';

        private readonly string _email;
        private readonly string _playerDisplayName;
        private readonly AuthenticationGateway _gateway = new AuthenticationGateway();
        private readonly TextBox[] _digitBoxes;

        /// <summary>
        /// Initializes the two-step verification window for the given player.
        /// </summary>
        /// <param name="email">Email address the verification code was sent to.</param>
        /// <param name="playerDisplayName">Display name of the player who logged in.</param>
        public TwoFactorWindow(string email, string playerDisplayName)
        {
            InitializeComponent();

            _email = email;
            _playerDisplayName = playerDisplayName;
            _digitBoxes = new[]
            {
                Digit1TextBox, Digit2TextBox, Digit3TextBox,
                Digit4TextBox, Digit5TextBox, Digit6TextBox
            };

            DestinationEmailTextBlock.Text = MaskEmail(email);
            LanguageSwitcherControl.ReopenWindowFunc = () => new TwoFactorWindow(email, playerDisplayName);
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
            if (!IsCodeComplete())
            {
                ShowMessage(Strings.TwoFactor_ErrorIncompleteCode);
                return;
            }

            await VerifyCodeAsync();
        }

        private async void ResendLink_Click(object sender, MouseButtonEventArgs e)
        {
            var request = new ResendCodeRequestDto
            {
                Email = _email,
                CultureName = App.CurrentCultureCode
            };

            try
            {
                ResendResultCode result = await _gateway.ResendCodeAsync(request);
                HandleResendResult(result);
            }
            catch (GatewayException ex)
            {
                ShowMessage(GatewayErrorMessages.GetMessage(ex.ErrorCode));
            }
        }

        private void BackLink_Click(object sender, MouseButtonEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
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
            new CustomDialogWindow(new DialogContentDto { Message = message }).ShowDialog();
        }

        private bool IsCodeComplete()
        {
            return _digitBoxes.All(box => box.Text.Length > 0);
        }

        private async Task VerifyCodeAsync()
        {
            var request = new VerifyCodeRequestDto
            {
                Email = _email,
                Code = string.Concat(_digitBoxes.Select(box => box.Text))
            };

            try
            {
                VerificationResultCode result = await _gateway.VerifyCodeAsync(request);
                HandleVerificationResult(result);
            }
            catch (GatewayException ex)
            {
                ShowMessage(GatewayErrorMessages.GetMessage(ex.ErrorCode));
            }
        }

        private void HandleVerificationResult(VerificationResultCode result)
        {
            switch (result)
            {
                case VerificationResultCode.Valid:
                    OpenMenuWindow();
                    break;
                case VerificationResultCode.Expired:
                    ShowMessage(Strings.TwoFactor_ErrorCodeExpired);
                    ClearDigits();
                    break;
                case VerificationResultCode.Locked:
                    ShowMessage(Strings.Dialogs_D19_Validation2FA);
                    ClearDigits();
                    break;
                default:
                    ShowMessage(Strings.Dialogs_D18_Validation2FA);
                    ClearDigits();
                    break;
            }
        }

        private void HandleResendResult(ResendResultCode result)
        {
            switch (result)
            {
                case ResendResultCode.CodeSent:
                    ClearDigits();
                    ShowMessage(Strings.TwoFactor_CodeResentMessage);
                    break;
                case ResendResultCode.ResendTooSoon:
                    ShowMessage(Strings.TwoFactor_ErrorResendTooSoon);
                    break;
                case ResendResultCode.VerificationLocked:
                    ShowMessage(Strings.Dialogs_D19_Validation2FA);
                    break;
                default:
                    ShowMessage(Strings.TwoFactor_ErrorSendFailed);
                    break;
            }
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

            for (int index = 0; index < digits.Length; index++)
            {
                _digitBoxes[index].Text = digits[index].ToString();
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