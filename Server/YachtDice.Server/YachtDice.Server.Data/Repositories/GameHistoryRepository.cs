using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using YachtDice.Server.Data.Exceptions;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Entity Framework implementation of the game history data access.
    /// </summary>
    public class GameHistoryRepository : IGameHistoryRepository
    {
        private const string FindByPlayerFailedMessage =
            "Database error occurred while fetching game history by player ID.";

        private const string RankingFailedMessage = "Database error occurred while calculating player rankings.";
        private const string SaveFailedMessage = "Database error occurred while saving the result of a game.";
        private const string NullResultMessage = "The result of the game cannot be null.";
        private const string ArchivedRoomName = "Partida";
        private const string RoomCodeFormat = "N";
        private const int RoomCodeLength = 6;

        /// <summary>
        /// Gets the result of every game played by a player.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>The results of the player, or an empty list if there are none.</returns>
        /// <exception cref="DataAccessException">The database could not be queried.</exception>
        public List<GameHistoryPlayer> FindByPlayerId(int playerId)
        {
            List<GameHistoryPlayer> history = DatabaseOperation.Query(
                context => SelectByPlayerId(context, playerId),
                FindByPlayerFailedMessage);

            return history;
        }

        /// <summary>
        /// Gets the players with the most accumulated points, from highest to lowest.
        /// </summary>
        /// <param name="maxEntries">Maximum number of players to return.</param>
        /// <returns>The ranking, or an empty list if there are no games played.</returns>
        /// <exception cref="DataAccessException">The database could not be queried.</exception>
        public List<PlayerRankingRecord> GetRanking(int maxEntries)
        {
            List<PlayerRankingRecord> ranking = DatabaseOperation.Query(
                context => QueryRanking(context, maxEntries),
                RankingFailedMessage);

            return ranking;
        }

        /// <summary>
        /// Saves the result of a game that just finished, with the players who took part in it, and updates
        /// the experience and the level of each player in the same transaction. The rooms are not stored while
        /// the game runs, so a row of the room is created only to link the result to it.
        /// </summary>
        /// <param name="result">Result of the game.</param>
        /// <exception cref="ArgumentNullException">The result is null.</exception>
        /// <exception cref="DataAccessException">The database could not be updated.</exception>
        public void Save(GameResultRecord result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result), NullResultMessage);
            }

            DatabaseOperation.Save(context => AddResult(context, result), SaveFailedMessage);
        }

        private static List<GameHistoryPlayer> SelectByPlayerId(YachtDiceContext context, int playerId)
        {
            Expression<Func<GameHistoryPlayer, bool>> isSamePlayerId = entry => entry.PlayerId == playerId;
            List<GameHistoryPlayer> history = context.GameHistoryPlayer
                .Where(isSamePlayerId)
                .ToList();

            return history;
        }

        private static void AddResult(YachtDiceContext context, GameResultRecord result)
        {
            DateTime now = DateTime.UtcNow;
            RoomRecordType roomType = result.IsPrivate ? RoomRecordType.Private : RoomRecordType.Public;
            var room = new Room
            {
                HostPlayerId = result.HostPlayerId,
                Code = CreateUnusedRoomCode(context),
                RoomName = ArchivedRoomName,
                MaxPlayers = result.MaxPlayers,
                RoomType = roomType.ToString(),
                Status = RoomRecordStatus.Finished.ToString(),
                CreateDate = now
            };
            var game = new GameHistory { Room = room, EndDate = now };

            foreach (PlayerResultRecord player in result.Players)
            {
                game.GameHistoryPlayer.Add(new GameHistoryPlayer
                {
                    PlayerId = player.PlayerId,
                    FinalScore = player.FinalScore,
                    IsWinner = player.IsWinner,
                    Status = player.Status.ToString()
                });
                UpdateProgress(context, player);
            }

            context.GameHistory.Add(game);
        }

        private static void UpdateProgress(YachtDiceContext context, PlayerResultRecord player)
        {
            Player account = context.Player.Find(player.PlayerId);

            if (account != null)
            {
                account.XP = player.Experience;
                account.Level = player.Level;
            }
        }

        private static string CreateUnusedRoomCode(YachtDiceContext context)
        {
            string code;
            bool isUsed;

            do
            {
                string uniqueText = Guid.NewGuid().ToString(RoomCodeFormat, CultureInfo.InvariantCulture);

                code = string.Concat(uniqueText.Take(RoomCodeLength)).ToUpperInvariant();

                Expression<Func<Room, bool>> hasCode = candidate => candidate.Code == code;
                isUsed = context.Room.Any(hasCode);
            }
            while (isUsed);

            return code;
        }

        private static List<PlayerRankingRecord> QueryRanking(YachtDiceContext context, int maxEntries)
        {
            List<PlayerRankingRecord> ranking = context.GameHistoryPlayer
                .GroupBy(entry => entry.PlayerId)
                .Select(group => new
                {
                    PlayerId = group.Key,
                    TotalPoints = group.Sum(entry => entry.FinalScore),
                    Wins = group.Count(entry => entry.IsWinner == true)
                })
                .Join(
                    context.Player,
                    totals => totals.PlayerId,
                    player => player.Id,
                    (totals, player) => new PlayerRankingRecord
                    {
                        DisplayName = player.DisplayName,
                        AvatarId = player.AvatarId,
                        TotalPoints = totals.TotalPoints,
                        Wins = totals.Wins
                    })
                .OrderByDescending(record => record.TotalPoints)
                .ThenBy(record => record.DisplayName)
                .Take(maxEntries)
                .ToList();

            return ranking;
        }
    }
}