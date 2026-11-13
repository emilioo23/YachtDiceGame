using System;
using Xunit;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Game;
using YachtDice.Server.Logic.Rooms;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Game
{
    /// <summary>
    /// Rules to save the result of a finished game: only the registered players are saved, the one who left
    /// is marked as abandoned and cannot win, and nothing is saved when no registered player took part.
    /// </summary>
    public class GameResultRecorderTests
    {
        private const string BetoName = "Beto";
        private const string AnaName = "Ana";
        private const string GuestName = "Invitado";
        private const string AnaUsername = "ana_99";
        private const string CompletedStatus = "Completed";
        private const string AbandonedStatus = "Abandoned";
        private const int BetoId = 1;
        private const int AnaId = 2;
        private const int WinnerTotal = 250;
        private const int LoserTotal = 180;
        private const int RoomMaxPlayers = 3;

        private readonly InMemoryPlayerRepository _players = new InMemoryPlayerRepository();
        private readonly InMemoryGameHistoryRepository _history = new InMemoryGameHistoryRepository();
        private readonly GameResultRecorder _recorder;
        private readonly RoomSettings _settings = new RoomSettings { MaxPlayers = RoomMaxPlayers, IsPrivate = true };

        /// <summary>
        /// Prepares the recorder with two registered accounts, Beto and Ana.
        /// </summary>
        public GameResultRecorderTests()
        {
            _recorder = new GameResultRecorder(_players, _history);
            _players.Add(new Player { Id = BetoId, DisplayName = BetoName, Username = BetoName });
            _players.Add(new Player { Id = AnaId, DisplayName = AnaName, Username = AnaUsername });
        }

        [Fact]
        public void Constructor_NullPlayerRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new GameResultRecorder(null, _history));
        }

        [Fact]
        public void Constructor_NullHistoryRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new GameResultRecorder(_players, null));
        }

        [Fact]
        public void Record_TwoRegisteredPlayers_SavesTheWinnerAndTheLoser()
        {
            var expected = CreateExpectedResult(BetoId);
            expected.Players.Add(CreateResult(BetoId, WinnerTotal, true, CompletedStatus));
            expected.Players.Add(CreateResult(AnaId, LoserTotal, false, CompletedStatus));
            GameSnapshot snapshot = CreateSnapshot(
                CreatePlayer(BetoName, WinnerTotal, true, true),
                CreatePlayer(AnaName, LoserTotal, false, true));

            _recorder.Record(_settings, snapshot);

            Assert.Equivalent(new[] { expected }, _history.SavedResults, strict: true);
        }

        [Fact]
        public void Record_GuestAndRegisteredPlayer_LeavesTheGuestOut()
        {
            var expected = CreateExpectedResult(BetoId);
            expected.Players.Add(CreateResult(BetoId, LoserTotal, false, CompletedStatus));
            GameSnapshot snapshot = CreateSnapshot(
                CreatePlayer(GuestName, WinnerTotal, true, true),
                CreatePlayer(BetoName, LoserTotal, false, true));

            _recorder.Record(_settings, snapshot);

            Assert.Equivalent(new[] { expected }, _history.SavedResults, strict: true);
        }

        [Fact]
        public void Record_OnlyGuests_SavesNothing()
        {
            GameSnapshot snapshot = CreateSnapshot(
                CreatePlayer(GuestName, WinnerTotal, true, true),
                CreatePlayer(GuestName + "2", LoserTotal, false, true));

            _recorder.Record(_settings, snapshot);

            Assert.Empty(_history.SavedResults);
        }

        [Fact]
        public void Record_WinnerWhoLeft_IsSavedAsAbandonedAndNotAsWinner()
        {
            var expected = CreateExpectedResult(BetoId);
            expected.Players.Add(CreateResult(BetoId, WinnerTotal, false, AbandonedStatus));
            expected.Players.Add(CreateResult(AnaId, LoserTotal, false, CompletedStatus));
            GameSnapshot snapshot = CreateSnapshot(
                CreatePlayer(BetoName, WinnerTotal, true, false),
                CreatePlayer(AnaName, LoserTotal, false, true));

            _recorder.Record(_settings, snapshot);

            Assert.Equivalent(new[] { expected }, _history.SavedResults, strict: true);
        }

        [Fact]
        public void Record_TwoNamesOfTheSameAccount_SavesTheAccountOnce()
        {
            var expected = CreateExpectedResult(AnaId);
            expected.Players.Add(CreateResult(AnaId, WinnerTotal, true, CompletedStatus));
            GameSnapshot snapshot = CreateSnapshot(
                CreatePlayer(AnaName, WinnerTotal, true, true),
                CreatePlayer(AnaUsername, LoserTotal, false, true));

            _recorder.Record(_settings, snapshot);

            Assert.Equivalent(new[] { expected }, _history.SavedResults, strict: true);
        }

        private static GameResultRecord CreateExpectedResult(int hostPlayerId)
        {
            return new GameResultRecord { HostPlayerId = hostPlayerId, MaxPlayers = RoomMaxPlayers, IsPrivate = true };
        }

        private static PlayerResultRecord CreateResult(int playerId, int finalScore, bool isWinner, string status)
        {
            return new PlayerResultRecord
            {
                PlayerId = playerId,
                FinalScore = finalScore,
                IsWinner = isWinner,
                Status = status
            };
        }

        private static PlayerSnapshot CreatePlayer(string name, int total, bool isWinner, bool isPresent)
        {
            return new PlayerSnapshot(name, name, isPresent) { Total = total, IsWinner = isWinner };
        }

        private static GameSnapshot CreateSnapshot(params PlayerSnapshot[] players)
        {
            var snapshot = new GameSnapshot { Status = GameStatus.Finished };

            foreach (PlayerSnapshot player in players)
            {
                snapshot.Players.Add(player);
            }

            return snapshot;
        }
    }
}
