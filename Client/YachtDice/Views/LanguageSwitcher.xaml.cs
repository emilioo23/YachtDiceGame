using System;
using System.Windows;
using System.Windows.Controls;
using YachtDice.Client.Logic.Settings;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Control in charge of changing the interface language and culture at runtime.
    /// </summary>
    public partial class LanguageSwitcher : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the floating language switch control.
        /// </summary>
        public LanguageSwitcher()
        {
            InitializeComponent();
            RefreshCurrentFlagDisplay();
        }

        /// <summary>
        /// Function that the containing window assigns to indicate how to reopen itself
        /// (with its own constructor parameters, for example the player name).
        /// </summary>
        public Func<Window> ReopenWindowFunc { get; set; }

        private void LanguageToggleButton_Click(object sender, RoutedEventArgs e)
        {
            LanguagePopup.IsOpen = LanguageToggleButton.IsChecked == true;
        }

        private void LanguageOptionButton_Click(object sender, RoutedEventArgs e)
        {
            var optionButton = (Button)sender;

            ApplyLanguage(optionButton.Tag.ToString());
        }

        /// <summary>
        /// Applies the chosen culture and reopens the current window to refresh the texts.
        /// </summary>
        /// <param name="cultureCode">Culture code, for example "es-MX" or "pt-BR".</param>
        private void ApplyLanguage(string cultureCode)
        {
            LanguagePopup.IsOpen = false;
            LanguageToggleButton.IsChecked = false;

            if (App.CurrentCultureCode != cultureCode)
            {
                App.SetCulture(cultureCode);
                ReopenParentWindow();
            }
        }

        /// <summary>
        /// Closes the window that contains this control and opens a new instance
        /// of the same type, already with the language applied (same pattern used in Settings).
        /// </summary>
        private void ReopenParentWindow()
        {
            Window currentWindow = Window.GetWindow(this);

            if (currentWindow != null)
            {
                ReopenWindowFunc.Invoke().Show();
                currentWindow.Close();
            }
        }

        private void RefreshCurrentFlagDisplay()
        {
            CurrentLangText.Text = LanguageCatalog.GetShortName(App.CurrentCultureCode);
            CurrentFlagImage.Source = FlagImages.Get(LanguageCatalog.GetFlagCode(App.CurrentCultureCode));
        }
    }
}
