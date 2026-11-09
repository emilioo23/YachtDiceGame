using System.Windows.Media;
using YachtDice.Client.Logic.Dice;

namespace YachtDice.Utils
{
    /// <summary>
    /// Gives the drawing of each face of a die.
    /// </summary>
    public static class DiceImages
    {
        private const string DrawingsSource = "Styles/Dice.xaml";

        private static readonly DrawingCatalog Catalog = new DrawingCatalog(DrawingsSource);

        /// <summary>
        /// Gets the drawing of a face.
        /// </summary>
        /// <param name="value">Number of the face, from 1 to 6.</param>
        /// <returns>The drawing; the face of the one if there is no drawing for the number.</returns>
        public static ImageSource Get(int value)
        {
            return Catalog.Get(DieResourceKey.Create(value), DieResourceKey.Create(DieResourceKey.DefaultValue));
        }
    }
}
