using System.Windows;
using System.Windows.Controls;
using YachtDice.Client.Logic.Dice;
using YachtDice.Utils;

namespace YachtDice.Views
{
    /// <summary>
    /// Shows a row of dice that are described with a short text, for example "3 3 3 5- 6-". The "How to play"
    /// window uses it to draw the example of each category.
    /// </summary>
    public class DiceRow : StackPanel
    {
        private const int DieSize = 40;
        private const int DieGap = 3;
        private const double DimmedOpacity = 0.3;

        /// <summary>
        /// Property that holds the text that describes the dice of the row.
        /// </summary>
        public static readonly DependencyProperty DiceProperty = DependencyProperty.Register(
            nameof(Dice),
            typeof(string),
            typeof(DiceRow),
            new PropertyMetadata(string.Empty, OnDiceChanged));

        /// <summary>
        /// Initializes an empty row of dice.
        /// </summary>
        public DiceRow()
        {
            Orientation = Orientation.Horizontal;
        }

        /// <summary>
        /// Gets or sets the text that describes the dice: the numbers separated by spaces, with a minus sign
        /// after the ones that must be shown faded.
        /// </summary>
        public string Dice
        {
            get { return (string)GetValue(DiceProperty); }
            set { SetValue(DiceProperty, value); }
        }

        private static void OnDiceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((DiceRow)sender).ShowDice((string)e.NewValue);
        }

        private static Image CreateDieImage(DieDisplay die)
        {
            var image = new Image
            {
                Source = DiceImages.Get(die.Value),
                Width = DieSize,
                Height = DieSize,
                Margin = new Thickness(DieGap)
            };

            if (die.IsDimmed)
            {
                image.Opacity = DimmedOpacity;
            }

            return image;
        }

        private void ShowDice(string text)
        {
            Children.Clear();

            foreach (DieDisplay die in DiceRowParser.Parse(text))
            {
                Children.Add(CreateDieImage(die));
            }
        }
    }
}
