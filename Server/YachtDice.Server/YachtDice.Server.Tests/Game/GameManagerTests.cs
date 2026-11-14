using System;
using System.Linq;
using System.Collections.Generic;
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
        private const int NormalTurnSeconds = 60;
        private const int FastTurnSeconds = 30;
        private const int NoExperience = 0;

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
            var expectedSnapshot = new
            {
                CurrentPlayerId = FirstId,
                Round = FirstRound,
                RollsLeft = RollsPerTurn,
                Status = GameStatus.InProgress,
                Dice = NoDice
            };

            GameSnapshot snapshot = StartTwoPlayerGame(manager);

            Assert.Equivalent(expectedSnapshot, snapshot);
        }

        [Theory]
        [InlineData(GameSpeed.Normal, NormalTurnSeconds)]
        [InlineData(GameSpeed.Fast, FastTurnSeconds)]
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
            var expectedRollResult = new
            {
                Status = GameActionStatus.Success,
                Snapshot = new { Dice = LittleStraightDice, RollsLeft = TwoRollsLeft }
            };

            GameActionResult rollResult = manager.Roll(new RoomMember(RoomCode, FirstId), NoHeldDice);

            Assert.Equivalent(expectedRollResult, rollResult);
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
            var expectedSnapshot = new
            {
                Dice = FirstThreeKeptWithTwoSixes,
                HeldDice = FirstThreeHeld,
                RollsLeft = OneRollLeft
            };

            GameActionResult rollResult = manager.Roll(new RoomMember(RoomCode, FirstId), FirstThreeHeld);

            Assert.Equivalent(expectedSnapshot, rollResult.Snapshot);
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
            var expectedOptions = new { OptionCount = CategoryCount, LittleStraightPoints };

            GameActionResult rollResult = manager.Roll(new RoomMember(RoomCode, FirstId), NoHeldDice);

            var actualOptions = new
            {
                OptionCount = rollResult.Snapshot.Options.Count,
                LittleStraightPoints = FindOption(rollResult.Snapshot, ScoreCategory.LittleStraight).Points
            };
            Assert.Equal(expectedOptions, actualOptions);
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
            var expectedScore = new
            {
                Status = GameActionStatus.Success,
                CurrentPlayerId = SecondId,
                RollsLeft = RollsPerTurn,
                Dice = NoDice,
                FirstPlayerTotal = LittleStraightPoints
            };

            GameActionResult scoreResult =
                manager.Score(new RoomMember(RoomCode, FirstId), ScoreCategory.LittleStraight);

            var actualScore = new
            {
                scoreResult.Status,
                scoreResult.Snapshot.CurrentPlayerId,
                scoreResult.Snapshot.RollsLeft,
                scoreResult.Snapshot.Dice,
                FirstPlayerTotal = FindPlayer(scoreResult.Snapshot, FirstId).Total
            };
            Assert.Equivalent(expectedScore, actualScore);
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
            var expectedSnapshot = new { CurrentPlayerId = FirstId, Round = SecondRound };

            GameActionResult scoreResult = PlayTurn(manager, SecondId, ScoreCategory.Choice);

            Assert.Equivalent(expectedSnapshot, scoreResult.Snapshot);
        }

        [Fact]
        public void Score_LastBoxOfTheOnlyPlayer_FinishesTheGameWithAWinner()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartSinglePlayerGame(manager);
            var expectedGameEnd = new { Status = GameStatus.Finished, CurrentPlayerId = string.Empty, IsWinner = true };

            PlayEveryCategory(manager);

            GameSnapshot snapshot = manager.GetState(new RoomMember(RoomCode, FirstId)).Snapshot;
            var actualGameEnd = new
            {
                snapshot.Status,
                snapshot.CurrentPlayerId,
                FindPlayer(snapshot, FirstId).IsWinner
            };
            Assert.Equal(expectedGameEnd, actualGameEnd);
        }

        [Fact]
        public void Score_LastBoxOfTheOnlyPlayer_GivesTheWinnerExperience()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartSinglePlayerGame(manager);
            PlayEveryCategory(manager);

            PlayerSnapshot winner = FindPlayer(manager.GetState(new RoomMember(RoomCode, FirstId)).Snapshot, FirstId);

            int expectedExperienceGained = ExperienceRules.WinnerBaseExperience + winner.Total;
            Assert.Equal(expectedExperienceGained, winner.ExperienceGained);
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
            var expectedGameEnd = new { Status = GameStatus.Finished, AreAllWinners = true };

            GameActionResult lastResult = PlayTwoPlayerGameToTheEnd(manager);

            var actualGameEnd = new
            {
                lastResult.Snapshot.Status,
                AreAllWinners =
                    lastResult.Snapshot.Players.All(player => player.IsWinner)
            };
            Assert.Equal(expectedGameEnd, actualGameEnd);
        }

        [Fact]
        public void MarkAbsent_PlayerWithTheTurn_PassesTheTurnToTheNextPlayer()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            manager.Roll(new RoomMember(RoomCode, FirstId), NoHeldDice);
            var expectedTurnChange = new { CurrentPlayerId = SecondId, Dice = NoDice, IsPresent = false };

            GameActionResult absenceResult = manager.MarkAbsent(new RoomMember(RoomCode, FirstId));

            var actualTurnChange = new
            {
                absenceResult.Snapshot.CurrentPlayerId,
                absenceResult.Snapshot.Dice,
                FindPlayer(absenceResult.Snapshot, FirstId).IsPresent
            };
            Assert.Equivalent(expectedTurnChange, actualTurnChange);
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
            var expectedSnapshot = new { CurrentPlayerId = string.Empty, Status = GameStatus.InProgress };

            GameActionResult paused = manager.MarkAbsent(new RoomMember(RoomCode, SecondId));

            Assert.Equivalent(expectedSnapshot, paused.Snapshot);
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
        public void Remove_PlayerWithTheTurn_PassesTheTurnAndDropsThePlayer()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            var expectedTurnChange = new { CurrentPlayerId = SecondId, PlayerCount = OnePlayerLeft };

            GameActionResult removalResult = manager.Remove(new RoomMember(RoomCode, FirstId));

            var actualTurnChange = new
            {
                removalResult.Snapshot.CurrentPlayerId,
                PlayerCount = removalResult.Snapshot.Players.Count
            };
            Assert.Equal(expectedTurnChange, actualTurnChange);
        }

        [Fact]
        public void Remove_PlayerWithoutTheTurn_KeepsTheTurnOfTheCurrentPlayer()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            StartTwoPlayerGame(manager);
            var expectedTurnChange = new { CurrentPlayerId = FirstId, PlayerCount = OnePlayerLeft };

            GameActionResult removalResult = manager.Remove(new RoomMember(RoomCode, SecondId));

            var actualTurnChange = new
            {
                removalResult.Snapshot.CurrentPlayerId,
                PlayerCount = removalResult.Snapshot.Players.Count
            };
            Assert.Equal(expectedTurnChange, actualTurnChange);
        }

        [Fact]
        public void AutoPlay_PlayerDidNotRoll_RollsAndChoosesTheBestBox()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            int turnNumber = StartTwoPlayerGame(manager).TurnNumber;
            var expectedAutoPlay = new
            {
                Status = GameActionStatus.Success,
                CurrentPlayerId = SecondId,
                FirstPlayerTotal = LittleStraightPoints
            };

            GameActionResult autoPlayResult = manager.AutoPlay(new GameTurn(RoomCode, turnNumber));

            var actualAutoPlay = new
            {
                autoPlayResult.Status,
                autoPlayResult.Snapshot.CurrentPlayerId,
                FirstPlayerTotal = FindPlayer(autoPlayResult.Snapshot, FirstId).Total
            };
            Assert.Equal(expectedAutoPlay, actualAutoPlay);
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
            var expectedVersions = new { IsRollNewer = true, IsScoreNewer = true };

            GameSnapshot rolled = manager.Roll(new RoomMember(RoomCode, FirstId), NoHeldDice).Snapshot;
            GameSnapshot scored = manager.Score(new RoomMember(RoomCode, FirstId), ScoreCategory.Choice).Snapshot;

            var actualVersions = new
            {
                IsRollNewer = rolled.Version > started.Version,
                IsScoreNewer = scored.Version > rolled.Version
            };
            Assert.Equal(expectedVersions, actualVersions);
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

        private static GameManager CreateManager(int[] dieValues)
        {
            var secondsBySpeed = new Dictionary<GameSpeed, int>
            {
                { GameSpeed.Normal, NormalTurnSeconds },
                { GameSpeed.Fast, FastTurnSeconds }
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
