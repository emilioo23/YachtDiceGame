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
        private const string SpanishLanguageLabel = "ES";
        private const string EnglishLanguageLabel = "EN";
        private const int UsFlagTotalRows = 7;
        private const int UsFlagCantonRowSpan = 4;
        private const double UsFlagCantonWidth = 9.0;
        private const int EvenRowDivisor = 2;
        private const int ZeroRemainder = 0;

        private static readonly Color _spanishRedColor = Color.FromRgb(0xAA, 0x15, 0x1B);
        private static readonly Color _spanishYellowColor = Color.FromRgb(0xF1, 0xBF, 0x00);
        private static readonly Color _usRedColor = Color.FromRgb(0xB2, 0x22, 0x34);
        private static readonly Color _usBlueColor = Color.FromRgb(0x3C, 0x3B, 0x6E);

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
                Window newWindow = ReopenWindowFunc != null
                    ? ReopenWindowFunc.Invoke()
                    : (Window)Activator.CreateInstance(currentWindow.GetType());

                newWindow.Show();
                currentWindow.Close();
            }
        }

        private void RefreshCurrentFlagDisplay()
        {
            bool isSpanish = App.CurrentCultureCode == SpanishCultureCode;
            CurrentLangText.Text = isSpanish ? SpanishLanguageLabel : EnglishLanguageLabel;
            CurrentFlagGrid.Children.Clear();
            CurrentFlagGrid.Children.Add(isSpanish ? BuildSpanishFlag() : BuildEnglishFlag());
        }

        private static Grid BuildSpanishFlag()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());

            var topStripe = new Rectangle { Fill = new SolidColorBrush(_spanishRedColor) };
            var midStripe = new Rectangle { Fill = new SolidColorBrush(_spanishYellowColor) };
            var bottomStripe = new Rectangle { Fill = new SolidColorBrush(_spanishRedColor) };

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
            for (int rowIndex = 0; rowIndex < UsFlagTotalRows; rowIndex++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
            }

            var stripeRed = new SolidColorBrush(_usRedColor);
            var stripeWhite = new SolidColorBrush(Colors.White);

            for (int row = 0; row < UsFlagTotalRows; row++)
            {
                var stripe = new Rectangle { Fill = row % EvenRowDivisor == ZeroRemainder ? stripeRed : stripeWhite };
                Grid.SetRow(stripe, row);
                grid.Children.Add(stripe);
            }

            var canton = new Rectangle
            {
                Width = UsFlagCantonWidth,
                HorizontalAlignment = HorizontalAlignment.Left,
                Fill = new SolidColorBrush(_usBlueColor)
            };
            Grid.SetRowSpan(canton, UsFlagCantonRowSpan);
            grid.Children.Add(canton);

            return grid;
        }
    }
}