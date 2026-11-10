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
        private const string UnknownId = "player-x";
        private const string UnknownRoomCode = "NOROOM";

        private static readonly bool[] NoHeldDice = { false, false, false, false, false };
        private static readonly bool[] FirstThreeHeld = { true, true, true, false, false };

        [Fact]
        public void Start_TwoPlayers_GivesTheFirstTurnToTheFirstPlayer()
        {
            var manager = CreateManager(1, 2, 3, 4, 5);

            var snapshot = StartTwoPlayerGame(manager);

            Assert.Equal(FirstId, snapshot.CurrentPlayerId);
            Assert.Equal(1, snapshot.Round);
            Assert.Equal(3, snapshot.RollsLeft);
            Assert.Equal(GameStatus.InProgress, snapshot.Status);
            Assert.Empty(snapshot.Dice);
        }

        [Theory]
        [InlineData(GameSpeed.Normal, 60)]
        [InlineData(GameSpeed.Fast, 30)]
        public void Start_Speed_SetsTheSecondsOfTheTurn(GameSpeed speed, int expectedSeconds)
        {
            var manager = CreateManager(1, 2, 3, 4, 5);

            var snapshot = manager.Start(RoomCode, CreateParticipants(), speed);

            Assert.Equal(expectedSeconds, snapshot.TurnSeconds);
        }

        [Fact]
        public void Roll_FirstRollOfTheTurn_RollsTheFiveDiceAndLeavesTwoRolls()
        {
            var manager = CreateManager(1, 2, 3, 4, 5);
            StartTwoPlayerGame(manager);

            var result = manager.Roll(RoomCode, FirstId, NoHeldDice);

            Assert.Equal(GameActionStatus.Success, result.Status);
            Assert.Equal(new[] { 1, 2, 3, 4, 5 }, result.Snapshot.Dice.ToArray());
            Assert.Equal(2, result.Snapshot.RollsLeft);
        }

        [Fact]
        public void Roll_FirstRollIgnoresTheHeldDice_RollsAllDice()
        {
            var manager = CreateManager(1, 2, 3, 4, 5);
            StartTwoPlayerGame(manager);

            var result = manager.Roll(RoomCode, FirstId, FirstThreeHeld);

            Assert.Equal(new[] { 1, 2, 3, 4, 5 }, result.Snapshot.Dice.ToArray());
        }

        [Fact]
        public void Roll_SecondRollWithHeldDice_KeepsTheHeldDiceAndRollsTheRest()
        {
            var manager = CreateManager(1, 2, 3, 4, 5, 6, 6);
            StartTwoPlayerGame(manager);
            manager.Roll(RoomCode, FirstId, NoHeldDice);

            var result = manager.Roll(RoomCode, FirstId, FirstThreeHeld);

            Assert.Equal(new[] { 1, 2, 3, 6, 6 }, result.Snapshot.Dice.ToArray());
            Assert.Equal(new[] { true, true, true, false, false }, result.Snapshot.HeldDice.ToArray());
            Assert.Equal(1, result.Snapshot.RollsLeft);
        }

        [Fact]
        public void Roll_FourthRollOfTheTurn_ReturnsNoRollsLeft()
        {
            var manager = CreateManager(1, 2, 3, 4, 5);
            StartTwoPlayerGame(manager);
            manager.Roll(RoomCode, FirstId, NoHeldDice);
            manager.Roll(RoomCode, FirstId, NoHeldDice);
            manager.Roll(RoomCode, FirstId, NoHeldDice);

            var result = manager.Roll(RoomCode, FirstId, NoHeldDice);

            Assert.Equal(GameActionStatus.NoRollsLeft, result.Status);
        }

        [Fact]
        public void Roll_HeldDiceWithWrongLength_ReturnsInvalidHeldDice()
        {
            var manager = CreateManager(1, 2, 3, 4, 5);
            StartTwoPlayerGame(manager);
            manager.Roll(RoomCode, FirstId, NoHeldDice);

            var result = manager.Roll(RoomCode, FirstId, new[] { true, false });

            Assert.Equal(GameActionStatus.InvalidHeldDice, result.Status);
        }

        [Fact]
        public void Roll_PlayerWhoseTurnIsNotNow_ReturnsNotYourTurn()
        {
            var manager = CreateManager(1, 2, 3, 4, 5);
            StartTwoPlayerGame(manager);

            var result = manager.Roll(RoomCode, SecondId, NoHeldDice);

            Assert.Equal(GameActionStatus.NotYourTurn, result.Status);
        }

        [Fact]
        public void Roll_PlayerOutsideTheGame_ReturnsPlayerNotInGame()
        {
            var manager = CreateManager(1, 2, 3, 4, 5);
            StartTwoPlayerGame(manager);

            var result = manager.Roll(RoomCode, UnknownId, NoHeldDice);

            Assert.Equal(GameActionStatus.PlayerNotInGame, result.Status);
        }

        [Fact]
        public void Roll_RoomWithoutGame_ReturnsGameNotFound()
        {
            var manager = CreateManager(1, 2, 3, 4, 5);

            var result = manager.Roll(UnknownRoomCode, FirstId, NoHeldDice);

            Assert.Equal(GameActionStatus.GameNotFound, result.Status);
        }

        [Fact]
        public void Roll_AfterRolling_ShowsTheBoxesThatTheCurrentPlayerCanChoose()
        {
            var manager = CreateManager(1, 2, 3, 4, 5);
            StartTwoPlayerGame(manager);

            var result = manager.Roll(RoomCode, FirstId, NoHeldDice);

            Assert.Equal(12, result.Snapshot.Options.Count);
            Assert.Equal(30, result.Snapshot.Options.Single(o => o.Category == ScoreCategory.LittleStraight).Points);
        }

        [Fact]
        public void Score_BeforeRolling_ReturnsMustRollFirst()
        {
            var manager = CreateManager(1, 2, 3, 4, 5);
            StartTwoPlayerGame(manager);

            var result = manager.Score(RoomCode, FirstId, ScoreCategory.Choice);

            Assert.Equal(GameActionStatus.MustRollFirst, result.Status);
        }

        [Fact]
        public void Score_AfterRolling_WritesThePointsAndPassesTheTurn()
        {
            var manager = CreateManager(1, 2, 3, 4, 5);
            StartTwoPlayerGame(manager);
            manager.Roll(RoomCode, FirstId, NoHeldDice);

            var result = manager.Score(RoomCode, FirstId, ScoreCategory.LittleStraight);

            Assert.Equal(GameActionStatus.Success, result.Status);
            Assert.Equal(SecondId, result.Snapshot.CurrentPlayerId);
            Assert.Equal(3, result.Snapshot.RollsLeft);
            Assert.Empty(result.Snapshot.Dice);
            Assert.Equal(30, FindPlayer(result.Snapshot, FirstId).Total);
        }

        [Fact]
        public void Score_PlayerWhoseTurnIsNotNow_ReturnsNotYourTurn()
        {
            var manager = CreateManager(1, 2, 3, 4, 5);
            StartTwoPlayerGame(manager);
            manager.Roll(RoomCode, FirstId, NoHeldDice);

            var result = manager.Score(RoomCode, SecondId, ScoreCategory.Choice);

            Assert.Equal(GameActionStatus.NotYourTurn, result.Status);
        }

        [Fact]
        public void Score_CategoryAlreadyUsed_ReturnsCategoryFilledAndKeepsTheTurn()
        {
            var manager = CreateManager(1, 2, 3, 4, 5);
            StartTwoPlayerGame(manager);
            PlayTurn(manager, FirstId, ScoreCategory.Choice);
            PlayTurn(manager, SecondId, ScoreCategory.Choice);
            manager.Roll(RoomCode, FirstId, NoHeldDice);

            var result = manager.Score(RoomCode, FirstId, ScoreCategory.Choice);
            var state = manager.GetState(RoomCode, FirstId);

            Assert.Equal(GameActionStatus.CategoryFilled, result.Status);
            Assert.Equal(FirstId, state.Snapshot.CurrentPlayerId);
        }

        [Fact]
        public void Score_EveryPlayerPlayedOnce_StartsTheSecondRound()
        {
            var manager = CreateManager(1, 2, 3, 4, 5);
            StartTwoPlayerGame(manager);
            PlayTurn(manager, FirstId, ScoreCategory.Choice);

            var result = PlayTurn(manager, SecondId, ScoreCategory.Choice);

            Assert.Equal(FirstId, result.Snapshot.CurrentPlayerId);
            Assert.Equal(2, result.Snapshot.Round);
        }

        [Fact]
        public void Score_LastBoxOfTheOnlyPlayer_FinishesTheGameWithAWinner()
        {
            var manager = CreateManager(1, 2, 3, 4, 5);
            manager.Start(RoomCode, new[] { new GameParticipant(FirstId, "Emilio") }, GameSpeed.Normal);

            foreach (var category in ScoreCategories.All)
            {
                PlayTurn(manager, FirstId, category);
            }

            var snapshot = manager.GetState(RoomCode, FirstId).Snapshot;
            Assert.Equal(GameStatus.Finished, snapshot.Status);
            Assert.Equal(string.Empty, snapshot.CurrentPlayerId);
            Assert.True(FindPlayer(snapshot, FirstId).IsWinner);
        }

        [Fact]
        public void Score_FinishedGame_RejectsNewActions()
        {
            var manager = CreateManager(1, 2, 3, 4, 5);
            manager.Start(RoomCode, new[] { new GameParticipant(FirstId, "Emilio") }, GameSpeed.Normal);
            foreach (var category in ScoreCategories.All)
            {
                PlayTurn(manager, FirstId, category);
            }

            var result = manager.Roll(RoomCode, FirstId, NoHeldDice);

            Assert.Equal(GameActionStatus.GameFinished, result.Status);
        }

        [Fact]
        public void Score_TwoPlayersWithTheSameTotal_MarksBothAsWinners()
        {
            var manager = CreateManager(1, 2, 3, 4, 5);
            StartTwoPlayerGame(manager);
            var lastResult = PlayTwoPlayerGameToTheEnd(manager);

            Assert.Equal(GameStatus.Finished, lastResult.Snapshot.Status);
            Assert.True(lastResult.Snapshot.Players.All(player => player.IsWinner));
        }

        [Fact]
        public void MarkAbsent_PlayerWithTheTurn_PassesTheTurnToTheNextPlayer()
        {
            var manager = CreateManager(1, 2, 3, 4, 5);
            StartTwoPlayerGame(manager);
            manager.Roll(RoomCode, FirstId, NoHeldDice);

            var result = manager.MarkAbsent(RoomCode, FirstId);

            Assert.Equal(SecondId, result.Snapshot.CurrentPlayerId);
            Assert.Empty(result.Snapshot.Dice);
            Assert.False(FindPlayer(result.Snapshot, FirstId).IsPresent);
        }

        [Fact]
        public void Score_NextPlayerIsAbsent_SkipsTheAbsentPlayer()
        {
            var manager = CreateManager(1, 2, 3, 4, 5);
            StartTwoPlayerGame(manager);
            manager.MarkAbsent(RoomCode, SecondId);
            manager.Roll(RoomCode, FirstId, NoHeldDice);

            var result = manager.Score(RoomCode, FirstId, ScoreCategory.Choice);

            Assert.Equal(FirstId, result.Snapshot.CurrentPlayerId);
        }

        [Fact]
        public void MarkPresent_PlayerWhoCameBack_GetsTurnsAgain()
        {
            var manager = CreateManager(1, 2, 3, 4, 5);
            StartTwoPlayerGame(manager);
            manager.MarkAbsent(RoomCode, SecondId);
            manager.MarkPresent(RoomCode, SecondId);
            manager.Roll(RoomCode, FirstId, NoHeldDice);

            var result = manager.Score(RoomCode, FirstId, ScoreCategory.Choice);

            Assert.Equal(SecondId, result.Snapshot.CurrentPlayerId);
        }

        [Fact]
        public void MarkAbsent_EveryPlayer_PausesTheGameUntilSomeoneReturns()
        {
            var manager = CreateManager(1, 2, 3, 4, 5);
            StartTwoPlayerGame(manager);
            manager.MarkAbsent(RoomCode, FirstId);

            var paused = manager.MarkAbsent(RoomCode, SecondId);
            var resumed = manager.MarkPresent(RoomCode, SecondId);

            Assert.Equal(string.Empty, paused.Snapshot.CurrentPlayerId);
            Assert.Equal(GameStatus.InProgress, paused.Snapshot.Status);
            Assert.Equal(SecondId, resumed.Snapshot.CurrentPlayerId);
        }

        [Fact]
        public void MarkAbsent_UnknownPlayer_ReturnsPlayerNotInGame()
        {
            var manager = CreateManager(1, 2, 3, 4, 5);
            StartTwoPlayerGame(manager);

            var result = manager.MarkAbsent(RoomCode, UnknownId);

            Assert.Equal(GameActionStatus.PlayerNotInGame, result.Status);
        }

        [Fact]
        public void Remove_PlayerWithTheTurn_PassesTheTurnAndDropsThePlayer()
        {
            var manager = CreateManager(1, 2, 3, 4, 5);
            StartTwoPlayerGame(manager);

            var result = manager.Remove(RoomCode, FirstId);

            Assert.Equal(SecondId, result.Snapshot.CurrentPlayerId);
            Assert.Equal(1, result.Snapshot.Players.Count);
        }

        [Fact]
        public void Remove_PlayerWithoutTheTurn_KeepsTheTurnOfTheCurrentPlayer()
        {
            var manager = CreateManager(1, 2, 3, 4, 5);
            StartTwoPlayerGame(manager);

            var result = manager.Remove(RoomCode, SecondId);

            Assert.Equal(FirstId, result.Snapshot.CurrentPlayerId);
            Assert.Equal(1, result.Snapshot.Players.Count);
        }

        [Fact]
        public void AutoPlay_PlayerDidNotRoll_RollsAndChoosesTheBestBox()
        {
            var manager = CreateManager(1, 2, 3, 4, 5);
            var turnNumber = StartTwoPlayerGame(manager).TurnNumber;

            var result = manager.AutoPlay(RoomCode, turnNumber);

            Assert.Equal(GameActionStatus.Success, result.Status);
            Assert.Equal(SecondId, result.Snapshot.CurrentPlayerId);
            Assert.Equal(30, FindPlayer(result.Snapshot, FirstId).Total);
        }

        [Fact]
        public void AutoPlay_TurnAlreadyChanged_DoesNothing()
        {
            var manager = CreateManager(1, 2, 3, 4, 5);
            var turnNumber = StartTwoPlayerGame(manager).TurnNumber;
            PlayTurn(manager, FirstId, ScoreCategory.Choice);

            var result = manager.AutoPlay(RoomCode, turnNumber);

            Assert.Equal(GameActionStatus.NotYourTurn, result.Status);
            Assert.Equal(SecondId, manager.GetState(RoomCode, SecondId).Snapshot.CurrentPlayerId);
        }

        [Fact]
        public void GetState_PlayerOutsideTheGame_ReturnsPlayerNotInGame()
        {
            var manager = CreateManager(1, 2, 3, 4, 5);
            StartTwoPlayerGame(manager);

            var result = manager.GetState(RoomCode, UnknownId);

            Assert.Equal(GameActionStatus.PlayerNotInGame, result.Status);
        }

        [Fact]
        public void End_RoomWithGame_RemovesTheGame()
        {
            var manager = CreateManager(1, 2, 3, 4, 5);
            StartTwoPlayerGame(manager);

            manager.End(RoomCode);

            Assert.Equal(GameActionStatus.GameNotFound, manager.GetState(RoomCode, FirstId).Status);
        }

        private static GameManager CreateManager(params int[] dieValues)
        {
            return new GameManager(new InMemoryDieSource(dieValues));
        }

        private static GameParticipant[] CreateParticipants()
        {
            return new[] { new GameParticipant(FirstId, "Emilio"), new GameParticipant(SecondId, "Gabriel") };
        }

        private static GameSnapshot StartTwoPlayerGame(GameManager manager)
        {
            return manager.Start(RoomCode, CreateParticipants(), GameSpeed.Normal);
        }

        private static PlayerSnapshot FindPlayer(GameSnapshot snapshot, string playerId)
        {
            return snapshot.Players.Single(player => player.PlayerId == playerId);
        }

        private static GameActionResult PlayTurn(GameManager manager, string playerId, ScoreCategory category)
        {
            manager.Roll(RoomCode, playerId, NoHeldDice);

            return manager.Score(RoomCode, playerId, category);
        }

        private static GameActionResult PlayTwoPlayerGameToTheEnd(GameManager manager)
        {
            var result = GameActionResult.CreateFailed(GameActionStatus.GameNotFound);

            foreach (var category in ScoreCategories.All)
            {
                PlayTurn(manager, FirstId, category);
                result = PlayTurn(manager, SecondId, category);
            }

            return result;
        }
    }
}
