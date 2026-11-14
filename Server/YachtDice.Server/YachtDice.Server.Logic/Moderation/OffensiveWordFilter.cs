using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace YachtDice.Server.Logic.Moderation
{
    /// <summary>
    /// Finds the offensive words of a list, in Spanish and in English, inside the names and the chat messages.
    /// The list comes from a file of the server, so it can grow without compiling the server again.
    /// </summary>
    public sealed class OffensiveWordFilter
    {
        private const char CensorCharacter = '*';
        private const char PartSeparator = ' ';
        private const int ShortWordLength = 4;
        private const string WordSeparator = "|";
        private const string WholeWordPatternFormat = @"\b({0})\b";
        private const string NeverMatchingPattern = "(?!)";
        private const int RegexTimeoutMilliseconds = 100;
        private const string NullWordsMessage = "The list of offensive words cannot be null.";
        private const string NullTextMessage = "The text to check cannot be null.";

        private static readonly Dictionary<char, char> LookAlikeLetters = new Dictionary<char, char>
        {
            { '0', 'o' },
            { '1', 'i' },
            { '3', 'e' },
            { '4', 'a' },
            { '5', 's' },
            { '7', 't' },
            { '@', 'a' },
            { '$', 's' }
        };

        private readonly List<string> _words;
        private readonly Regex _wholeWords;

        /// <summary>
        /// Initializes the filter with the offensive words. Blank lines are ignored, and upper and lower case
        /// are treated the same.
        /// </summary>
        /// <param name="words">Offensive words, one per item.</param>
        /// <exception cref="ArgumentNullException">The list is null.</exception>
        public OffensiveWordFilter(IEnumerable<string> words)
        {
            if (words == null)
            {
                throw new ArgumentNullException(nameof(words), NullWordsMessage);
            }

            Func<string, bool> hasText = word => !string.IsNullOrWhiteSpace(word);

            _words = words
                .Where(hasText)
                .Select(word => word.Trim().ToLowerInvariant())
                .Distinct()
                .ToList();
            _wholeWords = new Regex(
                CreateWholeWordPattern(_words),
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
                TimeSpan.FromMilliseconds(RegexTimeoutMilliseconds));
        }

        /// <summary>
        /// Tells whether a username hides an offensive word. A long word is looked for anywhere inside the
        /// username, ignoring dots, underscores and the numbers that look like letters. A word of four letters
        /// or less must be a whole part of the name, so names like "computadora" are not rejected.
        /// </summary>
        /// <param name="username">Username to check.</param>
        /// <returns>True if the username contains an offensive word.</returns>
        /// <exception cref="ArgumentNullException">The username is null.</exception>
        public bool ContainsOffensiveWord(string username)
        {
            if (username == null)
            {
                throw new ArgumentNullException(nameof(username), NullTextMessage);
            }

            string plainUsername = ToPlainLetters(username);
            HashSet<string> nameParts = SplitIntoLetterParts(username);

            return _words.Any(word => IsHiddenIn(word, plainUsername, nameParts));
        }

        /// <summary>
        /// Replaces each offensive word of a message with as many asterisks as letters it has.
        /// </summary>
        /// <param name="message">Message to censor.</param>
        /// <returns>The message with the offensive words hidden.</returns>
        /// <exception cref="ArgumentNullException">The message is null.</exception>
        public string Censor(string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message), NullTextMessage);
            }

            return _wholeWords.Replace(message, match => new string(CensorCharacter, match.Length));
        }

        private static string CreateWholeWordPattern(List<string> words)
        {
            string pattern = NeverMatchingPattern;

            if (words.Count > 0)
            {
                string alternatives = string.Join(WordSeparator, words.Select(Regex.Escape));
                pattern = string.Format(CultureInfo.InvariantCulture, WholeWordPatternFormat, alternatives);
            }

            return pattern;
        }

        private static bool IsHiddenIn(string word, string plainUsername, ISet<string> nameParts)
        {
            bool isHidden;

            if (word.Length > ShortWordLength)
            {
                isHidden = plainUsername.Contains(word);
            }
            else
            {
                isHidden = nameParts.Contains(word);
            }

            return isHidden;
        }

        private static HashSet<string> SplitIntoLetterParts(string username)
        {
            var spacedLetters = new StringBuilder();

            foreach (char character in username.ToLowerInvariant())
            {
                char letterOrSpace = char.IsLetter(character) ? character : PartSeparator;

                spacedLetters.Append(letterOrSpace);
            }

            string[] parts = spacedLetters.ToString()
                .Split(new[] { PartSeparator }, StringSplitOptions.RemoveEmptyEntries);

            return new HashSet<string>(parts);
        }

        private static string ToPlainLetters(string username)
        {
            var plainLetters = new StringBuilder();

            foreach (char character in username.ToLowerInvariant())
            {
                char letter = LookAlikeLetters.TryGetValue(character, out char lookAlike) ? lookAlike : character;

                if (char.IsLetter(letter))
                {
                    plainLetters.Append(letter);
                }
            }

            return plainLetters.ToString();
        }
    }
}
