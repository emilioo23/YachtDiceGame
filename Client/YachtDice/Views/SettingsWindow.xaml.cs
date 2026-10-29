using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Settings window to adjust application preferences such as the theme and notifications.
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private const string AccountSection = "Account";
        private const string AppearanceSection = "Appearance";
        private const string SecuritySection = "Security";
        private const string NotificationsSection = "Notifications";
        private const string PreferencesSection = "Preferences";
        private const string PrivacySection = "Privacy";
        private const string SpanishCultureCode = "es-MX";
        private const int SpanishLanguageIndex = 0;
        private const int OtherLanguageIndex = 1;
        private const int ToggleKnobMargin = 3;

        private readonly string _playerName;
        private bool _isInitializing;

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
            // The real account deletion flow will be connected here later.
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