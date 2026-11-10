using System.Linq;
using Xunit;
using YachtDice.Server.Logic.Game;
using YachtDice.Server.Logic.Rooms;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Game
{
    /// <summary>
    /// Rules of a game: turns, rolls, held dice, players who leave and the end of the game.
    /// </summary>
    public class GameManagerTests
    {
        private const string RoomCode = "ROOM01";
        private const string FirstId = "player-a";
        private const string SecondId = "player-b";
        private const string FirstName = "Emilio";
        private const string SecondName = "Gabriel";
        private const string UnknownId = "player-x";
        private const string UnknownRoomCode = "NOROOM";
        private const int RollsPerTurn = 3;
        private const int CategoryCount = 12;
        private const int LittleStraightPoints = 30;

        private static readonly int[] LittleStraightDice = { 1, 2, 3, 4, 5 };
        private static readonly int[] LittleStraightThenTwoSixes = { 1, 2, 3, 4, 5, 6, 6 };
        private static readonly int[] FirstThreeKeptWithTwoSixes = { 1, 2, 3, 6, 6 };
        private static readonly int[] NoDice = { };
        private static readonly bool[] NoHeldDice = { false, false, false, false, false };
        private static readonly bool[] FirstThreeHeld = { true, true, true, false, false };

        [Fact]
        public void Start_TwoPlayers_GivesTheFirstTurnToTheFirstPlayer()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            var expected = new
            {
                CurrentPlayerId = FirstId,
                Round = 1,
                RollsLeft = RollsPerTurn,
                Status = GameStatus.InProgress,
                Dice = NoDice
            };

            GameSnapshot snapshot = StartTwoPlayerGame(manager);

            var actual = new
            {
                snapshot.CurrentPlayerId,
                snapshot.Round,
                snapshot.RollsLeft,
                snapshot.Status,
                snapshot.Dice
            };
            Assert.Equivalent(expected, actual);
        }

        [Theory]
        [InlineData(GameSpeed.Normal, 60)]
        [InlineData(GameSpeed.Fast, 30)]
        public void Start_Speed_SetsTheSecondsOfTheTurn(GameSpeed speed, int expectedSeconds)
        {
            GameManager manager = CreateManager(LittleStraightDice);

            GameSnapshot snapshot = manager.Start(RoomCode, CreateParticipants(), speed);

            Assert.Equal(expectedSeconds, snapshot.TurnSeconds);
        }

        [Fact]
        public void Roll_FirstRollOfTheTurn_RollsTheFiveDiceAndLeavesTwoRolls()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            var expected = new { Status = GameActionStatus.Success, Dice = LittleStraightDice, RollsLeft = 2 };

            GameActionResult result = manager.Roll(RoomCode, FirstId, NoHeldDice);

            var actual = new { result.Status, result.Snapshot.Dice, result.Snapshot.RollsLeft };
            Assert.Equivalent(expected, actual);
        }

        [Fact]
        public void Roll_FirstRollIgnoresTheHeldDice_RollsAllDice()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);

            GameActionResult result = manager.Roll(RoomCode, FirstId, FirstThreeHeld);

            Assert.Equal(LittleStraightDice, result.Snapshot.Dice.ToArray());
        }

        [Fact]
        public void Roll_SecondRollWithHeldDice_KeepsTheHeldDiceAndRollsTheRest()
        {
            GameManager manager = CreateManager(LittleStraightThenTwoSixes);
            StartTwoPlayerGame(manager);
            manager.Roll(RoomCode, FirstId, NoHeldDice);
            var expected = new { Dice = FirstThreeKeptWithTwoSixes, HeldDice = FirstThreeHeld, RollsLeft = 1 };

            GameActionResult result = manager.Roll(RoomCode, FirstId, FirstThreeHeld);

            var actual = new { result.Snapshot.Dice, result.Snapshot.HeldDice, result.Snapshot.RollsLeft };
            Assert.Equivalent(expected, actual);
        }

        [Fact]
        public void Roll_FourthRollOfTheTurn_ReturnsNoRollsLeft()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            manager.Roll(RoomCode, FirstId, NoHeldDice);
            manager.Roll(RoomCode, FirstId, NoHeldDice);
            manager.Roll(RoomCode, FirstId, NoHeldDice);

            GameActionResult result = manager.Roll(RoomCode, FirstId, NoHeldDice);

            Assert.Equal(GameActionStatus.NoRollsLeft, result.Status);
        }

        [Fact]
        public void Roll_HeldDiceWithWrongLength_ReturnsInvalidHeldDice()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            manager.Roll(RoomCode, FirstId, NoHeldDice);

            GameActionResult result = manager.Roll(RoomCode, FirstId, new[] { true, false });

            Assert.Equal(GameActionStatus.InvalidHeldDice, result.Status);
        }

        [Fact]
        public void Roll_PlayerWhoseTurnIsNotNow_ReturnsNotYourTurn()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);

            GameActionResult result = manager.Roll(RoomCode, SecondId, NoHeldDice);

            Assert.Equal(GameActionStatus.NotYourTurn, result.Status);
        }

        [Fact]
        public void Roll_PlayerOutsideTheGame_ReturnsPlayerNotInGame()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);

            GameActionResult result = manager.Roll(RoomCode, UnknownId, NoHeldDice);

            Assert.Equal(GameActionStatus.PlayerNotInGame, result.Status);
        }

        [Fact]
        public void Roll_RoomWithoutGame_ReturnsGameNotFound()
        {
            GameManager manager = CreateManager(LittleStraightDice);

            GameActionResult result = manager.Roll(UnknownRoomCode, FirstId, NoHeldDice);

            Assert.Equal(GameActionStatus.GameNotFound, result.Status);
        }

        [Fact]
        public void Roll_AfterRolling_ShowsTheBoxesThatTheCurrentPlayerCanChoose()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            var expected = new { OptionCount = CategoryCount, LittleStraightPoints };

            GameActionResult result = manager.Roll(RoomCode, FirstId, NoHeldDice);

            var actual = new
            {
                OptionCount = result.Snapshot.Options.Count,
                LittleStraightPoints = FindOption(result.Snapshot, ScoreCategory.LittleStraight).Points
            };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Score_BeforeRolling_ReturnsMustRollFirst()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);

            GameActionResult result = manager.Score(RoomCode, FirstId, ScoreCategory.Choice);

            Assert.Equal(GameActionStatus.MustRollFirst, result.Status);
        }

        [Fact]
        public void Score_AfterRolling_WritesThePointsAndPassesTheTurn()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            manager.Roll(RoomCode, FirstId, NoHeldDice);
            var expected = new
            {
                Status = GameActionStatus.Success,
                CurrentPlayerId = SecondId,
                RollsLeft = RollsPerTurn,
                Dice = NoDice,
                FirstPlayerTotal = LittleStraightPoints
            };

            GameActionResult result = manager.Score(RoomCode, FirstId, ScoreCategory.LittleStraight);

            var actual = new
            {
                result.Status,
                result.Snapshot.CurrentPlayerId,
                result.Snapshot.RollsLeft,
                result.Snapshot.Dice,
                FirstPlayerTotal = FindPlayer(result.Snapshot, FirstId).Total
            };
            Assert.Equivalent(expected, actual);
        }

        [Fact]
        public void Score_PlayerWhoseTurnIsNotNow_ReturnsNotYourTurn()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            manager.Roll(RoomCode, FirstId, NoHeldDice);

            GameActionResult result = manager.Score(RoomCode, SecondId, ScoreCategory.Choice);

            Assert.Equal(GameActionStatus.NotYourTurn, result.Status);
        }

        [Fact]
        public void Score_CategoryAlreadyUsed_ReturnsCategoryFilled()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            PlayTurn(manager, FirstId, ScoreCategory.Choice);
            PlayTurn(manager, SecondId, ScoreCategory.Choice);
            manager.Roll(RoomCode, FirstId, NoHeldDice);

            GameActionResult result = manager.Score(RoomCode, FirstId, ScoreCategory.Choice);

            Assert.Equal(GameActionStatus.CategoryFilled, result.Status);
        }

        [Fact]
        public void Score_CategoryAlreadyUsed_KeepsTheTurn()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            PlayTurn(manager, FirstId, ScoreCategory.Choice);
            PlayTurn(manager, SecondId, ScoreCategory.Choice);
            manager.Roll(RoomCode, FirstId, NoHeldDice);
            manager.Score(RoomCode, FirstId, ScoreCategory.Choice);

            GameActionResult state = manager.GetState(RoomCode, FirstId);

            Assert.Equal(FirstId, state.Snapshot.CurrentPlayerId);
        }

        [Fact]
        public void Score_EveryPlayerPlayedOnce_StartsTheSecondRound()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            PlayTurn(manager, FirstId, ScoreCategory.Choice);
            var expected = new { CurrentPlayerId = FirstId, Round = 2 };

            GameActionResult result = PlayTurn(manager, SecondId, ScoreCategory.Choice);

            var actual = new { result.Snapshot.CurrentPlayerId, result.Snapshot.Round };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Score_LastBoxOfTheOnlyPlayer_FinishesTheGameWithAWinner()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartSinglePlayerGame(manager);
            var expected = new { Status = GameStatus.Finished, CurrentPlayerId = string.Empty, IsWinner = true };

            PlayEveryCategory(manager);

            GameSnapshot snapshot = manager.GetState(RoomCode, FirstId).Snapshot;
            var actual = new { snapshot.Status, snapshot.CurrentPlayerId, FindPlayer(snapshot, FirstId).IsWinner };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Score_FinishedGame_RejectsNewActions()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartSinglePlayerGame(manager);
            PlayEveryCategory(manager);

            GameActionResult result = manager.Roll(RoomCode, FirstId, NoHeldDice);

            Assert.Equal(GameActionStatus.GameFinished, result.Status);
        }

        [Fact]
        public void Score_TwoPlayersWithTheSameTotal_MarksBothAsWinners()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            var expected = new { Status = GameStatus.Finished, AreAllWinners = true };

            GameActionResult lastResult = PlayTwoPlayerGameToTheEnd(manager);

            var actual = new
            {
                lastResult.Snapshot.Status,
                AreAllWinners = lastResult.Snapshot.Players.All(player => player.IsWinner)
            };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void MarkAbsent_PlayerWithTheTurn_PassesTheTurnToTheNextPlayer()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            manager.Roll(RoomCode, FirstId, NoHeldDice);
            var expected = new { CurrentPlayerId = SecondId, Dice = NoDice, IsPresent = false };

            GameActionResult result = manager.MarkAbsent(RoomCode, FirstId);

            var actual = new
            {
                result.Snapshot.CurrentPlayerId,
                result.Snapshot.Dice,
                FindPlayer(result.Snapshot, FirstId).IsPresent
            };
            Assert.Equivalent(expected, actual);
        }

        [Fact]
        public void Score_NextPlayerIsAbsent_SkipsTheAbsentPlayer()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            manager.MarkAbsent(RoomCode, SecondId);
            manager.Roll(RoomCode, FirstId, NoHeldDice);

            GameActionResult result = manager.Score(RoomCode, FirstId, ScoreCategory.Choice);

            Assert.Equal(FirstId, result.Snapshot.CurrentPlayerId);
        }

        [Fact]
        public void MarkPresent_PlayerWhoCameBack_GetsTurnsAgain()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            manager.MarkAbsent(RoomCode, SecondId);
            manager.MarkPresent(RoomCode, SecondId);
            manager.Roll(RoomCode, FirstId, NoHeldDice);

            GameActionResult result = manager.Score(RoomCode, FirstId, ScoreCategory.Choice);

            Assert.Equal(SecondId, result.Snapshot.CurrentPlayerId);
        }

        [Fact]
        public void MarkAbsent_EveryPlayer_PausesTheGame()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            manager.MarkAbsent(RoomCode, FirstId);
            var expected = new { CurrentPlayerId = string.Empty, Status = GameStatus.InProgress };

            GameActionResult paused = manager.MarkAbsent(RoomCode, SecondId);

            var actual = new { paused.Snapshot.CurrentPlayerId, paused.Snapshot.Status };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void MarkPresent_PlayerReturnsToAPausedGame_GetsTheTurn()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            manager.MarkAbsent(RoomCode, FirstId);
            manager.MarkAbsent(RoomCode, SecondId);

            GameActionResult resumed = manager.MarkPresent(RoomCode, SecondId);

            Assert.Equal(SecondId, resumed.Snapshot.CurrentPlayerId);
        }

        [Fact]
        public void MarkAbsent_UnknownPlayer_ReturnsPlayerNotInGame()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);

            GameActionResult result = manager.MarkAbsent(RoomCode, UnknownId);

            Assert.Equal(GameActionStatus.PlayerNotInGame, result.Status);
        }

        [Fact]
        public void Remove_PlayerWithTheTurn_PassesTheTurnAndDropsThePlayer()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            var expected = new { CurrentPlayerId = SecondId, PlayerCount = 1 };

            GameActionResult result = manager.Remove(RoomCode, FirstId);

            var actual = new { result.Snapshot.CurrentPlayerId, PlayerCount = result.Snapshot.Players.Count };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Remove_PlayerWithoutTheTurn_KeepsTheTurnOfTheCurrentPlayer()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            var expected = new { CurrentPlayerId = FirstId, PlayerCount = 1 };

            GameActionResult result = manager.Remove(RoomCode, SecondId);

            var actual = new { result.Snapshot.CurrentPlayerId, PlayerCount = result.Snapshot.Players.Count };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AutoPlay_PlayerDidNotRoll_RollsAndChoosesTheBestBox()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            int turnNumber = StartTwoPlayerGame(manager).TurnNumber;
            var expected = new
            {
                Status = GameActionStatus.Success,
                CurrentPlayerId = SecondId,
                FirstPlayerTotal = LittleStraightPoints
            };

            GameActionResult result = manager.AutoPlay(RoomCode, turnNumber);

            var actual = new
            {
                result.Status,
                result.Snapshot.CurrentPlayerId,
                FirstPlayerTotal = FindPlayer(result.Snapshot, FirstId).Total
            };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AutoPlay_TurnAlreadyChanged_ReturnsNotYourTurn()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            int turnNumber = StartTwoPlayerGame(manager).TurnNumber;
            PlayTurn(manager, FirstId, ScoreCategory.Choice);

            GameActionResult result = manager.AutoPlay(RoomCode, turnNumber);

            Assert.Equal(GameActionStatus.NotYourTurn, result.Status);
        }

        [Fact]
        public void AutoPlay_TurnAlreadyChanged_KeepsTheTurnOfTheNextPlayer()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            int turnNumber = StartTwoPlayerGame(manager).TurnNumber;
            PlayTurn(manager, FirstId, ScoreCategory.Choice);
            manager.AutoPlay(RoomCode, turnNumber);

            GameActionResult state = manager.GetState(RoomCode, SecondId);

            Assert.Equal(SecondId, state.Snapshot.CurrentPlayerId);
        }

        [Fact]
        public void Roll_AfterEachChange_GivesAHigherVersionToTheSnapshot()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            GameSnapshot started = StartTwoPlayerGame(manager);
            var expected = new { IsRollNewer = true, IsScoreNewer = true };

            GameSnapshot rolled = manager.Roll(RoomCode, FirstId, NoHeldDice).Snapshot;
            GameSnapshot scored = manager.Score(RoomCode, FirstId, ScoreCategory.Choice).Snapshot;

            var actual = new
            {
                IsRollNewer = rolled.Version > started.Version,
                IsScoreNewer = scored.Version > rolled.Version
            };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetState_PlayerOutsideTheGame_ReturnsPlayerNotInGame()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);

            GameActionResult result = manager.GetState(RoomCode, UnknownId);

            Assert.Equal(GameActionStatus.PlayerNotInGame, result.Status);
        }

        [Fact]
        public void End_RoomWithGame_RemovesTheGame()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            manager.End(RoomCode);

            GameActionResult state = manager.GetState(RoomCode, FirstId);

            Assert.Equal(GameActionStatus.GameNotFound, state.Status);
        }

        private static GameManager CreateManager(int[] dieValues)
        {
            return new GameManager(new InMemoryDieSource(dieValues));
        }

        private static GameParticipant[] CreateParticipants()
        {
            return new[] { new GameParticipant(FirstId, FirstName), new GameParticipant(SecondId, SecondName) };
        }

        private static GameSnapshot StartTwoPlayerGame(GameManager manager)
        {
            return manager.Start(RoomCode, CreateParticipants(), GameSpeed.Normal);
        }

        private static GameSnapshot StartSinglePlayerGame(GameManager manager)
        {
            return manager.Start(RoomCode, new[] { new GameParticipant(FirstId, FirstName) }, GameSpeed.Normal);
        }

        private static PlayerSnapshot FindPlayer(GameSnapshot snapshot, string playerId)
        {
            return snapshot.Players.Single(player => player.PlayerId == playerId);
        }

        private static ScoreOption FindOption(GameSnapshot snapshot, ScoreCategory category)
        {
            return snapshot.Options.Single(option => option.Category == category);
        }

        private static GameActionResult PlayTurn(GameManager manager, string playerId, ScoreCategory category)
        {
            manager.Roll(RoomCode, playerId, NoHeldDice);

            return manager.Score(RoomCode, playerId, category);
        }

        private static void PlayEveryCategory(GameManager manager)
        {
            foreach (ScoreCategory category in ScoreCategories.All)
            {
                PlayTurn(manager, FirstId, category);
            }
        }

        private static GameActionResult PlayTwoPlayerGameToTheEnd(GameManager manager)
        {
            GameActionResult result = GameActionResult.CreateFailed(GameActionStatus.GameNotFound);

            foreach (ScoreCategory category in ScoreCategories.All)
            {
                PlayTurn(manager, FirstId, category);
                result = PlayTurn(manager, SecondId, category);
            }

            return result;
        }
    }
}
