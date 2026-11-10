namespace YachtDice.Models
{
    /// <summary>
    /// A line that the chat panel shows: who wrote it, what it says and which avatar goes next to the name.
    /// </summary>
    public class ChatEntry
    {
        /// <summary>
        /// Avatar value of the notices of the game, which have no avatar.
        /// </summary>
        public const int NoAvatar = 0;

        /// <summary>
        /// Gets or sets the name of the player who wrote the message, or of the game for a notice.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the text of the message.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets who wrote the message.
        /// </summary>
        public ChatMessageKind Kind { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the avatar of the author, or <see cref="NoAvatar"/>.
        /// </summary>
        public int AvatarId { get; set; }
    }
}
