using System.Globalization;

namespace YachtDice.Client.Logic.Dice
{
    /// <summary>
    /// Builds the name under which the drawing of each face of a die is saved in the resources.
    /// </summary>
    public static class DieResourceKey
    {
        /// <summary>
        /// Number of the face that is shown when the drawing of another one is missing.
        /// </summary>
        public const int DefaultValue = 1;

        private const string KeyFormat = "Die_{0}";

        /// <summary>
        /// Gets the resource name of the drawing of a face.
        /// </summary>
        /// <param name="value">Number of the face, from 1 to 6.</param>
        /// <returns>The resource name, for example Die_4.</returns>
        public static string Create(int value)
        {
            return string.Format(CultureInfo.InvariantCulture, KeyFormat, value);
        }
    }
}
