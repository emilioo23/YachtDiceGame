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
        private const string RecentGamesFailedMessage = "Database error occurred while reading the recent games.";
        private const string FindByPlayerFailedMessage =
            "Database error occurred while fetching game history by player ID.";

        private const int FirstLevel = 1;
        private const string RankingFailedMessage = "Database error occurred while calculating player rankings.";
        private const string SaveFailedMessage = "Database error occurred while saving the result of a game.";
        private const string NullResultMessage = "The result of the game cannot be null.";
        private const string ArchivedRoomName = "Partida";
        private const string RoomCodeFormat = "N";
        private const int RoomCodeLength = 6;
        private const string ComputerGamesFailedMessage =
            "Database error occurred while reading the games against the computer.";

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

        /// <inheritdoc />
        public List<GameHistoryPlayer> FindComputerGames(int playerId)
        {
            List<GameHistoryPlayer> history = DatabaseOperation.Query(
                context => SelectComputerGames(context, playerId),
                ComputerGamesFailedMessage);

            return history;
        }

        /// <summary>
        /// Gets the totals of every player who finished at least one game, so each player has a position.
        /// </summary>
        /// <returns>The totals of the players, or an empty list if there are no games played.</returns>
        /// <exception cref="DataAccessException">The database could not be queried.</exception>
        public List<PlayerRankingRecord> GetRanking()
        {
            List<PlayerRankingRecord> ranking = DatabaseOperation.Query(QueryRanking, RankingFailedMessage);

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

        /// <summary>
        /// Gets the most recent games of a player, with the score of every player who took part in them.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="maxGames">Maximum number of games to return.</param>
        /// <returns>One record for each player of each game, or an empty list if the player has no games.</returns>
        /// <exception cref="DataAccessException">The database could not be queried.</exception>
        public List<PlayedGameRecord> FindRecentGames(int playerId, int maxGames)
        {
            List<PlayedGameRecord> games = DatabaseOperation.Query(
                context => SelectRecentGames(context, playerId, maxGames),
                RecentGamesFailedMessage);

            return games;
        }

        private static List<PlayedGameRecord> SelectRecentGames(YachtDiceContext context, int playerId, int maxGames)
        {
            Expression<Func<GameHistoryPlayer, bool>> isSamePlayer =
                historyPlayer => historyPlayer.PlayerId == playerId;
            Expression<Func<GameHistoryPlayer, DateTime?>> readEndDate =
                historyPlayer => historyPlayer.GameHistory.EndDate;
            Expression<Func<GameHistoryPlayer, int>> readGameId = historyPlayer => historyPlayer.GameId;
            List<int> gameIds = context.GameHistoryPlayer
                .Where(isSamePlayer)
                .OrderByDescending(readEndDate)
                .Select(readGameId)
                .Take(maxGames)
                .ToList();
            Expression<Func<GameHistoryPlayer, bool>> isInRecentGames =
                historyPlayer => gameIds.Contains(historyPlayer.GameId);
            Expression<Func<GameHistoryPlayer, PlayedGameRecord>> createRecord = historyPlayer => new PlayedGameRecord
            {
                GameId = historyPlayer.GameId,
                EndDate = historyPlayer.GameHistory.EndDate,
                PlayerId = historyPlayer.PlayerId,
                PlayerName = historyPlayer.Player.DisplayName,
                FinalScore = historyPlayer.FinalScore,
                IsWinner = historyPlayer.IsWinner == true
            };

            return context.GameHistoryPlayer.Where(isInRecentGames).Select(createRecord).ToList();
        }

        private static List<GameHistoryPlayer> SelectByPlayerId(YachtDiceContext context, int playerId)
        {
            Expression<Func<GameHistoryPlayer, bool>> isSamePlayerId =
                historyPlayer => historyPlayer.PlayerId == playerId;
            List<GameHistoryPlayer> history = context.GameHistoryPlayer
                .Where(isSamePlayerId)
                .ToList();

            return history;
        }

        private static List<GameHistoryPlayer> SelectComputerGames(YachtDiceContext context, int playerId)
        {
            string computerType = RoomRecordType.Computer.ToString();
            Expression<Func<GameHistoryPlayer, bool>> isComputerGameOfPlayer =
                historyPlayer => historyPlayer.PlayerId == playerId
                    && historyPlayer.GameHistory.Room.RoomType == computerType;

            return context.GameHistoryPlayer
                .Where(isComputerGameOfPlayer)
                .ToList();
        }

        private static RoomRecordType ChooseRoomType(GameResultRecord result)
        {
            RoomRecordType roomType = RoomRecordType.Public;

            if (result.HasComputer)
            {
                roomType = RoomRecordType.Computer;
            }
            else if (result.IsPrivate)
            {
                roomType = RoomRecordType.Private;
            }

            return roomType;
        }

        private static void AddResult(YachtDiceContext context, GameResultRecord result)
        {
            DateTime now = DateTime.UtcNow;
            RoomRecordType roomType = ChooseRoomType(result);
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

        private static List<PlayerRankingRecord> QueryRanking(YachtDiceContext context)
        {
            List<PlayerRankingRecord> ranking = context.GameHistoryPlayer
                .GroupBy(historyPlayer => historyPlayer.PlayerId)
                .Select(group => new
                {
                    PlayerId = group.Key,
                    TotalPoints = group.Sum(historyPlayer => historyPlayer.FinalScore),
                    Wins = group.Count(historyPlayer => historyPlayer.IsWinner == true),
                    GamesPlayed = group.Count()
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
                        Wins = totals.Wins,
                        GamesPlayed = totals.GamesPlayed,
                        Level = player.Level ?? FirstLevel
                    })
                .ToList();

            return ranking;
        }
    }
}