using System;
using System.Linq;
using Xunit;
using YachtDice.Server.Data;
using YachtDice.Server.Logic.Authentication;
using YachtDice.Server.Logic.Friends;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Friends
{
    /// <summary>
    /// Rules to report a player: only someone who finished a game with the player in the last day, once a day,
    /// never oneself, and with a short comment.
    /// </summary>
    public class PlayerReportTests
    {
        private const int ReporterId = 1;
        private const int ReportedId = 2;
        private const int StrangerId = 3;
        private const int GameId = 50;
        private const string ReporterName = "gabriel";
        private const string ReportedName = "emilio";
        private const string StrangerName = "carlos";
        private const string UnknownName = "ramiro_lopez";
        private const string Comment = "  Insulto a todos en el chat.  ";
        private const string TrimmedComment = "Insulto a todos en el chat.";
        private const int TooLongCommentLength = 301;
        private const char CommentCharacter = 'a';
        private const int TwoDaysAgo = -2;
        private const int SecondReporterId = 4;
        private const int ThirdReporterId = 5;
        private const string SecondReporterName = "ana";
        private const string ThirdReporterName = "beto";

        private readonly InMemoryPlayerRepository _players = new InMemoryPlayerRepository();
        private readonly InMemoryPlayerReportRepository _reports = new InMemoryPlayerReportRepository();
        private readonly FriendshipService _service;

        /// <summary>
        /// Prepares three registered players; the first two finished a game together just now.
        /// </summary>
        public PlayerReportTests()
        {
            _players.Add(new Player { Id = ReporterId, Username = ReporterName, DisplayName = ReporterName });
            _players.Add(new Player { Id = ReportedId, Username = ReportedName, DisplayName = ReportedName });
            _players.Add(new Player { Id = StrangerId, Username = StrangerName, DisplayName = StrangerName });
            _reports.AddGame(CreateGame(DateTime.UtcNow));
            _service = new FriendshipService(_players, new InMemoryFriendRepository(), _reports);
        }

        [Fact]
        public void Constructor_NullReportRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => new FriendshipService(_players, new InMemoryFriendRepository(), null));
        }

        [Fact]
        public void ReportPlayer_NullDetails_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _service.ReportPlayer(ReporterName, null));
        }

        [Fact]
        public void ReportPlayer_OpponentOfARecentGame_ReturnsSent()
        {
            ReportStatus status = _service.ReportPlayer(ReporterName, CreateDetails(ReportedName));

            Assert.Equal(ReportStatus.Sent, status);
        }

        [Fact]
        public void ReportPlayer_OpponentOfARecentGame_SavesTheReportWithTheGame()
        {
            var reportExpected = new
            {
                ReporterId,
                ReportedId,
                GameId = (int?)GameId,
                Reason = ReportReason.OffensiveLanguage.ToString(),
                Comment = TrimmedComment
            };

            _service.ReportPlayer(ReporterName, CreateDetails(ReportedName));

            Assert.Equivalent(reportExpected, _reports.Reports.Single());
        }

        [Fact]
        public void ReportPlayer_SamePlayerTwiceInADay_ReturnsAlreadyReported()
        {
            _service.ReportPlayer(ReporterName, CreateDetails(ReportedName));

            ReportStatus status = _service.ReportPlayer(ReporterName, CreateDetails(ReportedName));

            Assert.Equal(ReportStatus.AlreadyReported, status);
        }

        [Fact]
        public void ReportPlayer_PlayerWithoutARecentGame_ReturnsNoRecentGame()
        {
            ReportStatus status = _service.ReportPlayer(ReporterName, CreateDetails(StrangerName));

            Assert.Equal(ReportStatus.NoRecentGame, status);
        }

        [Fact]
        public void ReportPlayer_GameOfTwoDaysAgo_ReturnsNoRecentGame()
        {
            var oldGameReports = new InMemoryPlayerReportRepository();
            oldGameReports.AddGame(CreateGame(DateTime.UtcNow.AddDays(TwoDaysAgo)));
            var service = new FriendshipService(_players, new InMemoryFriendRepository(), oldGameReports);

            ReportStatus status = service.ReportPlayer(ReporterName, CreateDetails(ReportedName));

            Assert.Equal(ReportStatus.NoRecentGame, status);
        }

        [Fact]
        public void ReportPlayer_Oneself_ReturnsCannotReportSelf()
        {
            ReportStatus status = _service.ReportPlayer(ReporterName, CreateDetails(ReporterName));

            Assert.Equal(ReportStatus.CannotReportSelf, status);
        }

        [Fact]
        public void ReportPlayer_UnknownPlayer_ReturnsPlayerNotFound()
        {
            ReportStatus status = _service.ReportPlayer(ReporterName, CreateDetails(UnknownName));

            Assert.Equal(ReportStatus.PlayerNotFound, status);
        }

        [Fact]
        public void ReportPlayer_TooLongComment_ReturnsInvalidComment()
        {
            ReportDetails details = CreateDetails(ReportedName);
            details.Comment = new string(CommentCharacter, TooLongCommentLength);

            ReportStatus status = _service.ReportPlayer(ReporterName, details);

            Assert.Equal(ReportStatus.InvalidComment, status);
        }

        [Fact]
        public void ReportPlayer_ThirdDifferentReporterInADay_SuspendsTheReportedPlayer()
        {
            AddReporterWithGame(SecondReporterId, SecondReporterName);
            AddReporterWithGame(ThirdReporterId, ThirdReporterName);
            _service.ReportPlayer(ReporterName, CreateDetails(ReportedName));
            _service.ReportPlayer(SecondReporterName, CreateDetails(ReportedName));

            _service.ReportPlayer(ThirdReporterName, CreateDetails(ReportedName));

            Assert.Equal(LoginLock.Reported.ToString(), _players.FindLocks(ReportedId).Single().LockType);
        }

        [Fact]
        public void ReportPlayer_TwoDifferentReporters_DoesNotSuspendThePlayer()
        {
            AddReporterWithGame(SecondReporterId, SecondReporterName);
            _service.ReportPlayer(ReporterName, CreateDetails(ReportedName));

            _service.ReportPlayer(SecondReporterName, CreateDetails(ReportedName));

            Assert.Empty(_players.FindLocks(ReportedId));
        }

        private void AddReporterWithGame(int reporterId, string reporterName)
        {
            _players.Add(new Player { Id = reporterId, Username = reporterName, DisplayName = reporterName });

            var game = new GameHistory { Id = reporterId, EndDate = DateTime.UtcNow };
            game.GameHistoryPlayer.Add(new GameHistoryPlayer { GameId = reporterId, PlayerId = reporterId });
            game.GameHistoryPlayer.Add(new GameHistoryPlayer { GameId = reporterId, PlayerId = ReportedId });
            _reports.AddGame(game);
        }

        private static ReportDetails CreateDetails(string reportedName)
        {
            return new ReportDetails
            {
                ReportedName = reportedName,
                Reason = ReportReason.OffensiveLanguage,
                Comment = Comment
            };
        }

        private static GameHistory CreateGame(DateTime endDate)
        {
            var game = new GameHistory { Id = GameId, EndDate = endDate };
            game.GameHistoryPlayer.Add(new GameHistoryPlayer { GameId = GameId, PlayerId = ReporterId });
            game.GameHistoryPlayer.Add(new GameHistoryPlayer { GameId = GameId, PlayerId = ReportedId });

            return game;
        }
    }
}
