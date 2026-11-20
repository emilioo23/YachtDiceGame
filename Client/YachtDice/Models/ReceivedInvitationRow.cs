namespace YachtDice.Models
{
    /// <summary>
    /// A row of the list of invitations that the player received.
    /// </summary>
    public class ReceivedInvitationRow
    {
        /// <summary>
        /// Gets or sets the name of the friend who sent the invitation.
        /// </summary>
        public string SenderName { get; set; }

        /// <summary>
        /// Gets or sets the initial of the name of the friend, used for the avatar.
        /// </summary>
        public string Initial { get; set; }

        /// <summary>
        /// Gets or sets the sentence with the room and the hour of the invitation.
        /// </summary>
        public string DetailText { get; set; }

        /// <summary>
        /// Gets or sets the code of the room.
        /// </summary>
        public string RoomCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the room still exists, so the player can join it.
        /// </summary>
        public bool CanJoin { get; set; }
    }
}
