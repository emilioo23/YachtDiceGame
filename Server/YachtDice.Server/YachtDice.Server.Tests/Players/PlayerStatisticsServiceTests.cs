using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        private const string UnknownName = "ramiro_lopez";
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
        private const int RankingPoints = 500;
        private const int LowerRankingPoints = 120;
        private const int ManyWins = 4;
        private const int FewWins = 1;
        private const int TwoGames = 2;
        private const int OneWin = 1;
        private const int FirstPlace = 1;
        private const int ThirdPlace = 3;
        private const int TopSize = 100;
        private const int TwoEntries = 2;
        private const int RankingGames = 9;
        private const int RankingLevel = 4;
        private const int RankingExperience = 350;
        private const int NoExperience = 0;
        private const int SecondLevelExperience = 100;
        private const int ThirdLevelExperience = 200;
        private const int FourthLevelExperience = 300;
        private const string RankedPlayerNameFormat = "jugador_{0}";

        private static readonly DateTime EndDate = DateTime.UtcNow;

        private readonly InMemoryPlayerRepository _players = new InMemoryPlayerRepository();
        private readonly InMemoryGameHistoryRepository _history = new InMemoryGameHistoryRepository();
        private readonly PlayerStatisticsService _service;

        /// <summary>
        /// Prepares the service with a registered player who has 740 points of experience.
        /// </summary>
        public PlayerStatisticsServiceTests()
        {
            _service = new PlayerStatisticsService(_players, _history, CreateLevelTable());
            _players.Add(new Player
            {
                Id = PlayerId,
                Username = PlayerName,
                DisplayName = PlayerName,
                XP = PlayerExperience
            });
        }

        [Fact]
        public void Constructor_NullPlayerRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PlayerStatisticsService(null, _history, CreateLevelTable()));
        }

        [Fact]
        public void Constructor_NullHistoryRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => new PlayerStatisticsService(_players, null, CreateLevelTable()));
        }

        [Fact]
        public void Constructor_NullLevelTable_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PlayerStatisticsService(_players, _history, null));
        }

        [Fact]
        public void GetStatistics_PlayerWithExperience_ReturnsTheTotalExperience()
        {
            PlayerStatistics statistics = _service.GetStatistics(PlayerName);

            Assert.Equal(PlayerExperience, statistics.Experience);
        }

        [Fact]
        public void GetStatistics_TwoGamesAgainstTheComputer_CountsTheGamesAndTheWinsApart()
        {
            _history.AddComputerGame(PlayerId, PlayerScore, true);
            _history.AddComputerGame(PlayerId, LowerOpponentScore, false);
            var computerStatisticsExpected = new { GamesAgainstComputer = TwoGames, WinsAgainstComputer = OneWin };

            PlayerStatistics statistics = _service.GetStatistics(PlayerName);

            var computerStatisticsResult = new { statistics.GamesAgainstComputer, statistics.WinsAgainstComputer };
            Assert.Equal(computerStatisticsExpected, computerStatisticsResult);
        }

        [Fact]
        public void GetScorecards_PlayerWhoTookPart_ReturnsTheScorecardsOfTheGame()
        {
            _history.Scorecards = new List<ScorecardRecord>
            {
                CreateScorecard(PlayerName),
                CreateScorecard(FirstOpponentName)
            };
            var scorecardsExpected = new[]
            {
                new { PlayerName, Total = PlayerScore },
                new { PlayerName = FirstOpponentName, Total = PlayerScore }
            };

            List<SavedScorecard> scorecards = _service.GetScorecards(PlayerName, FirstGameId);

            Assert.Equivalent(scorecardsExpected, scorecards);
        }

        [Fact]
        public void GetScorecards_PlayerWhoDidNotTakePart_ReturnsAnEmptyList()
        {
            _history.Scorecards = new List<ScorecardRecord> { CreateScorecard(FirstOpponentName) };

            List<SavedScorecard> scorecards = _service.GetScorecards(PlayerName, FirstGameId);

            Assert.Empty(scorecards);
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

        [Fact]
        public void GetTopPlayers_SamePointsDifferentWins_PutsTheOneWithMoreWinsFirst()
        {
            AddRanking(FirstOpponentName, RankingPoints, FewWins);
            AddRanking(SecondOpponentName, RankingPoints, ManyWins);
            var namesExpected = new List<string> { SecondOpponentName, FirstOpponentName };

            List<LeaderboardEntry> entries = _service.GetTopPlayers(PlayerName);

            Assert.Equal(namesExpected, entries.Select(leaderboardEntry => leaderboardEntry.DisplayName).ToList());
        }

        [Fact]
        public void GetTopPlayers_SamePointsAndWins_SharesThePosition()
        {
            AddRanking(PlayerName, RankingPoints, ManyWins);
            AddRanking(FirstOpponentName, RankingPoints, ManyWins);
            AddRanking(SecondOpponentName, LowerRankingPoints, ManyWins);
            var positionsExpected = new List<int> { FirstPlace, FirstPlace, ThirdPlace };

            List<LeaderboardEntry> entries = _service.GetTopPlayers(PlayerName);

            Assert.Equal(positionsExpected, entries.Select(leaderboardEntry => leaderboardEntry.Position).ToList());
        }

        [Fact]
        public void GetTopPlayers_ViewerOutsideTheTop_AddsTheViewerAtTheEnd()
        {
            AddPlayersAbove(TopSize);
            AddRanking(PlayerName, LowerRankingPoints, FewWins);
            var lastEntryExpected = new { DisplayName = PlayerName, Position = TopSize + 1 };

            List<LeaderboardEntry> entries = _service.GetTopPlayers(PlayerName);

            Assert.Equivalent(lastEntryExpected, entries.Last());
        }

        [Fact]
        public void GetTopPlayers_ViewerInsideTheTop_DoesNotRepeatTheViewer()
        {
            AddRanking(PlayerName, RankingPoints, ManyWins);
            AddRanking(FirstOpponentName, LowerRankingPoints, FewWins);

            List<LeaderboardEntry> entries = _service.GetTopPlayers(PlayerName);

            Assert.Equal(TwoEntries, entries.Count);
        }

        [Fact]
        public void GetTopPlayers_MoreThanTheTop_ShowsOnlyTheTop()
        {
            AddPlayersAbove(TopSize + 1);

            List<LeaderboardEntry> entries = _service.GetTopPlayers(PlayerName);

            Assert.Equal(TopSize, entries.Count);
        }

        [Fact]
        public void GetTopPlayers_AnyPlayer_CalculatesTheLevelAndKeepsTheGamesPlayed()
        {
            _history.AddRankingRecord(new PlayerRankingRecord
            {
                DisplayName = PlayerName,
                TotalPoints = RankingPoints,
                Wins = ManyWins,
                GamesPlayed = RankingGames,
                Experience = RankingExperience
            });
            var leaderboardEntryExpected = new { GamesPlayed = RankingGames, Level = RankingLevel };

            LeaderboardEntry leaderboardEntry = _service.GetTopPlayers(PlayerName).Single();

            Assert.Equivalent(leaderboardEntryExpected, leaderboardEntry);
        }

        private void AddGame(int gameId, DateTime endDate, bool isWinner)
        {
            AddRecord(gameId, endDate, new PlayedGameRecord
            {
                PlayerId = PlayerId,
                PlayerName = PlayerName,
                FinalScore = PlayerScore,
                IsWinner = isWinner
            });
            AddRecord(gameId, endDate, new PlayedGameRecord
            {
                PlayerId = FirstOpponentId,
                PlayerName = FirstOpponentName,
                FinalScore = BestOpponentScore,
                IsWinner = !isWinner
            });
            AddRecord(gameId, endDate, new PlayedGameRecord
            {
                PlayerId = SecondOpponentId,
                PlayerName = SecondOpponentName,
                FinalScore = LowerOpponentScore,
                IsWinner = false
            });
        }

        private void AddRecord(int gameId, DateTime endDate, PlayedGameRecord record)
        {
            record.GameId = gameId;
            record.EndDate = endDate;
            record.IsAgainstComputer = false;
            _history.AddPlayedGameRecord(record);
        }

        private static ScorecardRecord CreateScorecard(string playerName)
        {
            return new ScorecardRecord { PlayerName = playerName, Total = PlayerScore };
        }

        private static LevelTable CreateLevelTable()
        {
            return new LevelTable(new[]
            {
                NoExperience,
                SecondLevelExperience,
                ThirdLevelExperience,
                FourthLevelExperience
            });
        }

        private void AddRanking(string name, int totalPoints, int wins)
        {
            var record = new PlayerRankingRecord { DisplayName = name, TotalPoints = totalPoints, Wins = wins };

            _history.AddRankingRecord(record);
        }

        private void AddPlayersAbove(int count)
        {
            for (int number = 0; number < count; number++)
            {
                string name = string.Format(CultureInfo.InvariantCulture, RankedPlayerNameFormat, number);

                AddRanking(name, RankingPoints + number, ManyWins);
            }
        }
    }
}
