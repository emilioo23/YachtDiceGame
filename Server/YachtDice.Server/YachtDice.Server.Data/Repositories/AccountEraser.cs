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
            Expression<Func<Scorecard, bool>> isPlayerCard = card => card.PlayerId == playerId;
            List<int> scorecardIds = context.Scorecard
                .Where(isPlayerCard)
                .Select(card => card.Id)
                .ToList();
            Expression<Func<ScorecardEntry, bool>> isEntryOfCard = entry => scorecardIds.Contains(entry.ScorecardId);
            Expression<Func<TurnState, bool>> isPlayerTurn = turn => turn.PlayerId == playerId;
            Expression<Func<GameHistoryPlayer, bool>> isPlayerInHistory = entry => entry.PlayerId == playerId;
            Expression<Func<RoomPlayer, bool>> isPlayerInRoom = entry => entry.PlayerId == playerId;

            RemoveWhere(context.ScorecardEntry, isEntryOfCard);
            RemoveWhere(context.Scorecard, isPlayerCard);
            RemoveWhere(context.TurnState, isPlayerTurn);
            RemoveWhere(context.GameHistoryPlayer, isPlayerInHistory);
            RemoveWhere(context.RoomPlayer, isPlayerInRoom);
        }

        private static void RemoveSecurityData(YachtDiceContext context, int playerId)
        {
            Expression<Func<BackupCode, bool>> isPlayerCode = code => code.PlayerId == playerId;
            Expression<Func<Passkey, bool>> isPlayerKey = key => key.PlayerId == playerId;
            Expression<Func<PwdResetToken, bool>> isPlayerToken = token => token.PlayerId == playerId;
            Expression<Func<TwoFactorAuth, bool>> isPlayerMethod = method => method.PlayerId == playerId;
            Expression<Func<PlayerSettings, bool>> isPlayerSettings = settings => settings.PlayerId == playerId;
            Expression<Func<PasswordHistory, bool>> isPlayerPassword = password => password.PlayerId == playerId;

            RemoveWhere(context.BackupCode, isPlayerCode);
            RemoveWhere(context.Passkey, isPlayerKey);
            RemoveWhere(context.PwdResetToken, isPlayerToken);
            RemoveWhere(context.TwoFactorAuth, isPlayerMethod);
            RemoveWhere(context.PlayerSettings, isPlayerSettings);
            RemoveWhere(context.PasswordHistory, isPlayerPassword);
        }

        private static void RemoveSocialData(YachtDiceContext context, int playerId)
        {
            Expression<Func<Friendship, bool>> hasPlayerInFriendship =
                friendship => friendship.RequesterId == playerId || friendship.AddresseeId == playerId;
            Expression<Func<KickLog, bool>> hasPlayerInKick =
                kick => kick.HostId == playerId || kick.TargetId == playerId;
            Expression<Func<RoomInvite, bool>> hasPlayerInInvite =
                invite => invite.InviterId == playerId || invite.InvitedId == playerId;
            Expression<Func<PlayerReport, bool>> hasPlayerInReport =
                report => report.ReporterId == playerId || report.ReportedId == playerId;

            RemoveWhere(context.Friendship, hasPlayerInFriendship);
            RemoveWhere(context.KickLog, hasPlayerInKick);
            RemoveWhere(context.RoomInvite, hasPlayerInInvite);
            RemoveWhere(context.PlayerReport, hasPlayerInReport);
        }

        private static void PassHostedRooms(YachtDiceContext context, int playerId)
        {
            Expression<Func<Room, bool>> isHostedByPlayer = room => room.HostPlayerId == playerId;
            List<Room> hostedRooms = context.Room.Where(isHostedByPlayer).ToList();

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
            Expression<Func<GameHistoryPlayer, bool>> isOtherParticipant =
                entry => entry.GameHistory.RoomId == roomId && entry.PlayerId != playerId;
            List<int> participants = context.GameHistoryPlayer
                .Where(isOtherParticipant)
                .Select(entry => entry.PlayerId)
                .ToList();

            return participants;
        }

        private static void RemoveRoom(YachtDiceContext context, Room room)
        {
            int roomId = room.Id;
            Expression<Func<GameHistory, bool>> isGameOfRoom = game => game.RoomId == roomId;
            List<int> gameIds = context.GameHistory
                .Where(isGameOfRoom)
                .Select(game => game.Id)
                .ToList();
            Expression<Func<Scorecard, bool>> isCardOfGame = card => gameIds.Contains(card.GameId);
            List<int> scorecardIds = context.Scorecard
                .Where(isCardOfGame)
                .Select(card => card.Id)
                .ToList();

            RemoveRoomGames(context, gameIds, scorecardIds);
            RemoveRoomRecords(context, roomId, gameIds);
            RemoveWhere(context.GameHistory, isGameOfRoom);
            context.Room.Remove(room);
        }

        private static void RemoveRoomGames(YachtDiceContext context, List<int> gameIds, List<int> scorecardIds)
        {
            Expression<Func<ScorecardEntry, bool>> isEntryOfCard = entry => scorecardIds.Contains(entry.ScorecardId);
            Expression<Func<Scorecard, bool>> isCardOfGame = card => gameIds.Contains(card.GameId);
            Expression<Func<TurnState, bool>> isTurnOfGame = turn => gameIds.Contains(turn.GameId);
            Expression<Func<GameHistoryPlayer, bool>> isPlayerOfGame = entry => gameIds.Contains(entry.GameId);

            RemoveWhere(context.ScorecardEntry, isEntryOfCard);
            RemoveWhere(context.Scorecard, isCardOfGame);
            RemoveWhere(context.TurnState, isTurnOfGame);
            RemoveWhere(context.GameHistoryPlayer, isPlayerOfGame);
        }

        private static void RemoveRoomRecords(YachtDiceContext context, int roomId, List<int> gameIds)
        {
            Expression<Func<PlayerReport, bool>> isReportOfRoom = report =>
                report.RoomId == roomId || (report.GameId != null && gameIds.Contains(report.GameId.Value));
            Expression<Func<KickLog, bool>> isKickOfRoom = kick => kick.RoomId == roomId;
            Expression<Func<RoomInvite, bool>> isInviteOfRoom = invite => invite.RoomId == roomId;
            Expression<Func<RoomPlayer, bool>> isPlayerOfRoom = entry => entry.RoomId == roomId;

            RemoveWhere(context.PlayerReport, isReportOfRoom);
            RemoveWhere(context.KickLog, isKickOfRoom);
            RemoveWhere(context.RoomInvite, isInviteOfRoom);
            RemoveWhere(context.RoomPlayer, isPlayerOfRoom);
        }
    }
}
