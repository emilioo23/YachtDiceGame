using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace YachtDice.Views
{
    /// <summary>
    /// Control in charge of changing the interface language and culture at runtime.
    /// </summary>
    public partial class LanguageSwitcher : UserControl
    {
        private const string SpanishCultureCode = "es-MX";
        private const string EnglishCultureCode = "en-US";

        /// <summary>
        /// Function that the containing window assigns to indicate how to reopen itself
        /// (with its own constructor parameters, for example the player name).
        /// If it is not assigned, Activator.CreateInstance is used as a fallback (it only works
        /// with windows whose constructor has no parameters, such as LoginWindow).
        /// </summary>
        public Func<Window> ReopenWindowFunc { get; set; }

        /// <summary>
        /// Initializes a new instance of the floating language switch control.
        /// </summary>
        public LanguageSwitcher()
        {
            InitializeComponent();
            RefreshCurrentFlagDisplay();
        }

        private void LanguageToggleButton_Click(object sender, RoutedEventArgs e)
        {
            LanguagePopup.IsOpen = LanguageToggleButton.IsChecked == true;
        }

        private void SpanishOptionButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyLanguage(SpanishCultureCode);
        }

        private void EnglishOptionButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyLanguage(EnglishCultureCode);
        }

        /// <summary>
        /// Applies the chosen culture and reopens the current window to refresh the texts.
        /// </summary>
        /// <param name="cultureCode">Culture code, for example "es-MX" or "en-US".</param>
        private void ApplyLanguage(string cultureCode)
        {
            LanguagePopup.IsOpen = false;
            LanguageToggleButton.IsChecked = false;

            if (App.CurrentCultureCode == cultureCode)
            {
                return;
            }

            App.SetCulture(cultureCode);
            ReopenParentWindow();
        }

        /// <summary>
        /// Closes the window that contains this control and opens a new instance
        /// of the same type, already with the language applied (same pattern used in Settings).
        /// </summary>
        private void ReopenParentWindow()
        {
            Window currentWindow = Window.GetWindow(this);
            if (currentWindow == null)
            {
                return;
            }

            Window newWindow = ReopenWindowFunc != null
                ? ReopenWindowFunc.Invoke()
                : (Window)Activator.CreateInstance(currentWindow.GetType());

            newWindow.Show();
            currentWindow.Close();
        }

        private void RefreshCurrentFlagDisplay()
        {
            bool isSpanish = App.CurrentCultureCode == SpanishCultureCode;
            CurrentLangText.Text = isSpanish ? "ES" : "EN";
            CurrentFlagGrid.Children.Clear();
            CurrentFlagGrid.Children.Add(isSpanish ? BuildSpanishFlag() : BuildEnglishFlag());
        }

        private static Grid BuildSpanishFlag()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());

            var topStripe = new Rectangle { Fill = new SolidColorBrush(Color.FromRgb(0xAA, 0x15, 0x1B)) };
            var midStripe = new Rectangle { Fill = new SolidColorBrush(Color.FromRgb(0xF1, 0xBF, 0x00)) };
            var bottomStripe = new Rectangle { Fill = new SolidColorBrush(Color.FromRgb(0xAA, 0x15, 0x1B)) };

            Grid.SetRow(topStripe, 0);
            Grid.SetRow(midStripe, 1);
            Grid.SetRow(bottomStripe, 2);

            grid.Children.Add(topStripe);
            grid.Children.Add(midStripe);
            grid.Children.Add(bottomStripe);
            return grid;
        }

        private static Grid BuildEnglishFlag()
        {
            var grid = new Grid();
            for (int i = 0; i < 7; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
            }

            var stripeRed = new SolidColorBrush(Color.FromRgb(0xB2, 0x22, 0x34));
            var stripeWhite = new SolidColorBrush(Colors.White);

            for (int row = 0; row < 7; row++)
            {
                var stripe = new Rectangle { Fill = row % 2 == 0 ? stripeRed : stripeWhite };
                Grid.SetRow(stripe, row);
                grid.Children.Add(stripe);
            }

            var canton = new Rectangle
            {
                Width = 9,
                HorizontalAlignment = HorizontalAlignment.Left,
                Fill = new SolidColorBrush(Color.FromRgb(0x3C, 0x3B, 0x6E))
            };
            Grid.SetRowSpan(canton, 4);
            grid.Children.Add(canton);

            return grid;
        }
    }
}