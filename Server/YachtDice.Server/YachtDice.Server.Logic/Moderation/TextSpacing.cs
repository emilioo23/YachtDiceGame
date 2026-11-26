using System;
using System.Text.RegularExpressions;
using YachtDice.Server.Data.Logging;

namespace YachtDice.Server.Logic.Moderation
{
    /// <summary>
    /// Cleans the spaces of the texts that the players write, so a message or a name cannot be filled with spaces.
    /// </summary>
    public static class TextSpacing
    {
        private const string SpacesPattern = @"\s+";
        private const string SingleSpace = " ";
        private const int SpacesRegexTimeoutMilliseconds = 100;

        private static readonly Regex Spaces = new Regex(
            SpacesPattern,
            RegexOptions.CultureInvariant | RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(SpacesRegexTimeoutMilliseconds));

        /// <summary>
        /// Removes the spaces at the start and at the end of the text and leaves a single space where the text
        /// has several spaces, tabs or line breaks together.
        /// </summary>
        /// <param name="text">Text written by the player.</param>
        /// <returns>The text with single spaces between its words.</returns>
        /// <exception cref="ArgumentNullException">The text is null.</exception>
        public static string Collapse(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text), LogMessages.TextSpacingNullTextMessage);
            }

            return Spaces.Replace(text.Trim(), SingleSpace);
        }
    }
}
