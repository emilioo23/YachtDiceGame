using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using YachtDice.Gateway;
using YachtDice.Models;
using YachtDice.Navigation;
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
        private const int NoMargin = 0;

        private readonly PlayerSession _session;
        private readonly TwoFactorManagementGateway _twoFactorGateway = new TwoFactorManagementGateway();
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
            _session = session;
            _isInitializing = true;

            AccountUsernameTextBlock.Text = session.DisplayName;
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

            string styleKey = ThemeResourceKeys.OutlineButtonStyleKey;

            if (isSelected)
            {
                styleKey = ThemeResourceKeys.ActiveTabStyleKey;
            }

            navButton.Style = (Style)FindResource(styleKey);
        }

        private void ApplyDarkThemeToggleVisual(bool isDark)
        {
            if (isDark)
            {
                DarkThemeToggle.Background = (SolidColorBrush)FindResource(ThemeResourceKeys.FeltLightBrushKey);
                DarkThemeToggleKnob.HorizontalAlignment = HorizontalAlignment.Right;
                DarkThemeToggleKnob.Margin = new Thickness(NoMargin, NoMargin, ToggleKnobMargin, NoMargin);
            }
            else
            {
                DarkThemeToggle.Background = (SolidColorBrush)FindResource(ThemeResourceKeys.BorderBrushKey);
                DarkThemeToggleKnob.HorizontalAlignment = HorizontalAlignment.Left;
                DarkThemeToggleKnob.Margin = new Thickness(ToggleKnobMargin, NoMargin, NoMargin, NoMargin);
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
        }

        private async Task LoadTwoFactorStateAsync()
        {
            try
            {
                bool isEnabled = await _twoFactorGateway.IsTwoFactorEnabledAsync(_session.DisplayName);
                ShowTwoFactorState(isEnabled);
            }
            catch (GatewayException ex)
            {
                CustomDialogWindow.ShowMessage(this, GatewayErrorMessages.GetMessage(ex.ErrorCode));
            }
        }

        private async Task StartTwoFactorActivationAsync()
        {
            TwoFactorActivationData data = await _twoFactorGateway.StartTwoFactorActivationAsync(
                _session.DisplayName,
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
                _session.DisplayName,
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

        private void EditProfileButton_Click(object sender, RoutedEventArgs e)
        {
            WindowNavigator.SwitchTo(this, new ProfileWindow(_session));
        }

        private void DeleteAccountButton_Click(object sender, RoutedEventArgs e)
        {
            CustomDialogWindow.ShowMessage(this, Strings.Settings_FeatureNotImplementedMessage);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            WindowNavigator.SwitchTo(this, new MenuWindow(_session));
        }
    }
}