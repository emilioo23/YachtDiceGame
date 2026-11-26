using System;
using System.Text.RegularExpressions;
using YachtDice.Server.Data.Logging;

namespace YachtDice.Server.Logic.Moderation
{
    /// <summary>
    /// Hides the links of the chat messages and shares the censor values with the filter of words.
    /// </summary>
    public static class ChatCensor
    {
        /// <summary>
        /// Character that replaces each letter of a hidden word or link.
        /// </summary>
        public const char CensorCharacter = '*';

        /// <summary>
        /// Most milliseconds that a search with a regular expression can take before it is stopped.
        /// </summary>
        public const int RegexTimeoutMilliseconds = 100;

        private const string LinkPattern =
            @"(https?://|www\.)\S+"
            + @"|\b[a-z0-9-]+(\.[a-z0-9-]+)*\.(com|net|org|edu|gov|mx|io|gg|xyz|me|ly|tv|co|info|es|us|app|dev|link)\b"
            + @"(/\S*)?";
        private static readonly Regex Links = new Regex(
            LinkPattern,
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(ChatCensor.RegexTimeoutMilliseconds));

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
                throw new ArgumentNullException(nameof(message), LogMessages.ChatCensorNullMessageMessage);
            }

            return Links.Replace(message, match => new string(ChatCensor.CensorCharacter, match.Length));
        }
    }
}
