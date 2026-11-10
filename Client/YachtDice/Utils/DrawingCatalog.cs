using System;
using System.Windows;
using System.Windows.Media;

namespace YachtDice.Utils
{
    /// <summary>
    /// A set of drawings kept in their own resource dictionary, so they do not depend on the theme that is
    /// active. It is shared by the avatars and the dice.
    /// </summary>
    internal sealed class DrawingCatalog
    {
        private readonly ResourceDictionary _drawings;

        /// <summary>
        /// Loads the drawings of a resource dictionary.
        /// </summary>
        /// <param name="source">Relative path of the resource dictionary.</param>
        public DrawingCatalog(string source)
        {
            _drawings = new ResourceDictionary { Source = new Uri(source, UriKind.Relative) };
        }

        /// <summary>
        /// Gets a drawing by its name.
        /// </summary>
        /// <param name="key">Name of the drawing.</param>
        /// <param name="fallbackKey">Name of the drawing to use when the first one does not exist.</param>
        /// <returns>The drawing.</returns>
        public ImageSource Get(string key, string fallbackKey)
        {
            string foundKey = _drawings.Contains(key) ? key : fallbackKey;

            return (ImageSource)_drawings[foundKey];
        }
    }
}
