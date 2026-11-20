using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YachtDice.Utils
{
    /// <summary>
    /// Loads the image of the flag of a country from the resources of the application.
    /// </summary>
    public static class FlagImages
    {
        private const string FlagUriFormat = "pack://application:,,,/Resources/Flags/{0}.png";

        /// <summary>
        /// Gets the image of a flag. The code must belong to the list of countries.
        /// </summary>
        /// <param name="code">Code of the country with two lowercase letters.</param>
        /// <returns>The image of the flag.</returns>
        public static ImageSource Get(string code)
        {
            return new BitmapImage(new Uri(string.Format(FlagUriFormat, code)));
        }
    }
}
