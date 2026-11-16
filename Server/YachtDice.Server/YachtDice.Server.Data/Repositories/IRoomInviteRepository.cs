using System;
using System.Collections.Generic;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Data access operations for the invitations to a room that a player receives.
    /// </summary>
    public interface IRoomInviteRepository
    {
        /// <summary>
        /// Saves a new invitation.
        /// </summary>
        /// <param name="invite">Invitation with the two players, the room code and the date.</param>
        void Add(RoomInvite invite);

        /// <summary>
        /// Finds the latest invitations that a player received after a moment.
        /// </summary>
        /// <param name="invitedId">Identifier of the player who received them.</param>
        /// <param name="since">Only the invitations sent after this moment count.</param>
        /// <param name="maxCount">Most invitations to return.</param>
        /// <returns>The invitations from the newest to the oldest, or an empty list if there is none.</returns>
        List<RoomInviteRecord> FindReceived(int invitedId, DateTime since, int maxCount);
    }
}
