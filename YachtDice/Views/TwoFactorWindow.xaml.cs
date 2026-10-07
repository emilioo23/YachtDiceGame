using System;
using YachtDice.Utils;
using YachtDice.Views;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YachtDice.Resources;
using YachtDice.Services;

namespace YachtDice.Views
{
    /// <summary>
    /// Ventana para la validación del código de autenticación de dos factores (2FA).
    /// </summary>
    public partial class TwoFactorWindow : Window
    {
        private const int CodeLength = TwoFactorCodeService.CodeLength;
        private const int VisibleLocalPartChars = 2;
        private const char MaskCharacter = '*';

        private readonly string _email;
        private readonly string _playerDisplayName;
        private readonly TwoFactorCodeService _codeService;
        private readonly TwoFactorDeliveryService _deliveryService;
        private readonly TextBox[] _digitBoxes;

        /// <summary>
        /// Inicializa la ventana de verificación en dos pasos para el jugador indicado.
        /// </summary>
        /// <param name="email">Correo al que se envió el código de verificación.</param>
        /// <param name="playerDisplayName">Nombre para mostrar del jugador que inició sesión.</param>
        /// <param name="codeService">Servicio que guarda y valida los códigos generados.</param>
        public TwoFactorWindow(string email, string playerDisplayName, TwoFactorCodeService codeService)
        {
            InitializeComponent();

            _email = email;
            _playerDisplayName = playerDisplayName;
            _codeService = codeService ?? throw new ArgumentNullException(nameof(codeService));
            _deliveryService = new TwoFactorDeliveryService(_codeService);
            _digitBoxes = new[]
            {
                Digit1TextBox, Digit2TextBox, Digit3TextBox,
                Digit4TextBox, Digit5TextBox, Digit6TextBox
            };

            DestinationEmailTextBlock.Text = MaskEmail(email);
            LanguageSwitcherControl.ReopenWindowFunc = () => new TwoFactorWindow(email, playerDisplayName, codeService);
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

        private void DigitTextBox_TextChanged(object sender, TextChangedEventArgs e)
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
                VerifyCode();
            }
        }

        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsCodeComplete())
            {
                new CustomDialogWindow(new DialogContentDto { Message = Strings.TwoFactor_ErrorIncompleteCode }).ShowDialog();
                return;
            }

            VerifyCode();
        }

        private async void ResendLink_Click(object sender, MouseButtonEventArgs e)
        {
            if (_codeService.IsLocked(_email))
            {
                new CustomDialogWindow(new DialogContentDto { Message = Strings.Dialogs_D19_Validation2FA }).ShowDialog();
                return;
            }

            if (!_codeService.IsResendAllowed(_email))
            {
                new CustomDialogWindow(new DialogContentDto { Message = Strings.TwoFactor_ErrorResendTooSoon }).ShowDialog();
                return;
            }

            bool wasSent = await _deliveryService.SendCodeAsync(_email, _playerDisplayName);

            if (wasSent)
            {
                ClearDigits();
                new CustomDialogWindow(new DialogContentDto { Message = Strings.TwoFactor_CodeResentMessage }).ShowDialog();
            }
            else
            {
                new CustomDialogWindow(new DialogContentDto { Message = Strings.TwoFactor_ErrorSendFailed }).ShowDialog();
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

        private bool IsCodeComplete()
        {
            return _digitBoxes.All(box => box.Text.Length > 0);
        }

        private void VerifyCode()
        {
            string code = string.Concat(_digitBoxes.Select(box => box.Text));
            TwoFactorValidationResult result = _codeService.Validate(_email, code);

            HandleValidationResult(result);
        }

        private void HandleValidationResult(TwoFactorValidationResult result)
        {
            switch (result)
            {
                case TwoFactorValidationResult.Valid:
                    OpenMenuWindow();
                    break;
                case TwoFactorValidationResult.Expired:
                    new CustomDialogWindow(new DialogContentDto { Message = Strings.TwoFactor_ErrorCodeExpired }).ShowDialog();
                    ClearDigits();
                    break;
                case TwoFactorValidationResult.Locked:
                    new CustomDialogWindow(new DialogContentDto { Message = Strings.Dialogs_D19_Validation2FA }).ShowDialog();
                    ClearDigits();
                    break;
                default:
                    new CustomDialogWindow(new DialogContentDto { Message = Strings.Dialogs_D18_Validation2FA }).ShowDialog();
                    ClearDigits();
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
            var digits = new string(clipboardText.Where(char.IsDigit).Take(CodeLength).ToArray());

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