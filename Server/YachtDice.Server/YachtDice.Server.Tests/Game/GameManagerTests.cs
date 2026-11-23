using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using YachtDice.Server.Logic.Game;
using YachtDice.Server.Logic.Players;
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
        private const string ThirdId = "player-c";
        private const string ThirdName = "Carlos";
        private const string UnknownId = "player-x";
        private const string UnknownRoomCode = "NOROOM";
        private const int RollsPerTurn = 3;
        private const int CategoryCount = 12;
        private const int LittleStraightPoints = 30;
        private const int FirstRound = 1;
        private const int SecondRound = 2;
        private const int OneRollLeft = 1;
        private const int TwoRollsLeft = 2;
        private const int OnePlayerLeft = 1;
        private const int TwoPlayersLeft = 2;
        private const int LongerTurnSeconds = 60;
        private const int ShorterTurnSeconds = 30;
        private const int NoExperience = 0;
        private const int OneRollMade = 1;

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
            var snapshotExpected = new
            {
                CurrentPlayerId = FirstId,
                Round = FirstRound,
                RollsLeft = RollsPerTurn,
                Status = GameStatus.InProgress,
                Dice = NoDice
            };

            GameSnapshot snapshot = StartTwoPlayerGame(manager);

            Assert.Equivalent(snapshotExpected, snapshot);
        }

        [Theory]
        [InlineData(GameSpeed.Normal, LongerTurnSeconds)]
        [InlineData(GameSpeed.Fast, ShorterTurnSeconds)]
        public void Start_Speed_SetsTheSecondsOfTheTurn(GameSpeed speed, int secondsExpected)
        {
            GameManager manager = CreateManager(LittleStraightDice);

            GameSnapshot snapshot = manager.Start(RoomCode, CreateParticipants(), speed);

            Assert.Equal(secondsExpected, snapshot.TurnSeconds);
        }

        [Fact]
        public void Roll_FirstRollOfTheTurn_RollsTheFiveDiceAndLeavesTwoRolls()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            var rollResultExpected = new
            {
                Status = GameActionStatus.Success,
                Snapshot = new { Dice = LittleStraightDice, RollsLeft = TwoRollsLeft }
            };

            GameActionResult rollResult = manager.Roll(new RoomMember(RoomCode, FirstId), NoHeldDice);

            Assert.Equivalent(rollResultExpected, rollResult);
        }

        [Fact]
        public void Roll_FirstRollIgnoresTheHeldDice_RollsAllDice()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);

            GameActionResult rollResult = manager.Roll(new RoomMember(RoomCode, FirstId), FirstThreeHeld);

            Assert.Equal(LittleStraightDice, rollResult.Snapshot.Dice.ToArray());
        }

        [Fact]
        public void Roll_SecondRollWithHeldDice_KeepsTheHeldDiceAndRollsTheRest()
        {
            GameManager manager = CreateManager(LittleStraightThenTwoSixes);
            StartTwoPlayerGame(manager);
            manager.Roll(new RoomMember(RoomCode, FirstId), NoHeldDice);
            var snapshotExpected = new
            {
                Dice = FirstThreeKeptWithTwoSixes,
                HeldDice = FirstThreeHeld,
                RollsLeft = OneRollLeft
            };

            GameActionResult rollResult = manager.Roll(new RoomMember(RoomCode, FirstId), FirstThreeHeld);

            Assert.Equivalent(snapshotExpected, rollResult.Snapshot);
        }

        [Fact]
        public void Roll_FourthRollOfTheTurn_ReturnsNoRollsLeft()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            manager.Roll(new RoomMember(RoomCode, FirstId), NoHeldDice);
            manager.Roll(new RoomMember(RoomCode, FirstId), NoHeldDice);
            manager.Roll(new RoomMember(RoomCode, FirstId), NoHeldDice);

            GameActionResult rollResult = manager.Roll(new RoomMember(RoomCode, FirstId), NoHeldDice);

            Assert.Equal(GameActionStatus.NoRollsLeft, rollResult.Status);
        }

        [Fact]
        public void Roll_HeldDiceWithWrongLength_ReturnsInvalidHeldDice()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            manager.Roll(new RoomMember(RoomCode, FirstId), NoHeldDice);

            GameActionResult rollResult = manager.Roll(new RoomMember(RoomCode, FirstId), new[] { true, false });

            Assert.Equal(GameActionStatus.InvalidHeldDice, rollResult.Status);
        }

        [Fact]
        public void Roll_PlayerWhoseTurnIsNotNow_ReturnsNotYourTurn()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);

            GameActionResult rollResult = manager.Roll(new RoomMember(RoomCode, SecondId), NoHeldDice);

            Assert.Equal(GameActionStatus.NotYourTurn, rollResult.Status);
        }

        [Fact]
        public void Roll_PlayerOutsideTheGame_ReturnsPlayerNotInGame()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);

            GameActionResult rollResult = manager.Roll(new RoomMember(RoomCode, UnknownId), NoHeldDice);

            Assert.Equal(GameActionStatus.PlayerNotInGame, rollResult.Status);
        }

        [Fact]
        public void Roll_RoomWithoutGame_ReturnsGameNotFound()
        {
            GameManager manager = CreateManager(LittleStraightDice);

            GameActionResult rollResult = manager.Roll(new RoomMember(UnknownRoomCode, FirstId), NoHeldDice);

            Assert.Equal(GameActionStatus.GameNotFound, rollResult.Status);
        }

        [Fact]
        public void Roll_AfterRolling_ShowsTheBoxesThatTheCurrentPlayerCanChoose()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            var optionsExpected = new { OptionCount = CategoryCount, LittleStraightPoints };

            GameActionResult rollResult = manager.Roll(new RoomMember(RoomCode, FirstId), NoHeldDice);

            var optionsResult = new
            {
                OptionCount = rollResult.Snapshot.Options.Count,
                LittleStraightPoints = FindOption(rollResult.Snapshot, ScoreCategory.LittleStraight).Points
            };
            Assert.Equal(optionsExpected, optionsResult);
        }

        [Fact]
        public void Score_BeforeRolling_ReturnsMustRollFirst()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);

            GameActionResult scoreResult = manager.Score(new RoomMember(RoomCode, FirstId), ScoreCategory.Choice);

            Assert.Equal(GameActionStatus.MustRollFirst, scoreResult.Status);
        }

        [Fact]
        public void Score_AfterRolling_WritesThePointsAndPassesTheTurn()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            manager.Roll(new RoomMember(RoomCode, FirstId), NoHeldDice);
            var scoreExpected = new
            {
                Status = GameActionStatus.Success,
                CurrentPlayerId = SecondId,
                RollsLeft = RollsPerTurn,
                Dice = NoDice,
                FirstPlayerTotal = LittleStraightPoints
            };

            GameActionResult scoreAction =
                manager.Score(new RoomMember(RoomCode, FirstId), ScoreCategory.LittleStraight);

            var scoreResult = new
            {
                scoreAction.Status,
                scoreAction.Snapshot.CurrentPlayerId,
                scoreAction.Snapshot.RollsLeft,
                scoreAction.Snapshot.Dice,
                FirstPlayerTotal = FindPlayer(scoreAction.Snapshot, FirstId).Total
            };
            Assert.Equivalent(scoreExpected, scoreResult);
        }

        [Fact]
        public void Score_PlayerWhoseTurnIsNotNow_ReturnsNotYourTurn()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            manager.Roll(new RoomMember(RoomCode, FirstId), NoHeldDice);

            GameActionResult scoreResult = manager.Score(new RoomMember(RoomCode, SecondId), ScoreCategory.Choice);

            Assert.Equal(GameActionStatus.NotYourTurn, scoreResult.Status);
        }

        [Fact]
        public void Score_CategoryAlreadyUsed_ReturnsCategoryFilled()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            PlayTurn(manager, FirstId, ScoreCategory.Choice);
            PlayTurn(manager, SecondId, ScoreCategory.Choice);
            manager.Roll(new RoomMember(RoomCode, FirstId), NoHeldDice);

            GameActionResult scoreResult = manager.Score(new RoomMember(RoomCode, FirstId), ScoreCategory.Choice);

            Assert.Equal(GameActionStatus.CategoryFilled, scoreResult.Status);
        }

        [Fact]
        public void Score_CategoryAlreadyUsed_KeepsTheTurn()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            PlayTurn(manager, FirstId, ScoreCategory.Choice);
            PlayTurn(manager, SecondId, ScoreCategory.Choice);
            manager.Roll(new RoomMember(RoomCode, FirstId), NoHeldDice);
            manager.Score(new RoomMember(RoomCode, FirstId), ScoreCategory.Choice);

            GameActionResult state = manager.GetState(new RoomMember(RoomCode, FirstId));

            Assert.Equal(FirstId, state.Snapshot.CurrentPlayerId);
        }

        [Fact]
        public void Score_EveryPlayerPlayedOnce_StartsTheSecondRound()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            PlayTurn(manager, FirstId, ScoreCategory.Choice);
            var snapshotExpected = new { CurrentPlayerId = FirstId, Round = SecondRound };

            GameActionResult scoreResult = PlayTurn(manager, SecondId, ScoreCategory.Choice);

            Assert.Equivalent(snapshotExpected, scoreResult.Snapshot);
        }

        [Fact]
        public void Score_LastBoxOfTheOnlyPlayer_FinishesTheGameWithAWinner()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartSinglePlayerGame(manager);
            var gameEndExpected = new { Status = GameStatus.Finished, CurrentPlayerId = string.Empty, IsWinner = true };

            PlayEveryCategory(manager);

            GameSnapshot snapshot = manager.GetState(new RoomMember(RoomCode, FirstId)).Snapshot;
            var gameEndResult = new
            {
                snapshot.Status,
                snapshot.CurrentPlayerId,
                FindPlayer(snapshot, FirstId).IsWinner
            };
            Assert.Equal(gameEndExpected, gameEndResult);
        }

        [Fact]
        public void Score_LastBoxOfTheOnlyPlayer_GivesTheWinnerExperience()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartSinglePlayerGame(manager);
            PlayEveryCategory(manager);

            PlayerSnapshot winner = FindPlayer(manager.GetState(new RoomMember(RoomCode, FirstId)).Snapshot, FirstId);

            int experienceGainedExpected = LevelTable.WinnerBaseExperience + winner.Total;
            Assert.Equal(experienceGainedExpected, winner.ExperienceGained);
        }

        [Fact]
        public void Score_GameInProgress_GivesNoExperience()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);

            GameActionResult scoreResult = PlayTurn(manager, FirstId, ScoreCategory.Choice);

            PlayerSnapshot player = FindPlayer(scoreResult.Snapshot, FirstId);
            Assert.Equal(NoExperience, player.ExperienceGained);
        }

        [Fact]
        public void Score_FinishedGame_RejectsNewActions()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartSinglePlayerGame(manager);
            PlayEveryCategory(manager);

            GameActionResult scoreResult = manager.Roll(new RoomMember(RoomCode, FirstId), NoHeldDice);

            Assert.Equal(GameActionStatus.GameFinished, scoreResult.Status);
        }

        [Fact]
        public void Score_TwoPlayersWithTheSameTotal_MarksBothAsWinners()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            var gameEndExpected = new { Status = GameStatus.Finished, AreAllWinners = true };

            GameActionResult lastResult = PlayTwoPlayerGameToTheEnd(manager);

            var gameEndResult = new
            {
                lastResult.Snapshot.Status,
                AreAllWinners =
                    lastResult.Snapshot.Players.All(player => player.IsWinner)
            };
            Assert.Equal(gameEndExpected, gameEndResult);
        }

        [Fact]
        public void MarkAbsent_PlayerWithTheTurn_PassesTheTurnToTheNextPlayer()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            manager.Roll(new RoomMember(RoomCode, FirstId), NoHeldDice);
            var turnChangeExpected = new { CurrentPlayerId = SecondId, Dice = NoDice, IsPresent = false };

            GameActionResult absenceResult = manager.MarkAbsent(new RoomMember(RoomCode, FirstId));

            var turnChangeResult = new
            {
                absenceResult.Snapshot.CurrentPlayerId,
                absenceResult.Snapshot.Dice,
                FindPlayer(absenceResult.Snapshot, FirstId).IsPresent
            };
            Assert.Equivalent(turnChangeExpected, turnChangeResult);
        }

        [Fact]
        public void Score_NextPlayerIsAbsent_SkipsTheAbsentPlayer()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            manager.MarkAbsent(new RoomMember(RoomCode, SecondId));
            manager.Roll(new RoomMember(RoomCode, FirstId), NoHeldDice);

            GameActionResult scoreResult = manager.Score(new RoomMember(RoomCode, FirstId), ScoreCategory.Choice);

            Assert.Equal(FirstId, scoreResult.Snapshot.CurrentPlayerId);
        }

        [Fact]
        public void MarkPresent_PlayerWhoCameBack_GetsTurnsAgain()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            manager.MarkAbsent(new RoomMember(RoomCode, SecondId));
            manager.MarkPresent(new RoomMember(RoomCode, SecondId));
            manager.Roll(new RoomMember(RoomCode, FirstId), NoHeldDice);

            GameActionResult returnResult = manager.Score(new RoomMember(RoomCode, FirstId), ScoreCategory.Choice);

            Assert.Equal(SecondId, returnResult.Snapshot.CurrentPlayerId);
        }

        [Fact]
        public void MarkAbsent_EveryPlayer_PausesTheGame()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            manager.MarkAbsent(new RoomMember(RoomCode, FirstId));
            var snapshotExpected = new { CurrentPlayerId = string.Empty, Status = GameStatus.InProgress };

            GameActionResult paused = manager.MarkAbsent(new RoomMember(RoomCode, SecondId));

            Assert.Equivalent(snapshotExpected, paused.Snapshot);
        }

        [Fact]
        public void MarkPresent_PlayerReturnsToAPausedGame_GetsTheTurn()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            manager.MarkAbsent(new RoomMember(RoomCode, FirstId));
            manager.MarkAbsent(new RoomMember(RoomCode, SecondId));

            GameActionResult resumed = manager.MarkPresent(new RoomMember(RoomCode, SecondId));

            Assert.Equal(SecondId, resumed.Snapshot.CurrentPlayerId);
        }

        [Fact]
        public void MarkAbsent_UnknownPlayer_ReturnsPlayerNotInGame()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);

            GameActionResult absenceResult = manager.MarkAbsent(new RoomMember(RoomCode, UnknownId));

            Assert.Equal(GameActionStatus.PlayerNotInGame, absenceResult.Status);
        }

        [Fact]
        public void Forfeit_AbsentPlayerOfATwoPlayerGame_FinishesTheGameWithTheOtherPlayerAsWinner()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            manager.MarkAbsent(new RoomMember(RoomCode, SecondId));
            var gameEndExpected = new { Status = GameStatus.Finished, FirstWins = true, SecondWins = false };

            GameActionResult forfeitResult = manager.Forfeit(new RoomMember(RoomCode, SecondId));

            var gameEndResult = new
            {
                forfeitResult.Snapshot.Status,
                FirstWins = FindPlayer(forfeitResult.Snapshot, FirstId).IsWinner,
                SecondWins = FindPlayer(forfeitResult.Snapshot, SecondId).IsWinner
            };
            Assert.Equivalent(gameEndExpected, gameEndResult);
        }

        [Fact]
        public void Forfeit_AbsentPlayerOfATwoPlayerGame_GivesExperienceOnlyToTheOtherPlayer()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            manager.MarkAbsent(new RoomMember(RoomCode, SecondId));
            var experienceExpected = new { FirstGainsExperience = true, SecondExperience = NoExperience };

            GameActionResult forfeitResult = manager.Forfeit(new RoomMember(RoomCode, SecondId));

            var experienceResult = new
            {
                FirstGainsExperience = FindPlayer(forfeitResult.Snapshot, FirstId).ExperienceGained > NoExperience,
                SecondExperience = FindPlayer(forfeitResult.Snapshot, SecondId).ExperienceGained
            };
            Assert.Equivalent(experienceExpected, experienceResult);
        }

        [Fact]
        public void Forfeit_AbsentPlayerOfAThreePlayerGame_KeepsTheGameInProgress()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartThreePlayerGame(manager);
            manager.MarkAbsent(new RoomMember(RoomCode, ThirdId));

            GameActionResult forfeitResult = manager.Forfeit(new RoomMember(RoomCode, ThirdId));

            Assert.Equal(GameStatus.InProgress, forfeitResult.Snapshot.Status);
        }

        [Fact]
        public void Forfeit_TwoAbsentPlayersOfAThreePlayerGame_FinishesTheGameWithTheLastPlayerAsWinner()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartThreePlayerGame(manager);
            manager.MarkAbsent(new RoomMember(RoomCode, SecondId));
            manager.MarkAbsent(new RoomMember(RoomCode, ThirdId));
            manager.Forfeit(new RoomMember(RoomCode, SecondId));

            GameActionResult forfeitResult = manager.Forfeit(new RoomMember(RoomCode, ThirdId));

            Assert.True(FindPlayer(forfeitResult.Snapshot, FirstId).IsWinner);
        }

        [Fact]
        public void Forfeit_PlayerWhoIsPresent_KeepsThePlayerInTheGame()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);

            GameActionResult forfeitResult = manager.Forfeit(new RoomMember(RoomCode, SecondId));

            Assert.Equal(GameStatus.InProgress, forfeitResult.Snapshot.Status);
        }

        [Fact]
        public void Forfeit_UnknownPlayer_ReturnsPlayerNotInGame()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);

            GameActionResult forfeitResult = manager.Forfeit(new RoomMember(RoomCode, UnknownId));

            Assert.Equal(GameActionStatus.PlayerNotInGame, forfeitResult.Status);
        }

        [Fact]
        public void MarkPresent_PlayerWhoForfeited_StaysOutOfTheGame()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartThreePlayerGame(manager);
            manager.MarkAbsent(new RoomMember(RoomCode, ThirdId));
            manager.Forfeit(new RoomMember(RoomCode, ThirdId));

            GameActionResult returnResult = manager.MarkPresent(new RoomMember(RoomCode, ThirdId));

            Assert.Equal(GameActionStatus.PlayerNotInGame, returnResult.Status);
        }

        [Fact]
        public void Roll_PlayerWhoForfeited_ReturnsPlayerNotInGame()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartThreePlayerGame(manager);
            manager.MarkAbsent(new RoomMember(RoomCode, ThirdId));
            manager.Forfeit(new RoomMember(RoomCode, ThirdId));

            GameActionResult rollResult = manager.Roll(new RoomMember(RoomCode, ThirdId), NoHeldDice);

            Assert.Equal(GameActionStatus.PlayerNotInGame, rollResult.Status);
        }

        [Fact]
        public void Remove_PlayerWithTheTurn_PassesTheTurnAndDropsThePlayer()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartThreePlayerGame(manager);
            var turnChangeExpected = new { CurrentPlayerId = SecondId, PlayerCount = TwoPlayersLeft };

            GameActionResult removalResult = manager.Remove(new RoomMember(RoomCode, FirstId));

            var turnChangeResult = new
            {
                removalResult.Snapshot.CurrentPlayerId,
                PlayerCount = removalResult.Snapshot.Players.Count
            };
            Assert.Equal(turnChangeExpected, turnChangeResult);
        }

        [Fact]
        public void Remove_PlayerWithoutTheTurn_KeepsTheTurnOfTheCurrentPlayer()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartThreePlayerGame(manager);
            var turnChangeExpected = new { CurrentPlayerId = FirstId, PlayerCount = TwoPlayersLeft };

            GameActionResult removalResult = manager.Remove(new RoomMember(RoomCode, SecondId));

            var turnChangeResult = new
            {
                removalResult.Snapshot.CurrentPlayerId,
                PlayerCount = removalResult.Snapshot.Players.Count
            };
            Assert.Equal(turnChangeExpected, turnChangeResult);
        }

        [Fact]
        public void Remove_PlayerOfATwoPlayerGame_FinishesTheGameWithTheOtherPlayerAsWinner()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            var gameEndExpected = new { Status = GameStatus.Finished, PlayerCount = OnePlayerLeft, FirstWins = true };

            GameActionResult removalResult = manager.Remove(new RoomMember(RoomCode, SecondId));

            var gameEndResult = new
            {
                removalResult.Snapshot.Status,
                PlayerCount = removalResult.Snapshot.Players.Count,
                FirstWins = FindPlayer(removalResult.Snapshot, FirstId).IsWinner
            };
            Assert.Equivalent(gameEndExpected, gameEndResult);
        }

        [Fact]
        public void AutoPlay_PlayerDidNotRoll_RollsAndChoosesTheBestBox()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            int turnNumber = StartTwoPlayerGame(manager).TurnNumber;
            var autoPlayExpected = new
            {
                Status = GameActionStatus.Success,
                CurrentPlayerId = SecondId,
                FirstPlayerTotal = LittleStraightPoints
            };

            GameActionResult autoPlayAction = manager.AutoPlay(new GameTurn(RoomCode, turnNumber));

            var autoPlayResult = new
            {
                autoPlayAction.Status,
                autoPlayAction.Snapshot.CurrentPlayerId,
                FirstPlayerTotal = FindPlayer(autoPlayAction.Snapshot, FirstId).Total
            };
            Assert.Equal(autoPlayExpected, autoPlayResult);
        }

        [Fact]
        public void AutoPlay_NullTurn_ThrowsArgumentNullException()
        {
            GameManager manager = CreateManager(LittleStraightDice);

            Assert.Throws<ArgumentNullException>(() => manager.AutoPlay(null));
        }

        [Fact]
        public void AutoPlay_TurnAlreadyChanged_ReturnsNotYourTurn()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            int turnNumber = StartTwoPlayerGame(manager).TurnNumber;
            PlayTurn(manager, FirstId, ScoreCategory.Choice);

            GameActionResult autoPlayResult = manager.AutoPlay(new GameTurn(RoomCode, turnNumber));

            Assert.Equal(GameActionStatus.NotYourTurn, autoPlayResult.Status);
        }

        [Fact]
        public void AutoPlay_TurnAlreadyChanged_KeepsTheTurnOfTheNextPlayer()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            int turnNumber = StartTwoPlayerGame(manager).TurnNumber;
            PlayTurn(manager, FirstId, ScoreCategory.Choice);
            manager.AutoPlay(new GameTurn(RoomCode, turnNumber));

            GameActionResult state = manager.GetState(new RoomMember(RoomCode, SecondId));

            Assert.Equal(SecondId, state.Snapshot.CurrentPlayerId);
        }

        [Fact]
        public void Roll_AfterEachChange_GivesAHigherVersionToTheSnapshot()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            GameSnapshot started = StartTwoPlayerGame(manager);
            var versionsExpected = new { IsRollNewer = true, IsScoreNewer = true };

            GameSnapshot rolled = manager.Roll(new RoomMember(RoomCode, FirstId), NoHeldDice).Snapshot;
            GameSnapshot scored = manager.Score(new RoomMember(RoomCode, FirstId), ScoreCategory.Choice).Snapshot;

            var versionsResult = new
            {
                IsRollNewer = rolled.Version > started.Version,
                IsScoreNewer = scored.Version > rolled.Version
            };
            Assert.Equal(versionsExpected, versionsResult);
        }

        [Fact]
        public void GetState_PlayerOutsideTheGame_ReturnsPlayerNotInGame()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);

            GameActionResult stateResult = manager.GetState(new RoomMember(RoomCode, UnknownId));

            Assert.Equal(GameActionStatus.PlayerNotInGame, stateResult.Status);
        }

        [Fact]
        public void End_RoomWithGame_RemovesTheGame()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            manager.End(RoomCode);

            GameActionResult state = manager.GetState(new RoomMember(RoomCode, FirstId));

            Assert.Equal(GameActionStatus.GameNotFound, state.Status);
        }

        [Fact]
        public void Restore_SavedGame_StartsWithEveryPlayerAbsent()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            var playersExpected = new[]
            {
                new { PlayerId = FirstId, IsPresent = false },
                new { PlayerId = SecondId, IsPresent = false }
            };

            GameSnapshot snapshot = manager.Restore(CreateSavedGame());

            Assert.Equivalent(playersExpected, snapshot.Players);
        }

        [Fact]
        public void Restore_SavedScores_KeepsTheTotalOfThePlayer()
        {
            GameManager manager = CreateManager(LittleStraightDice);

            GameSnapshot snapshot = manager.Restore(CreateSavedGame());

            Assert.Equal(LittleStraightPoints, snapshot.Players.First().Total);
        }

        [Fact]
        public void Restore_PlayerWithTheTurnComesBack_KeepsTheDiceAndTheRollsLeft()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            manager.Restore(CreateSavedGame());
            var snapshotExpected = new
            {
                CurrentPlayerId = FirstId,
                RollsLeft = TwoRollsLeft,
                Dice = LittleStraightDice
            };

            GameActionResult markPresentResult = manager.MarkPresent(new RoomMember(RoomCode, FirstId));

            Assert.Equivalent(snapshotExpected, markPresentResult.Snapshot);
        }

        [Fact]
        public void Restore_OtherPlayerComesBackFirst_GivesTheTurnToThatPlayer()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            manager.Restore(CreateSavedGame());

            GameActionResult markPresentResult = manager.MarkPresent(new RoomMember(RoomCode, SecondId));

            Assert.Equal(SecondId, markPresentResult.Snapshot.CurrentPlayerId);
        }

        [Fact]
        public void Restore_NullSavedGame_ThrowsArgumentNullException()
        {
            GameManager manager = CreateManager(LittleStraightDice);

            Assert.Throws<ArgumentNullException>(() => manager.Restore(null));
        }

        private static SavedGame CreateSavedGame()
        {
            var saved = new SavedGame
            {
                RoomCode = RoomCode,
                Settings = new RoomSettings { Speed = GameSpeed.Normal },
                CurrentPlayerName = FirstName,
                RollsMade = OneRollMade
            };
            var firstPlayer = new SavedPlayer { PlayerId = FirstId, Name = FirstName };

            firstPlayer.Scores.Add(new ScoreEntry(ScoreCategory.LittleStraight, true, LittleStraightPoints));
            saved.Players.Add(firstPlayer);
            saved.Players.Add(new SavedPlayer { PlayerId = SecondId, Name = SecondName });
            saved.Dice.AddRange(LittleStraightDice);
            saved.HeldDice.AddRange(NoHeldDice);

            return saved;
        }

        private static GameManager CreateManager(int[] dieValues)
        {
            var secondsBySpeed = new Dictionary<GameSpeed, int>
            {
                { GameSpeed.Normal, LongerTurnSeconds },
                { GameSpeed.Fast, ShorterTurnSeconds }
            };

            return new GameManager(new InMemoryDieRoller(dieValues), new TurnDuration(secondsBySpeed));
        }

        private static GameParticipant[] CreateParticipants()
        {
            return new[] { new GameParticipant(FirstId, FirstName), new GameParticipant(SecondId, SecondName) };
        }

        private static GameSnapshot StartTwoPlayerGame(GameManager manager)
        {
            return manager.Start(RoomCode, CreateParticipants(), GameSpeed.Normal);
        }

        private static GameSnapshot StartThreePlayerGame(GameManager manager)
        {
            var participants = new[]
            {
                new GameParticipant(FirstId, FirstName),
                new GameParticipant(SecondId, SecondName),
                new GameParticipant(ThirdId, ThirdName)
            };

            return manager.Start(RoomCode, participants, GameSpeed.Normal);
        }

        private static GameSnapshot StartSinglePlayerGame(GameManager manager)
        {
            return manager.Start(RoomCode, new[] { new GameParticipant(FirstId, FirstName) }, GameSpeed.Normal);
        }

        private static PlayerSnapshot FindPlayer(GameSnapshot snapshot, string playerId)
        {
            Func<PlayerSnapshot, bool> isSamePlayerId = player => player.PlayerId == playerId;

            return snapshot.Players.Single(isSamePlayerId);
        }

        private static ScoreEntry FindOption(GameSnapshot snapshot, ScoreCategory category)
        {
            Func<ScoreEntry, bool> isSameCategory = option => option.Category == category;

            return snapshot.Options.Single(isSameCategory);
        }

        private static GameActionResult PlayTurn(GameManager manager, string playerId, ScoreCategory category)
        {
            manager.Roll(new RoomMember(RoomCode, playerId), NoHeldDice);

            return manager.Score(new RoomMember(RoomCode, playerId), category);
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
            GameActionResult actionResult = GameActionResult.CreateFailed(GameActionStatus.GameNotFound);

            foreach (ScoreCategory category in ScoreCategories.All)
            {
                PlayTurn(manager, FirstId, category);
                actionResult = PlayTurn(manager, SecondId, category);
            }

            return actionResult;
        }
    }
}
