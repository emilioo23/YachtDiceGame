using System;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// An invitation to a room saved in the database, with the name of the player who sent it.
    /// </summary>
    public class RoomInviteRecord
    {
        /// <summary>
        /// Gets or sets the name of the player who sent the invitation.
        /// </summary>
        public string InviterName { get; set; }

        /// <summary>
        /// Gets or sets the code of the room of the invitation.
        /// </summary>
        public string RoomCode { get; set; }

        /// <summary>
        /// Gets or sets the moment when the invitation was sent, in universal time.
        /// </summary>
        public DateTime SendDate { get; set; }
    }
}
