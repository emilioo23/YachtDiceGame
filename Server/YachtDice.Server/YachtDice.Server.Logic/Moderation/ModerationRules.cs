namespace YachtDice.Server.Logic.Moderation
{
    /// <summary>
    /// Values that the filters of the chat share, so the hidden words and the hidden links look the same.
    /// </summary>
    public static class ModerationRules
    {
        /// <summary>
        /// Character that replaces each letter of a hidden word or link.
        /// </summary>
        public const char CensorCharacter = '*';

        /// <summary>
        /// Most milliseconds that a search with a regular expression can take before it is stopped.
        /// </summary>
        public const int RegexTimeoutMilliseconds = 100;
    }
}
