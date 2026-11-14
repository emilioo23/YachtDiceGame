using System;
using System.Linq;
using Xunit;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Game;
using YachtDice.Server.Logic.Players;
using YachtDice.Server.Logic.Rooms;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Game
{
    /// <summary>
    /// Rules to save the result of a finished game: only the registered players are saved, the one who left
    /// is marked as abandoned and cannot win, nothing is saved when no registered player took part, and the
    /// experience earned is added to the account with the level that matches it.
    /// </summary>
    public class GameResultRecorderTests
    {
        private const string BetoName = "Beto";
        private const string AnaName = "Ana";
        private const string GuestName = "Invitado";
        private const string AnaUsername = "ana_99";
        private const string OtherGuestName = "Invitado2";
        private const int BetoId = 1;
        private const int AnaId = 2;
        private const int WinnerTotal = 250;
        private const int LoserTotal = 180;
        private const int RoomMaxPlayers = 3;
        private const int NoExperience = 0;
        private const int FirstLevel = 1;
        private const int SecondLevel = 2;
        private const int SecondLevelExperience = 200;
        private const int ThirdLevelExperience = 500;
        private const int PreviousExperience = 150;
        private const int ExperienceGained = 280;
        private const int ExperienceAfterTheGame = 430;

        private readonly InMemoryPlayerRepository _players = new InMemoryPlayerRepository();
        private readonly InMemoryGameHistoryRepository _history = new InMemoryGameHistoryRepository();
        private readonly GameResultRecorder _recorder;
        private readonly RoomSettings _settings = new RoomSettings { MaxPlayers = RoomMaxPlayers, IsPrivate = true };

        /// <summary>
        /// Prepares the recorder with two registered accounts, Beto and Ana.
        /// </summary>
        public GameResultRecorderTests()
        {
            _recorder = new GameResultRecorder(_players, _history, CreateLevelTable());
            _players.Add(new Player { Id = BetoId, DisplayName = BetoName, Username = BetoName });
            _players.Add(new Player { Id = AnaId, DisplayName = AnaName, Username = AnaUsername });
        }

        [Fact]
        public void Constructor_NullPlayerRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new GameResultRecorder(null, _history, CreateLevelTable()));
        }

        [Fact]
        public void Constructor_NullHistoryRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new GameResultRecorder(_players, null, CreateLevelTable()));
        }

        [Fact]
        public void Constructor_NullLevelTable_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new GameResultRecorder(_players, _history, null));
        }

        [Fact]
        public void Record_PlayerWithExperience_AddsTheGainAndUpdatesTheLevel()
        {
            _players.FindByName(BetoName).Single().XP = PreviousExperience;
            PlayerResultRecord expectedBetoResult = CreateWinnerResult(BetoId, WinnerTotal);
            expectedBetoResult.Experience = ExperienceAfterTheGame;
            expectedBetoResult.Level = SecondLevel;
            PlayerSnapshot beto = CreateWinner(BetoName, WinnerTotal, true);
            beto.ExperienceGained = ExperienceGained;

            _recorder.Record(_settings, CreateSnapshot(beto));

            PlayerResultRecord betoResult = _history.SavedResults.Single().Players.Single();
            Assert.Equivalent(expectedBetoResult, betoResult);
        }

        [Fact]
        public void Record_NullSettings_ThrowsArgumentNullException()
        {
            GameSnapshot snapshot = CreateSnapshot();

            Assert.Throws<ArgumentNullException>(() => _recorder.Record(null, snapshot));
        }

        [Fact]
        public void Record_NullSnapshot_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _recorder.Record(_settings, null));
        }

        [Fact]
        public void Record_TwoRegisteredPlayers_SavesTheWinnerAndTheLoser()
        {
            GameResultRecord expectedSavedResult = CreateExpectedResult(BetoId);
            expectedSavedResult.Players.Add(CreateWinnerResult(BetoId, WinnerTotal));
            expectedSavedResult.Players.Add(CreateResult(AnaId, LoserTotal, PlayerResultStatus.Completed));
            GameSnapshot snapshot = CreateSnapshot(
                CreateWinner(BetoName, WinnerTotal, true),
                CreatePlayer(AnaName, LoserTotal, true));

            _recorder.Record(_settings, snapshot);

            Assert.Equivalent(new[] { expectedSavedResult }, _history.SavedResults, strict: true);
        }

        [Fact]
        public void Record_GuestAndRegisteredPlayer_LeavesTheGuestOut()
        {
            GameResultRecord expectedSavedResult = CreateExpectedResult(BetoId);
            expectedSavedResult.Players.Add(CreateResult(BetoId, LoserTotal, PlayerResultStatus.Completed));
            GameSnapshot snapshot = CreateSnapshot(
                CreateWinner(GuestName, WinnerTotal, true),
                CreatePlayer(BetoName, LoserTotal, true));

            _recorder.Record(_settings, snapshot);

            Assert.Equivalent(new[] { expectedSavedResult }, _history.SavedResults, strict: true);
        }

        [Fact]
        public void Record_OnlyGuests_SavesNothing()
        {
            GameSnapshot snapshot = CreateSnapshot(
                CreateWinner(GuestName, WinnerTotal, true),
                CreatePlayer(OtherGuestName, LoserTotal, true));

            _recorder.Record(_settings, snapshot);

            Assert.Empty(_history.SavedResults);
        }

        [Fact]
        public void Record_WinnerWhoLeft_IsSavedAsAbandonedAndNotAsWinner()
        {
            GameResultRecord expectedSavedResult = CreateExpectedResult(BetoId);
            expectedSavedResult.Players.Add(CreateResult(BetoId, WinnerTotal, PlayerResultStatus.Abandoned));
            expectedSavedResult.Players.Add(CreateResult(AnaId, LoserTotal, PlayerResultStatus.Completed));
            GameSnapshot snapshot = CreateSnapshot(
                CreateWinner(BetoName, WinnerTotal, false),
                CreatePlayer(AnaName, LoserTotal, true));

            _recorder.Record(_settings, snapshot);

            Assert.Equivalent(new[] { expectedSavedResult }, _history.SavedResults, strict: true);
        }

        [Fact]
        public void Record_TwoNamesOfTheSameAccount_SavesTheAccountOnce()
        {
            GameResultRecord expectedSavedResult = CreateExpectedResult(AnaId);
            expectedSavedResult.Players.Add(CreateWinnerResult(AnaId, WinnerTotal));
            GameSnapshot snapshot = CreateSnapshot(
                CreateWinner(AnaName, WinnerTotal, true),
                CreatePlayer(AnaUsername, LoserTotal, true));

            _recorder.Record(_settings, snapshot);

            Assert.Equivalent(new[] { expectedSavedResult }, _history.SavedResults, strict: true);
        }

        private static GameResultRecord CreateExpectedResult(int hostPlayerId)
        {
            return new GameResultRecord { HostPlayerId = hostPlayerId, MaxPlayers = RoomMaxPlayers, IsPrivate = true };
        }

        private static PlayerResultRecord CreateResult(int playerId, int finalScore, PlayerResultStatus status)
        {
            return new PlayerResultRecord
            {
                PlayerId = playerId,
                FinalScore = finalScore,
                IsWinner = false,
                Status = status,
                Experience = NoExperience,
                Level = FirstLevel
            };
        }

        private static PlayerResultRecord CreateWinnerResult(int playerId, int finalScore)
        {
            PlayerResultRecord winnerResult = CreateResult(playerId, finalScore, PlayerResultStatus.Completed);
            winnerResult.IsWinner = true;
            return winnerResult;
        }

        private static LevelTable CreateLevelTable()
        {
            return new LevelTable(new[] { NoExperience, SecondLevelExperience, ThirdLevelExperience });
        }

        private static PlayerSnapshot CreatePlayer(string name, int total, bool isPresent)
        {
            return new PlayerSnapshot(name, name, isPresent) { Total = total, IsWinner = false };
        }

        private static PlayerSnapshot CreateWinner(string name, int total, bool isPresent)
        {
            PlayerSnapshot player = CreatePlayer(name, total, isPresent);
            player.IsWinner = true;
            return player;
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
