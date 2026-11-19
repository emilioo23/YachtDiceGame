using System.Windows.Media;
using YachtDice.Client.Logic.Dice;

namespace YachtDice.Utils
{
    /// <summary>
    /// Gives the image of each face of a die.
    /// </summary>
    public static class DiceImages
    {
        private const string ImagesFolder = "Resources/Dice";

        private static readonly ImageCatalog Catalog = new ImageCatalog(ImagesFolder);

        /// <summary>
        /// Gets the image of a face.
        /// </summary>
        /// <param name="value">Number of the face, from 1 to 6.</param>
        /// <returns>The image; the face of the one if there is no image for the number.</returns>
        public static ImageSource Get(int value)
        {
            return Catalog.Get(DieResourceKey.Create(value), DieResourceKey.Create(DieResourceKey.DefaultValue));
        }
    }
}
