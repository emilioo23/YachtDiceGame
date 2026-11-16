using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;

namespace YachtDice.Server.Tests.InMemory
{
    /// <summary>
    /// Keeps the invitations in memory so the tests do not need the database. The names come from the players of
    /// the in-memory repository it receives.
    /// </summary>
    internal sealed class InMemoryRoomInviteRepository : IRoomInviteRepository
    {
        private readonly InMemoryPlayerRepository _players;
        private readonly List<RoomInvite> _invites = new List<RoomInvite>();

        /// <summary>
        /// Initializes the repository with the players whose names are shown.
        /// </summary>
        /// <param name="players">In-memory players of the test.</param>
        public InMemoryRoomInviteRepository(InMemoryPlayerRepository players)
        {
            _players = players;
        }

        /// <summary>
        /// Gets the invitations saved so far.
        /// </summary>
        public IReadOnlyList<RoomInvite> Invites => _invites;

        /// <inheritdoc />
        public void Add(RoomInvite invite)
        {
            _invites.Add(invite);
        }

        /// <inheritdoc />
        public List<RoomInviteRecord> FindReceived(int invitedId, DateTime since, int maxCount)
        {
            Func<RoomInvite, bool> isRecentForPlayer = invite =>
                invite.InvitedId == invitedId && invite.SendDate >= since;

            return _invites
                .Where(isRecentForPlayer)
                .OrderByDescending(invite => invite.SendDate)
                .Take(maxCount)
                .Select(ToRecord)
                .ToList();
        }

        private RoomInviteRecord ToRecord(RoomInvite invite)
        {
            Func<Player, bool> isInviter = player => player.Id == invite.InviterId;

            return new RoomInviteRecord
            {
                InviterName = _players.Players.First(isInviter).DisplayName,
                RoomCode = invite.RoomCode,
                SendDate = invite.SendDate ?? DateTime.MinValue
            };
        }
    }
}
