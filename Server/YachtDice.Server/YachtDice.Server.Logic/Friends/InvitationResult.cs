namespace YachtDice.Server.Logic.Friends
{
    /// <summary>
    /// Result of checking whether a player can invite a friend to a room.
    /// </summary>
    public class InvitationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvitationResult"/> class.
        /// </summary>
        /// <param name="status">Outcome of the check.</param>
        /// <param name="senderName">Name that the room shows for the player who invites.</param>
        public InvitationResult(InvitationStatus status, string senderName)
        {
            Status = status;
            SenderName = senderName;
        }

        /// <summary>
        /// Gets the outcome of the check.
        /// </summary>
        public InvitationStatus Status { get; }

        /// <summary>
        /// Gets the name that the room shows for the player who invites; empty if the player is not inside the room.
        /// </summary>
        public string SenderName { get; }
    }
}
