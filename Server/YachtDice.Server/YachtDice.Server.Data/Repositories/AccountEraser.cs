using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Removes everything that belongs to a player. The database does not delete in cascade, so every table
    /// that points to the player is cleaned by hand, inside the same context, so it all goes in one transaction.
    /// The games that other players also played are kept: a room that the player hosted goes to another
    /// participant, and a room without more participants is removed with its history.
    /// </summary>
    internal sealed class AccountEraser
    {
        /// <summary>
        /// Marks for removal the player and all the data that depends on the player. The caller saves the context.
        /// </summary>
        /// <param name="context">Context in which the removal is prepared.</param>
        /// <param name="playerId">Identifier of the player to remove.</param>
        public void Erase(YachtDiceContext context, int playerId)
        {
            Player player = context.Player.Find(playerId);

            if (player != null)
            {
                RemoveGameData(context, playerId);
                RemoveSecurityData(context, playerId);
                RemoveSocialData(context, playerId);
                PassHostedRooms(context, playerId);
                context.Player.Remove(player);
            }
        }

        private static void RemoveWhere<TEntity>(DbSet<TEntity> table, Expression<Func<TEntity, bool>> condition)
            where TEntity : class
        {
            List<TEntity> rows = table.Where(condition).ToList();

            table.RemoveRange(rows);
        }

        private static void RemoveGameData(YachtDiceContext context, int playerId)
        {
            List<int> scorecardIds = context.Scorecard
                .Where(card => card.PlayerId == playerId)
                .Select(card => card.Id)
                .ToList();

            RemoveWhere(context.ScorecardEntry, entry => scorecardIds.Contains(entry.ScorecardId));
            RemoveWhere(context.Scorecard, card => card.PlayerId == playerId);
            RemoveWhere(context.TurnState, turn => turn.PlayerId == playerId);
            RemoveWhere(context.GameHistoryPlayer, entry => entry.PlayerId == playerId);
            RemoveWhere(context.RoomPlayer, entry => entry.PlayerId == playerId);
        }

        private static void RemoveSecurityData(YachtDiceContext context, int playerId)
        {
            RemoveWhere(context.BackupCode, code => code.PlayerId == playerId);
            RemoveWhere(context.Passkey, key => key.PlayerId == playerId);
            RemoveWhere(context.PwdResetToken, token => token.PlayerId == playerId);
            RemoveWhere(context.TwoFactorAuth, method => method.PlayerId == playerId);
            RemoveWhere(context.PlayerSettings, settings => settings.PlayerId == playerId);
        }

        private static void RemoveSocialData(YachtDiceContext context, int playerId)
        {
            RemoveWhere(
                context.Friendship,
                friendship => friendship.RequesterId == playerId || friendship.AddresseeId == playerId);
            RemoveWhere(context.KickLog, kick => kick.HostId == playerId || kick.TargetId == playerId);
            RemoveWhere(
                context.RoomInvite,
                invite => invite.InviterId == playerId || invite.InvitedId == playerId);
            RemoveWhere(
                context.PlayerReport,
                report => report.ReporterId == playerId || report.ReportedId == playerId);
        }

        private static void PassHostedRooms(YachtDiceContext context, int playerId)
        {
            List<Room> hostedRooms = context.Room.Where(room => room.HostPlayerId == playerId).ToList();

            foreach (Room room in hostedRooms)
            {
                List<int> otherParticipants = FindOtherParticipants(context, room.Id, playerId);

                if (otherParticipants.Any())
                {
                    room.HostPlayerId = otherParticipants.First();
                }
                else
                {
                    RemoveRoom(context, room);
                }
            }
        }

        private static List<int> FindOtherParticipants(YachtDiceContext context, int roomId, int playerId)
        {
            List<int> participants = context.GameHistoryPlayer
                .Where(entry => entry.GameHistory.RoomId == roomId && entry.PlayerId != playerId)
                .Select(entry => entry.PlayerId)
                .ToList();

            return participants;
        }

        private static void RemoveRoom(YachtDiceContext context, Room room)
        {
            int roomId = room.Id;
            List<int> gameIds = context.GameHistory
                .Where(game => game.RoomId == roomId)
                .Select(game => game.Id)
                .ToList();
            List<int> scorecardIds = context.Scorecard
                .Where(card => gameIds.Contains(card.GameId))
                .Select(card => card.Id)
                .ToList();

            RemoveWhere(context.ScorecardEntry, entry => scorecardIds.Contains(entry.ScorecardId));
            RemoveWhere(context.Scorecard, card => gameIds.Contains(card.GameId));
            RemoveWhere(context.TurnState, turn => gameIds.Contains(turn.GameId));
            RemoveWhere(context.GameHistoryPlayer, entry => gameIds.Contains(entry.GameId));
            RemoveWhere(
                context.PlayerReport,
                report => report.RoomId == roomId || (report.GameId != null && gameIds.Contains(report.GameId.Value)));
            RemoveWhere(context.GameHistory, game => game.RoomId == roomId);
            RemoveWhere(context.KickLog, kick => kick.RoomId == roomId);
            RemoveWhere(context.RoomInvite, invite => invite.RoomId == roomId);
            RemoveWhere(context.RoomPlayer, entry => entry.RoomId == roomId);
            context.Room.Remove(room);
        }
    }
}
