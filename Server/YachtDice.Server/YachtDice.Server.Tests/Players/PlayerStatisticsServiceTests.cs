using System;
using System.Collections.Generic;
using Xunit;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Players;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Players
{
    /// <summary>
    /// Statistics and recent games that a player sees in the profile.
    /// </summary>
    public class PlayerStatisticsServiceTests
    {
        private const string PlayerName = "emilio";
        private const string FirstOpponentName = "gabriel";
        private const string SecondOpponentName = "ana";
        private const string UnknownName = "nobody";
        private const string BothOpponents = "gabriel, ana";
        private const int PlayerId = 1;
        private const int FirstOpponentId = 2;
        private const int SecondOpponentId = 3;
        private const int FirstGameId = 10;
        private const int SecondGameId = 11;
        private const int PlayerScore = 180;
        private const int BestOpponentScore = 210;
        private const int LowerOpponentScore = 90;
        private const int PlayerExperience = 740;
        private const int ThreeDaysAgo = -3;
        private const int OneDayAgo = -1;

        private static readonly DateTime EndDate = DateTime.UtcNow;

        private readonly InMemoryPlayerRepository _players = new InMemoryPlayerRepository();
        private readonly InMemoryGameHistoryRepository _history = new InMemoryGameHistoryRepository();
        private readonly PlayerStatisticsService _service;

        /// <summary>
        /// Prepares the service with a registered player who has 740 points of experience.
        /// </summary>
        public PlayerStatisticsServiceTests()
        {
            _service = new PlayerStatisticsService(_players, _history);
            _players.Add(new Player { Id = PlayerId, Username = PlayerName, DisplayName = PlayerName, XP = PlayerExperience });
        }

        [Fact]
        public void Constructor_NullPlayerRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PlayerStatisticsService(null, _history));
        }

        [Fact]
        public void Constructor_NullHistoryRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PlayerStatisticsService(_players, null));
        }

        [Fact]
        public void GetStatistics_PlayerWithExperience_ReturnsTheTotalExperience()
        {
            PlayerStatistics statistics = _service.GetStatistics(PlayerName);

            Assert.Equal(PlayerExperience, statistics.Experience);
        }

        [Fact]
        public void GetRecentGames_UnknownPlayer_ReturnsNoGames()
        {
            List<PlayedGameSummary> games = _service.GetRecentGames(UnknownName);

            Assert.Empty(games);
        }

        [Fact]
        public void GetRecentGames_PlayerWithoutGames_ReturnsNoGames()
        {
            List<PlayedGameSummary> games = _service.GetRecentGames(PlayerName);

            Assert.Empty(games);
        }

        [Fact]
        public void GetRecentGames_WonGame_ReturnsTheGameAsAWin()
        {
            AddGame(FirstGameId, EndDate, true);

            List<PlayedGameSummary> games = _service.GetRecentGames(PlayerName);

            Assert.True(games[0].IsWin);
        }

        [Fact]
        public void GetRecentGames_LostGame_ReturnsTheGameAsALoss()
        {
            AddGame(FirstGameId, EndDate, false);

            List<PlayedGameSummary> games = _service.GetRecentGames(PlayerName);

            Assert.False(games[0].IsWin);
        }

        [Fact]
        public void GetRecentGames_PlayedGame_ReturnsTheScoreOfThePlayer()
        {
            AddGame(FirstGameId, EndDate, true);

            List<PlayedGameSummary> games = _service.GetRecentGames(PlayerName);

            Assert.Equal(PlayerScore, games[0].OwnScore);
        }

        [Fact]
        public void GetRecentGames_TwoOpponents_ReturnsTheHighestOpponentScore()
        {
            AddGame(FirstGameId, EndDate, false);

            List<PlayedGameSummary> games = _service.GetRecentGames(PlayerName);

            Assert.Equal(BestOpponentScore, games[0].BestOpponentScore);
        }

        [Fact]
        public void GetRecentGames_TwoOpponents_ReturnsTheirNamesSeparatedByCommas()
        {
            AddGame(FirstGameId, EndDate, false);

            List<PlayedGameSummary> games = _service.GetRecentGames(PlayerName);

            Assert.Equal(BothOpponents, games[0].Opponents);
        }

        [Fact]
        public void GetRecentGames_TwoGames_ReturnsTheNewestFirst()
        {
            AddGame(FirstGameId, EndDate.AddDays(ThreeDaysAgo), false);
            AddGame(SecondGameId, EndDate.AddDays(OneDayAgo), true);

            List<PlayedGameSummary> games = _service.GetRecentGames(PlayerName);

            Assert.Equal(EndDate.AddDays(OneDayAgo), games[0].PlayedAt);
        }

        private void AddGame(int gameId, DateTime endDate, bool isWinner)
        {
            _history.AddPlayedGameRecord(CreateRecord(gameId, endDate, PlayerId, PlayerName, PlayerScore, isWinner));
            _history.AddPlayedGameRecord(
                CreateRecord(gameId, endDate, FirstOpponentId, FirstOpponentName, BestOpponentScore, !isWinner));
            _history.AddPlayedGameRecord(
                CreateRecord(gameId, endDate, SecondOpponentId, SecondOpponentName, LowerOpponentScore, false));
        }

        private static PlayedGameRecord CreateRecord(
            int gameId,
            DateTime endDate,
            int playerId,
            string playerName,
            int score,
            bool isWinner)
        {
            var record = new PlayedGameRecord
            {
                GameId = gameId,
                EndDate = endDate,
                PlayerId = playerId,
                PlayerName = playerName,
                FinalScore = score,
                IsWinner = isWinner
            };

            return record;
        }
    }
}
