using System;
using System.Text.RegularExpressions;

namespace YachtDice.Client.Logic.Chat
{
    /// <summary>
    /// Tells whether a chat message has a link. The chat does not allow links, so the window warns the player
    /// instead of sending the message.
    /// </summary>
    public static class ChatLinkDetector
    {
        private const string LinkPattern =
            @"(https?://|www\.)\S+"
            + @"|\b[a-z0-9-]+(\.[a-z0-9-]+)*\.(com|net|org|edu|gov|mx|io|gg|xyz|me|ly|tv|co|info|es|us|app|dev|link)\b";

        private const int RegexTimeoutMilliseconds = 100;

        private static readonly Regex Links = new Regex(
            LinkPattern,
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(RegexTimeoutMilliseconds));

        /// <summary>
        /// Tells whether a message has a link.
        /// </summary>
        /// <param name="message">Message that the player wrote.</param>
        /// <returns>True if the message has a link; false if it is empty or has none.</returns>
        public static bool ContainsLink(string message)
        {
            bool hasLink = false;

            if (!string.IsNullOrEmpty(message))
            {
                hasLink = Links.IsMatch(message);
            }

            return hasLink;
        }
    }
}
