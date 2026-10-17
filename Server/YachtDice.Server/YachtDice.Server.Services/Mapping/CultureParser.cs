using System.Globalization;

namespace YachtDice.Server.Services.Mapping
{
    /// <summary>
    /// Converts the language names sent by the clients into cultures.
    /// </summary>
    internal static class CultureParser
    {
        /// <summary>
        /// Gets the culture with the given name, or the default one if the name is empty or unknown.
        /// </summary>
        /// <param name="cultureName">Name of the culture, for example "es-MX".</param>
        /// <returns>The matching culture, or the invariant culture.</returns>
        public static CultureInfo Parse(string cultureName)
        {
            CultureInfo culture = CultureInfo.InvariantCulture;

            if (!string.IsNullOrWhiteSpace(cultureName))
            {
                try
                {
                    culture = CultureInfo.GetCultureInfo(cultureName);
                }
                catch (CultureNotFoundException)
                {
                    culture = CultureInfo.InvariantCulture;
                }
            }

            return culture;
        }
    }
}