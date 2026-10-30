using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using YachtDice.Gateway;
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
        private const string SpanishCultureCode = "es-MX";
        private const int SpanishLanguageIndex = 0;
        private const int OtherLanguageIndex = 1;
        private const int ToggleKnobMargin = 3;

        private readonly string _playerName;
        private readonly TwoFactorManagementGateway _twoFactorGateway = new TwoFactorManagementGateway();
        private bool _isInitializing;
        private bool _isTwoFactorEnabled;

        /// <summary>
        /// Initializes the settings window for the given player.
        /// </summary>
        /// <param name="playerName">Name of the player who opens the settings.</param>
        /// <param name="initialSection">Section that must be shown when the window opens.</param>
        public SettingsWindow(string playerName, string initialSection = AccountSection)
        {
            InitializeComponent();
            _playerName = playerName;
            _isInitializing = true;

            AccountUsernameTextBlock.Text = playerName;
            LanguageComboBox.SelectedIndex = App.CurrentCultureCode == SpanishCultureCode
                ? SpanishLanguageIndex
                : OtherLanguageIndex;
            ApplyDarkThemeToggleVisual(App.IsDarkTheme);

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
            navButton.Style = (Style)FindResource(
                isSelected ? ResourceKeys.ActiveTabStyleKey : ResourceKeys.OutlineButtonStyleKey);
        }

        private void ApplyDarkThemeToggleVisual(bool isDark)
        {
            DarkThemeToggle.Background = isDark
                ? (SolidColorBrush)FindResource(ResourceKeys.FeltLightBrushKey)
                : (SolidColorBrush)FindResource(ResourceKeys.BorderBrushKey);
            DarkThemeToggleKnob.HorizontalAlignment = isDark ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            DarkThemeToggleKnob.Margin = isDark
                ? new Thickness(0, 0, ToggleKnobMargin, 0)
                : new Thickness(ToggleKnobMargin, 0, 0, 0);
        }

        private void ShowMessage(string message)
        {
            var dialog = new CustomDialogWindow(new DialogContent { Message = message })
            {
                Owner = this
            };

            dialog.ShowDialog();
        }

        private void ShowTwoFactorState(bool isEnabled)
        {
            _isTwoFactorEnabled = isEnabled;
            TwoFactorToggleButton.Content = isEnabled
                ? Strings.Settings_Security_TwoFactorDisableButton
                : Strings.Settings_Security_TwoFactorEnableButton;
            TwoFactorPasswordBox.Visibility = isEnabled ? Visibility.Visible : Visibility.Collapsed;
            TwoFactorToggleButton.IsEnabled = true;
        }

        private async Task LoadTwoFactorStateAsync()
        {
            try
            {
                bool isEnabled = await _twoFactorGateway.IsTwoFactorEnabledAsync(_playerName);
                ShowTwoFactorState(isEnabled);
            }
            catch (GatewayException ex)
            {
                ShowMessage(GatewayErrorMessages.GetMessage(ex.ErrorCode));
            }
        }

        private async Task StartTwoFactorActivationAsync()
        {
            TwoFactorActivationData data = await _twoFactorGateway.StartTwoFactorActivationAsync(
                _playerName,
                App.CurrentCultureCode);

            if (data.Code == TwoFactorActivationResultCode.CodeSent)
            {
                var challenge = new VerificationChallenge
                {
                    Email = data.MaskedEmail,
                    PlayerDisplayName = _playerName,
                    ChallengeId = data.ChallengeId,
                    IsActivation = true
                };

                var twoFactorWindow = new TwoFactorWindow(challenge);
                twoFactorWindow.Show();
                this.Close();
            }
            else if (data.Code == TwoFactorActivationResultCode.AlreadyEnabled)
            {
                ShowTwoFactorState(true);
            }
            else
            {
                ShowMessage(GetActivationMessage(data.Code));
            }
        }

        private async Task DisableTwoFactorAsync()
        {
            TwoFactorChangeResultCode result = await _twoFactorGateway.DisableTwoFactorAsync(
                _playerName,
                TwoFactorPasswordBox.Password);

            if (result == TwoFactorChangeResultCode.Success)
            {
                TwoFactorPasswordBox.Clear();
                ShowTwoFactorState(false);
                ShowMessage(Strings.Settings_Security_TwoFactorDisabledMessage);
            }
            else if (result == TwoFactorChangeResultCode.InvalidPassword)
            {
                ShowMessage(Strings.Dialogs_D13_Login);
            }
            else
            {
                ShowMessage(Strings.Settings_Security_TwoFactorUnavailableMessage);
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
                    message = Strings.Dialogs_D19_Validation2FA;
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
                ShowMessage(GatewayErrorMessages.GetMessage(ex.ErrorCode));
            }
            finally
            {
                TwoFactorToggleButton.IsEnabled = true;
            }
        }

        private void ReopenSettingsWindow(string sectionToShow)
        {
            var settingsWindow = new SettingsWindow(_playerName, sectionToShow);
            settingsWindow.Show();
            this.Close();
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

        private void EditProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var profileWindow = new ProfileWindow(_playerName);
            profileWindow.Show();
            this.Close();
        }

        private void DeleteAccountButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Connect the real account deletion flow when the session token exists.
            var content = new DialogContent { Message = Strings.Settings_FeatureNotImplementedMessage };
            new CustomDialogWindow(content).ShowDialog();
        }

        private void BackLink_Click(object sender, MouseButtonEventArgs e)
        {
            var menuWindow = new MenuWindow(_playerName);
            menuWindow.Show();
            this.Close();
        }
    }
}