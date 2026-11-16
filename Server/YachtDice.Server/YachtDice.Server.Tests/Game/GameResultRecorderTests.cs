using System;
using System.Linq;
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
    /// is marked as abandoned and cannot win, nothing is saved when no registered player took part, and the
    /// experience earned is added to the account.
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
        private const int PreviousExperience = 150;
        private const int ExperienceGained = 280;
        private const int ExperienceAfterTheGame = 430;
        private const int UpperBonusPoints = 35;
        private const int YachtPoints = 50;

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
        public void Record_PlayerWithExperience_AddsTheGainToTheExperience()
        {
            _players.FindByName(BetoName).Single().XP = PreviousExperience;
            PlayerResultRecord betoResultExpected = CreateWinnerResult(BetoId, WinnerTotal);
            betoResultExpected.Experience = ExperienceAfterTheGame;
            PlayerSnapshot beto = CreateWinner(BetoName, WinnerTotal, true);
            beto.ExperienceGained = ExperienceGained;

            _recorder.Record(_settings, CreateSnapshot(beto));

            PlayerResultRecord betoResult = _history.SavedResults.Single().Players.Single();
            Assert.Equivalent(betoResultExpected, betoResult);
        }

        [Fact]
        public void Record_PlayerWithScores_SavesTheScorecardOfThePlayer()
        {
            PlayerSnapshot beto = CreateWinner(BetoName, WinnerTotal, true);
            beto.UpperBonus = UpperBonusPoints;
            beto.YachtBonus = ScoreCard.YachtBonusPoints;
            beto.Scores.Add(new ScoreEntry(ScoreCategory.Yacht, true, YachtPoints));
            var scorecardExpected = new
            {
                HasUpperBonus = true,
                YachtBonusCount = 1,
                Scores = new[]
                {
                    new { CategoryName = ScoreCategory.Yacht.ToString(), Points = YachtPoints, IsUsed = true }
                }
            };

            _recorder.Record(_settings, CreateSnapshot(beto));

            Assert.Equivalent(scorecardExpected, _history.SavedResults.Single().Players.Single());
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
            GameResultRecord savedResultExpected = CreateExpectedResult(BetoId);
            savedResultExpected.Players.Add(CreateWinnerResult(BetoId, WinnerTotal));
            savedResultExpected.Players.Add(CreateResult(AnaId, LoserTotal, PlayerResultStatus.Completed));
            GameSnapshot snapshot = CreateSnapshot(
                CreateWinner(BetoName, WinnerTotal, true),
                CreatePlayer(AnaName, LoserTotal, true));

            _recorder.Record(_settings, snapshot);

            Assert.Equivalent(new[] { savedResultExpected }, _history.SavedResults, strict: true);
        }

        [Fact]
        public void Record_GuestAndRegisteredPlayer_LeavesTheGuestOut()
        {
            GameResultRecord savedResultExpected = CreateExpectedResult(BetoId);
            savedResultExpected.Players.Add(CreateResult(BetoId, LoserTotal, PlayerResultStatus.Completed));
            GameSnapshot snapshot = CreateSnapshot(
                CreateWinner(GuestName, WinnerTotal, true),
                CreatePlayer(BetoName, LoserTotal, true));

            _recorder.Record(_settings, snapshot);

            Assert.Equivalent(new[] { savedResultExpected }, _history.SavedResults, strict: true);
        }

        [Fact]
        public void Record_ComputerAndRegisteredPlayer_LeavesTheComputerOut()
        {
            GameResultRecord savedResultExpected = CreateExpectedResult(BetoId);
            savedResultExpected.Players.Add(CreateResult(BetoId, LoserTotal, PlayerResultStatus.Completed));
            savedResultExpected.HasComputer = true;
            PlayerSnapshot computer = CreateWinner(AnaName, WinnerTotal, true);
            computer.IsComputer = true;

            _recorder.Record(_settings, CreateSnapshot(computer, CreatePlayer(BetoName, LoserTotal, true)));

            Assert.Equivalent(new[] { savedResultExpected }, _history.SavedResults, strict: true);
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
            GameResultRecord savedResultExpected = CreateExpectedResult(BetoId);
            savedResultExpected.Players.Add(CreateResult(BetoId, WinnerTotal, PlayerResultStatus.Abandoned));
            savedResultExpected.Players.Add(CreateResult(AnaId, LoserTotal, PlayerResultStatus.Completed));
            GameSnapshot snapshot = CreateSnapshot(
                CreateWinner(BetoName, WinnerTotal, false),
                CreatePlayer(AnaName, LoserTotal, true));

            _recorder.Record(_settings, snapshot);

            Assert.Equivalent(new[] { savedResultExpected }, _history.SavedResults, strict: true);
        }

        [Fact]
        public void Record_TwoNamesOfTheSameAccount_SavesTheAccountOnce()
        {
            GameResultRecord savedResultExpected = CreateExpectedResult(AnaId);
            savedResultExpected.Players.Add(CreateWinnerResult(AnaId, WinnerTotal));
            GameSnapshot snapshot = CreateSnapshot(
                CreateWinner(AnaName, WinnerTotal, true),
                CreatePlayer(AnaUsername, LoserTotal, true));

            _recorder.Record(_settings, snapshot);

            Assert.Equivalent(new[] { savedResultExpected }, _history.SavedResults, strict: true);
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
                Experience = NoExperience
            };
        }

        private static PlayerResultRecord CreateWinnerResult(int playerId, int finalScore)
        {
            PlayerResultRecord winnerResult = CreateResult(playerId, finalScore, PlayerResultStatus.Completed);
            winnerResult.IsWinner = true;
            return winnerResult;
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
