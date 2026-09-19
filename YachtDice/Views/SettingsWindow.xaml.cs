using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YachtDice.Resources;

namespace YachtDice.Views
{
    /// <summary>
    /// Ventana de configuración para ajustar preferencias de la aplicación como el tema y notificaciones.
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private const string AccountSection = "Account";
        private const string AppearanceSection = "Appearance";
        private const string SecuritySection = "Security";
        private const string NotificationsSection = "Notifications";
        private const string PreferencesSection = "Preferences";
        private const string PrivacySection = "Privacy";

        private readonly string _playerName;
        private bool _isInitializing;

        /// <summary>
        /// Inicializa la ventana de ajustes para el jugador indicado.
        /// </summary>
        /// <param name="playerName">Nombre del jugador que abre los ajustes.</param>
        /// <param name="initialSection">Seccion que debe mostrarse al abrir la ventana.</param>
        public SettingsWindow(string playerName, string initialSection = AccountSection)
        {
            InitializeComponent();
            _playerName = playerName;
            _isInitializing = true;

            AccountUsernameTextBlock.Text = playerName;
            LanguageComboBox.SelectedIndex = App.CurrentCultureCode == "es-MX" ? 0 : 1;
            SetDarkThemeToggleVisual(App.IsDarkTheme);

            ShowSection(initialSection);
            _isInitializing = false;
        }

        private void ShowSection(string sectionName)
        {
            AccountPanel.Visibility = sectionName == AccountSection ? Visibility.Visible : Visibility.Collapsed;
            AppearancePanel.Visibility = sectionName == AppearanceSection ? Visibility.Visible : Visibility.Collapsed;
            SecurityPanel.Visibility = sectionName == SecuritySection ? Visibility.Visible : Visibility.Collapsed;
            NotificationsPanel.Visibility = sectionName == NotificationsSection ? Visibility.Visible : Visibility.Collapsed;
            PreferencesPanel.Visibility = sectionName == PreferencesSection ? Visibility.Visible : Visibility.Collapsed;
            PrivacyPanel.Visibility = sectionName == PrivacySection ? Visibility.Visible : Visibility.Collapsed;

            AccountNavButton.Style = (Style)FindResource(sectionName == AccountSection ? "TabButtonActive" : "BtnOutline");
            AppearanceNavButton.Style = (Style)FindResource(sectionName == AppearanceSection ? "TabButtonActive" : "BtnOutline");
            SecurityNavButton.Style = (Style)FindResource(sectionName == SecuritySection ? "TabButtonActive" : "BtnOutline");
            NotificationsNavButton.Style = (Style)FindResource(sectionName == NotificationsSection ? "TabButtonActive" : "BtnOutline");
            PreferencesNavButton.Style = (Style)FindResource(sectionName == PreferencesSection ? "TabButtonActive" : "BtnOutline");
            PrivacyNavButton.Style = (Style)FindResource(sectionName == PrivacySection ? "TabButtonActive" : "BtnOutline");
        }

        private void SetDarkThemeToggleVisual(bool isDark)
        {
            DarkThemeToggle.Background = isDark
                ? (System.Windows.Media.SolidColorBrush)FindResource("BrushFeltLight")
                : (System.Windows.Media.SolidColorBrush)FindResource("BrushBorder");
            DarkThemeToggleKnob.HorizontalAlignment = isDark ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            DarkThemeToggleKnob.Margin = isDark ? new Thickness(0, 0, 3, 0) : new Thickness(3, 0, 0, 0);
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
            if (_isInitializing)
            {
                return;
            }

            var selectedItem = LanguageComboBox.SelectedItem as ComboBoxItem;
            if (selectedItem == null)
            {
                return;
            }

            string cultureCode = selectedItem.Tag.ToString();
            App.SetCulture(cultureCode);
            ReopenSettingsWindow(PreferencesSection);
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
            // Aqui, mas adelante, se conectara el flujo real de eliminacion de cuenta.
            MessageBox.Show(Strings.Settings_FeatureNotImplementedMessage);
        }

        private void BackLink_Click(object sender, MouseButtonEventArgs e)
        {
            var menuWindow = new MenuWindow(_playerName);
            menuWindow.Show();
            this.Close();
        }
    }
}