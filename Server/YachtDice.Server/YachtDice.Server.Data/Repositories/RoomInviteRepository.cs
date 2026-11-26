using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using YachtDice.Server.Data.Exceptions;
using YachtDice.Server.Data.Logging;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Entity Framework implementation of the data access of the invitations to a room.
    /// </summary>
    public class RoomInviteRepository : IRoomInviteRepository
    {
        /// <summary>
        /// Saves a new invitation.
        /// </summary>
        /// <param name="invite">Invitation with the two players, the room code and the date.</param>
        /// <exception cref="ArgumentNullException">The invitation is null.</exception>
        /// <exception cref="DataAccessException">The invitation could not be saved.</exception>
        public void Add(RoomInvite invite)
        {
            if (invite == null)
            {
                throw new ArgumentNullException(nameof(invite), LogMessages.NullInviteMessage);
            }

            DatabaseOperation.Save(context => context.RoomInvite.Add(invite),
                LogMessages.RoomInviteUpdateFailedMessage);
        }

        /// <summary>
        /// Finds the latest invitations that a player received after a moment.
        /// </summary>
        /// <param name="invitedId">Identifier of the player who received them.</param>
        /// <param name="since">Only the invitations sent after this moment count.</param>
        /// <param name="maxCount">Most invitations to return.</param>
        /// <returns>The invitations from the newest to the oldest, or an empty list if there is none.</returns>
        /// <exception cref="DataAccessException">The invitations could not be read.</exception>
        public List<RoomInviteRecord> FindReceived(int invitedId, DateTime since, int maxCount)
        {
            Expression<Func<RoomInvite, bool>> isRecentForPlayer = invite =>
                invite.InvitedId == invitedId && invite.SendDate >= since;
            List<RoomInviteRecord> invites = DatabaseOperation.Query(
                context => SelectNewest(context, isRecentForPlayer, maxCount),
                LogMessages.RoomInviteQueryFailedMessage);

            return invites;
        }

        private static List<RoomInviteRecord> SelectNewest(
            YachtDiceContext context,
            Expression<Func<RoomInvite, bool>> isWantedInvite,
            int maxCount)
        {
            List<RoomInviteRecord> invites = context.RoomInvite
                .Where(isWantedInvite)
                .OrderByDescending(invite => invite.SendDate)
                .Take(maxCount)
                .Select(invite => new RoomInviteRecord
                {
                    InviterName = invite.Player.DisplayName,
                    RoomCode = invite.RoomCode,
                    SendDate = invite.SendDate ?? DateTime.MinValue
                })
                .ToList();

            return invites;
        }
    }
}
