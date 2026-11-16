using System;

namespace YachtDice.Server.Logic.Friends
{
    /// <summary>
    /// An invitation to a room that a player received, as the list of invitations shows it.
    /// </summary>
    public class ReceivedInvitation
    {
        /// <summary>
        /// Gets or sets the name of the friend who sent it.
        /// </summary>
        public string SenderName { get; set; }

        /// <summary>
        /// Gets or sets the code of the room.
        /// </summary>
        public string RoomCode { get; set; }

        /// <summary>
        /// Gets or sets the moment when it was sent, in universal time.
        /// </summary>
        public DateTime SentAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the room still exists, so the player can try to join it.
        /// </summary>
        public bool IsRoomOpen { get; set; }
    }
}
