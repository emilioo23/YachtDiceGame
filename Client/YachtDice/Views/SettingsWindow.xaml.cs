using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using YachtDice.Client.Logic.Authentication;
using YachtDice.Client.Logic.Settings;
using YachtDice.Gateway;
using YachtDice.Models;
using YachtDice.Navigation;
using YachtDice.PlayerServiceReference;
using YachtDice.Resources;
using YachtDice.TwoFactorManagementServiceReference;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Settings window to adjust application preferences such as the theme and notifications.
    /// </summary>
    public partial class SettingsWindow : Window
    {
        /// <summary>
        /// Name of the section that has the password and the two-step verification.
        /// </summary>
        public const string SecuritySection = "Security";

        private const string AccountSection = "Account";
        private const string AppearanceSection = "Appearance";
        private const string NotificationsSection = "Notifications";
        private const string PreferencesSection = "Preferences";
        private const string PrivacySection = "Privacy";
        private const int ToggleKnobMargin = 3;
        private const int NoMargin = 0;

        private readonly PlayerSession _session;
        private readonly TwoFactorManagementGateway _twoFactorGateway = new TwoFactorManagementGateway();
        private readonly PlayerGateway _playerGateway = new PlayerGateway();
        private readonly AppLogger _logger = new AppLogger(typeof(SettingsWindow));
        private bool _isInitializing;
        private bool _isTwoFactorEnabled;

        /// <summary>
        /// Initializes the settings window for the given player.
        /// </summary>
        /// <param name="session">Session of the player who opens the settings.</param>
        /// <param name="initialSection">Section that must be shown when the window opens.</param>
        public SettingsWindow(PlayerSession session, string initialSection = AccountSection)
        {
            InitializeComponent();
            TwoFactorPasswordBox.MaxLength = RegistrationLimits.PasswordMaxLength;
            CurrentPasswordBox.MaxLength = RegistrationLimits.PasswordMaxLength;
            NewPasswordBox.MaxLength = RegistrationLimits.PasswordMaxLength;
            ConfirmNewPasswordBox.MaxLength = RegistrationLimits.PasswordMaxLength;
            DeleteAccountPasswordBox.MaxLength = RegistrationLimits.PasswordMaxLength;
            _session = session;
            _isInitializing = true;

            AccountUsernameTextBlock.Text = session.DisplayName;
            LanguageComboBox.SelectedIndex = LanguageCatalog.IndexOf(App.CurrentCultureCode);
            ApplyDarkThemeToggleVisual(App.IsDarkTheme);
            ApplyToggleVisual(SoundsToggle, SoundsToggleKnob, App.Audio.AreEffectsEnabled);
            ApplyToggleVisual(MusicToggle, MusicToggleKnob, !App.Audio.IsMusicMuted);
            MusicVolumeSlider.Value = App.Audio.MusicVolume;

            ShowSection(initialSection);
            _isInitializing = false;

            _ = LoadTwoFactorStateAsync();
        }

        private void ShowSection(string sectionName)
        {
            ShowSectionPanel(AccountPanel, AccountNavButton, sectionName == AccountSection);
            ShowSectionPanel(AppearancePanel, AppearanceNavButton, sectionName == AppearanceSection);
            ShowSectionPanel(SecurityPanel, SecurityNavButton, sectionName == SecuritySection);
            ShowSectionPanel(NotificationsPanel, NotificationsNavButton, sectionName == NotificationsSection);
            ShowSectionPanel(PreferencesPanel, PreferencesNavButton, sectionName == PreferencesSection);
            ShowSectionPanel(PrivacyPanel, PrivacyNavButton, sectionName == PrivacySection);
        }

        private void ShowSectionPanel(UIElement panel, Control navButton, bool isSelected)
        {
            panel.Visibility = isSelected ? Visibility.Visible : Visibility.Collapsed;

            string styleKey = ThemeResourceKeys.OutlineButtonStyleKey;

            if (isSelected)
            {
                styleKey = ThemeResourceKeys.ActiveTabStyleKey;
            }

            navButton.Style = (Style)FindResource(styleKey);
        }

        private void ApplyDarkThemeToggleVisual(bool isDark)
        {
            ApplyToggleVisual(DarkThemeToggle, DarkThemeToggleKnob, isDark);
        }

        private void ApplyToggleVisual(Border toggle, Border knob, bool isOn)
        {
            if (isOn)
            {
                toggle.Background = (SolidColorBrush)FindResource(ThemeResourceKeys.FeltLightBrushKey);
                knob.HorizontalAlignment = HorizontalAlignment.Right;
                knob.Margin = new Thickness(NoMargin, NoMargin, ToggleKnobMargin, NoMargin);
            }
            else
            {
                toggle.Background = (SolidColorBrush)FindResource(ThemeResourceKeys.BorderBrushKey);
                knob.HorizontalAlignment = HorizontalAlignment.Left;
                knob.Margin = new Thickness(ToggleKnobMargin, NoMargin, NoMargin, NoMargin);
            }
        }

        private void ShowTwoFactorState(bool isEnabled)
        {
            _isTwoFactorEnabled = isEnabled;
            TwoFactorToggleButton.Content = isEnabled
                ? Strings.Settings_Security_ButtonTwoFactorDisable
                : Strings.Settings_Security_ButtonTwoFactorEnable;
            TwoFactorPasswordBox.Visibility = isEnabled ? Visibility.Visible : Visibility.Collapsed;
            TwoFactorToggleButton.IsEnabled = true;
            CreateBackupCodesButton.IsEnabled = isEnabled;

            if (!isEnabled)
            {
                BackupCodesStatusTextBlock.Text = Strings.Settings_Security_BackupCodesOff;
                BackupCodesTextBox.Clear();
                BackupCodesTextBox.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowBackupCodeCount(int codeCount)
        {
            BackupCodesStatusTextBlock.Text = string.Format(Strings.Settings_Security_BackupCodesLeft, codeCount);
        }

        private async Task LoadTwoFactorStateAsync()
        {
            try
            {
                bool isEnabled = await _twoFactorGateway.IsTwoFactorEnabledAsync(_session.DisplayName);
                ShowTwoFactorState(isEnabled);

                if (isEnabled)
                {
                    ShowBackupCodeCount(await _twoFactorGateway.CountBackupCodesAsync(_session.SessionToken));
                }
            }
            catch (GatewayException ex)
            {
                CustomDialogWindow.ShowMessage(this, GatewayErrorMessages.GetMessage(ex.ErrorCode));
            }
        }

        private async Task StartTwoFactorActivationAsync()
        {
            TwoFactorActivationData data = await _twoFactorGateway.StartTwoFactorActivationAsync(
                _session.SessionToken,
                App.CurrentCultureCode);

            if (data.Code == TwoFactorActivationResultCode.CodeSent)
            {
                var challenge = new VerificationChallenge
                {
                    Email = data.MaskedEmail,
                    Session = _session,
                    ChallengeId = data.ChallengeId,
                    IsActivation = true
                };

                WindowNavigator.SwitchTo(this, new TwoFactorWindow(challenge));
            }
            else if (data.Code == TwoFactorActivationResultCode.AlreadyEnabled)
            {
                ShowTwoFactorState(true);
            }
            else
            {
                CustomDialogWindow.ShowMessage(this, GetActivationMessage(data.Code));
            }
        }

        private async Task DisableTwoFactorAsync()
        {
            TwoFactorChangeResultCode result = await _twoFactorGateway.DisableTwoFactorAsync(
                _session.SessionToken,
                TwoFactorPasswordBox.Password);

            if (result == TwoFactorChangeResultCode.Success)
            {
                TwoFactorPasswordBox.Clear();
                ShowTwoFactorState(false);
                CustomDialogWindow.ShowMessage(this, Strings.Settings_Security_TwoFactorDisabledMessage);
            }
            else if (result == TwoFactorChangeResultCode.InvalidPassword)
            {
                CustomDialogWindow.ShowMessage(this, Strings.Dialogs_InvalidCredentials);
            }
            else
            {
                CustomDialogWindow.ShowMessage(this, Strings.Settings_Security_TwoFactorUnavailableMessage);
            }
        }

        private static string GetActivationMessage(TwoFactorActivationResultCode code)
        {
            string message;

            switch (code)
            {
                case TwoFactorActivationResultCode.PlayerNotFound:
                    message = Strings.Settings_Security_TwoFactorUnavailableMessage;
                    break;
                case TwoFactorActivationResultCode.VerificationLocked:
                    message = Strings.Dialogs_TooManyAttempts;
                    break;
                case TwoFactorActivationResultCode.CodeSendFailed:
                    message = Strings.TwoFactor_ErrorSendFailed;
                    break;
                default:
                    message = Strings.Dialogs_UnexpectedError;
                    break;
            }

            return message;
        }

        private async void TwoFactorToggleButton_Click(object sender, RoutedEventArgs e)
        {
            TwoFactorToggleButton.IsEnabled = false;

            try
            {
                if (_isTwoFactorEnabled)
                {
                    await DisableTwoFactorAsync();
                }
                else
                {
                    await StartTwoFactorActivationAsync();
                }
            }
            catch (GatewayException ex)
            {
                CustomDialogWindow.ShowMessage(this, GatewayErrorMessages.GetMessage(ex.ErrorCode));
            }
            finally
            {
                TwoFactorToggleButton.IsEnabled = true;
            }
        }

        private async void CreateBackupCodesButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TwoFactorPasswordBox.Password))
            {
                CustomDialogWindow.ShowMessage(this, Strings.Settings_Security_BackupCodesPasswordRequired);
            }
            else if (CustomDialogWindow.Confirm(this, Strings.Settings_Security_BackupCodesConfirm))
            {
                await CreateBackupCodesAsync();
            }
        }

        private async Task CreateBackupCodesAsync()
        {
            CreateBackupCodesButton.IsEnabled = false;

            try
            {
                BackupCodesData codesData = await _twoFactorGateway.CreateBackupCodesAsync(
                    _session.SessionToken,
                    TwoFactorPasswordBox.Password);

                if (codesData.Code == BackupCodesResultCode.Created)
                {
                    ShowBackupCodes(codesData.Codes);
                }

                CustomDialogWindow.ShowMessage(this, BackupCodesMessages.GetMessage(codesData.Code));
            }
            catch (GatewayException ex)
            {
                CustomDialogWindow.ShowMessage(this, GatewayErrorMessages.GetMessage(ex.ErrorCode));
            }
            finally
            {
                CreateBackupCodesButton.IsEnabled = _isTwoFactorEnabled;
            }
        }

        private void ShowBackupCodes(string[] codes)
        {
            TwoFactorPasswordBox.Clear();
            BackupCodesTextBox.Text = string.Join(Environment.NewLine, codes);
            BackupCodesTextBox.Visibility = Visibility.Visible;
            ShowBackupCodeCount(codes.Length);
        }

        private void ReopenSettingsWindow(string sectionToShow)
        {
            WindowNavigator.SwitchTo(this, new SettingsWindow(_session, sectionToShow));
        }

        private void AccountNavButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSection(AccountSection);
        }

        private void AppearanceNavButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSection(AppearanceSection);
        }

        private void SecurityNavButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSection(SecuritySection);
        }

        private void NotificationsNavButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSection(NotificationsSection);
        }

        private void PreferencesNavButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSection(PreferencesSection);
        }

        private void PrivacyNavButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSection(PrivacySection);
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = LanguageComboBox.SelectedItem as ComboBoxItem;

            if (!_isInitializing && selectedItem != null)
            {
                string cultureCode = selectedItem.Tag.ToString();
                App.SetCulture(cultureCode);
                ReopenSettingsWindow(PreferencesSection);
            }
        }

        private void DarkThemeToggle_Click(object sender, MouseButtonEventArgs e)
        {
            App.SetTheme(!App.IsDarkTheme);
            ReopenSettingsWindow(AppearanceSection);
        }

        private void SoundsToggle_Click(object sender, MouseButtonEventArgs e)
        {
            App.Audio.SetEffectsEnabled(!App.Audio.AreEffectsEnabled);
            ApplyToggleVisual(SoundsToggle, SoundsToggleKnob, App.Audio.AreEffectsEnabled);
        }

        private void MusicToggle_Click(object sender, MouseButtonEventArgs e)
        {
            App.Audio.SetMusicMuted(!App.Audio.IsMusicMuted);
            ApplyToggleVisual(MusicToggle, MusicToggleKnob, !App.Audio.IsMusicMuted);
        }

        private void MusicVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isInitializing)
            {
                App.Audio.SetMusicVolume((int)e.NewValue);
            }
        }

        private void EditProfileButton_Click(object sender, RoutedEventArgs e)
        {
            WindowNavigator.SwitchTo(this, new ProfileWindow(_session));
        }

        private async void DeleteAccountButton_Click(object sender, RoutedEventArgs e)
        {
            bool isConfirmed = CustomDialogWindow.Confirm(this, Strings.Settings_DeleteAccount_FirstConfirmation)
                && CustomDialogWindow.Confirm(this, Strings.Settings_DeleteAccount_FinalConfirmation);

            if (isConfirmed)
            {
                var button = (Button)sender;

                button.IsEnabled = false;
                await GatewayCallRunner.RunAsync(this, _logger, DeleteAccountAsync);
                button.IsEnabled = true;
            }
        }

        private async Task DeleteAccountAsync()
        {
            AccountDeletionResultCode result =
                await _playerGateway.DeleteAccountAsync(_session.SessionToken, DeleteAccountPasswordBox.Password);

            if (result == AccountDeletionResultCode.Success)
            {
                await Task.Run(() => _session.Presence.Disconnect());

                CustomDialogWindow.ShowMessage(this, Strings.Settings_DeleteAccount_DoneMessage);
                WindowNavigator.SwitchTo(this, new LoginWindow());
            }
            else if (result == AccountDeletionResultCode.WrongPassword)
            {
                CustomDialogWindow.ShowMessage(this, Strings.Settings_DeleteAccount_WrongPassword);
            }
            else
            {
                CustomDialogWindow.ShowMessage(this, Strings.Settings_DeleteAccount_Error);
            }
        }

        private void ChangePasswordToggleButton_Click(object sender, RoutedEventArgs e)
        {
            bool willShow = ChangePasswordPanel.Visibility != Visibility.Visible;

            CurrentPasswordBox.Clear();
            NewPasswordBox.Clear();
            ConfirmNewPasswordBox.Clear();
            ChangePasswordPanel.Visibility = willShow ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void SavePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            SavePasswordButton.IsEnabled = false;
            await GatewayCallRunner.RunAsync(this, _logger, ChangePasswordAsync);
            SavePasswordButton.IsEnabled = true;
        }

        private async Task ChangePasswordAsync()
        {
            PasswordChangeInputStatus inputStatus = PasswordChangeInputValidator.Validate(
                CurrentPasswordBox.Password,
                NewPasswordBox.Password,
                ConfirmNewPasswordBox.Password);

            if (inputStatus == PasswordChangeInputStatus.Valid)
            {
                await SendPasswordChangeAsync();
            }
            else
            {
                CustomDialogWindow.ShowMessage(this, GetPasswordInputMessage(inputStatus));
            }
        }

        private async Task SendPasswordChangeAsync()
        {
            var change = new PasswordChange
            {
                CurrentPassword = CurrentPasswordBox.Password,
                NewPassword = NewPasswordBox.Password
            };

            PasswordChangeResultCode result =
                await _twoFactorGateway.ChangePasswordAsync(_session.SessionToken, change);

            CustomDialogWindow.ShowMessage(this, PasswordChangeMessages.GetMessage(result));

            if (result == PasswordChangeResultCode.Success)
            {
                ChangePasswordToggleButton_Click(ChangePasswordToggleButton, new RoutedEventArgs());
            }
        }

        private static string GetPasswordInputMessage(PasswordChangeInputStatus status)
        {
            string message = Strings.Dialogs_UnexpectedError;

            switch (status)
            {
                case PasswordChangeInputStatus.EmptyCurrentPassword:
                    message = Strings.Settings_Password_ErrorEmptyCurrent;
                    break;
                case PasswordChangeInputStatus.EmptyNewPassword:
                    message = Strings.ResetPassword_ErrorEmptyPassword;
                    break;
                case PasswordChangeInputStatus.PasswordsDoNotMatch:
                    message = Strings.ResetPassword_ErrorPasswordsDoNotMatch;
                    break;
            }

            return message;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            WindowNavigator.SwitchTo(this, new MenuWindow(_session));
        }
    }
}