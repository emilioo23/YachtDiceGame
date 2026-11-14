using System;
using System.Text.RegularExpressions;

namespace YachtDice.Server.Logic.Moderation
{
    /// <summary>
    /// Hides the links of a chat message, so no player can send other players to an outside page. The client
    /// already stops the messages with links; this check covers a client that was changed to skip it.
    /// </summary>
    public static class LinkCensor
    {
        private const char CensorCharacter = '*';
        private const string LinkPattern =
            @"(https?://|www\.)\S+"
            + @"|\b[a-z0-9-]+(\.[a-z0-9-]+)*\.(com|net|org|edu|gov|mx|io|gg|xyz|me|ly|tv|co|info|es|us|app|dev|link)\b"
            + @"(/\S*)?";

        private const int RegexTimeoutMilliseconds = 100;
        private const string NullMessageMessage = "The message to censor cannot be null.";

        private static readonly Regex Links = new Regex(
            LinkPattern,
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(RegexTimeoutMilliseconds));

        /// <summary>
        /// Replaces each link of a message with as many asterisks as characters it has.
        /// </summary>
        /// <param name="message">Message to censor.</param>
        /// <returns>The message without links.</returns>
        /// <exception cref="ArgumentNullException">The message is null.</exception>
        public static string Censor(string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message), NullMessageMessage);
            }

            return Links.Replace(message, match => new string(CensorCharacter, match.Length));
        }
    }
}
