using System;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// An expulsion saved in the database, with the names of the two players.
    /// </summary>
    public class KickLogRecord
    {
        /// <summary>
        /// Gets or sets the name of the host who expelled the player.
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// Gets or sets the name of the player who was expelled.
        /// </summary>
        public string TargetName { get; set; }

        /// <summary>
        /// Gets or sets the code of the room where it happened.
        /// </summary>
        public string RoomCode { get; set; }

        /// <summary>
        /// Gets or sets the moment of the expulsion, in universal time.
        /// </summary>
        public DateTime Date { get; set; }
    }
}
