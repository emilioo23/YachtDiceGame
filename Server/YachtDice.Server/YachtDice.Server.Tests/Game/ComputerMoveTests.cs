using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using YachtDice.Server.Logic.Game;
using YachtDice.Server.Logic.Rooms;
using YachtDice.Server.Tests.InMemory;

namespace YachtDice.Server.Tests.Game
{
    /// <summary>
    /// Moves of a computer player: it rolls, keeps the dice of the value that repeats the most, stops when
    /// the dice are worth keeping and writes the points in the best box.
    /// </summary>
    public class ComputerMoveTests
    {
        private const string RoomCode = "ROOM01";
        private const string ComputerId = "computer-1";
        private const string ComputerName = "CPU";
        private const string PersonId = "player-a";
        private const string PersonName = "Gabriel";
        private const int TurnSeconds = 60;
        private const int TwoRollsLeft = 2;
        private const int OneRollLeft = 1;
        private const int LittleStraightPoints = 30;
        private const int MovesOfAFullTurn = 4;

        private static readonly int[] LittleStraightDice = { 1, 2, 3, 4, 5 };
        private static readonly int[] PairOfSixesThenThreeSixes = { 6, 6, 2, 3, 1, 6, 6, 6 };
        private static readonly int[] YachtOfSixes = { 6, 6, 6, 6, 6 };
        private static readonly int[] TwosAndThrees = { 2, 3 };
        private static readonly bool[] SixesHeld = { true, true, false, false, false };

        [Fact]
        public void Start_ComputerPlaysFirst_MarksTheTurnAsComputerTurn()
        {
            GameManager manager = CreateManager(LittleStraightDice);

            GameSnapshot startResult = StartWithComputerFirst(manager);

            Assert.True(startResult.IsComputerTurn);
        }

        [Fact]
        public void PlayComputerMove_FirstMoveOfTheTurn_RollsTheFiveDice()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            int turnNumber = StartWithComputerFirst(manager).TurnNumber;
            var moveExpected = new
            {
                Status = GameActionStatus.Success,
                Snapshot = new { Dice = LittleStraightDice, RollsLeft = TwoRollsLeft }
            };

            GameActionResult moveResult = manager.PlayComputerMove(new GameTurn(RoomCode, turnNumber));

            Assert.Equivalent(moveExpected, moveResult);
        }

        [Fact]
        public void PlayComputerMove_PairOfSixes_RollsAgainKeepingTheSixes()
        {
            GameManager manager = CreateManager(PairOfSixesThenThreeSixes);
            int turnNumber = StartWithComputerFirst(manager).TurnNumber;
            var turn = new GameTurn(RoomCode, turnNumber);
            manager.PlayComputerMove(turn);
            var snapshotExpected = new { Dice = YachtOfSixes, HeldDice = SixesHeld, RollsLeft = OneRollLeft };

            GameSnapshot snapshotResult = manager.PlayComputerMove(turn).Snapshot;

            Assert.Equivalent(snapshotExpected, snapshotResult);
        }

        [Fact]
        public void PlayComputerMove_LittleStraight_WritesItWithoutRollingAgain()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            int turnNumber = StartWithComputerFirst(manager).TurnNumber;
            var turn = new GameTurn(RoomCode, turnNumber);
            manager.PlayComputerMove(turn);
            var scoreExpected = new { CurrentPlayerId = PersonId, ComputerTotal = LittleStraightPoints };

            GameSnapshot snapshot = manager.PlayComputerMove(turn).Snapshot;

            var scoreResult = new { snapshot.CurrentPlayerId, ComputerTotal = FindComputer(snapshot).Total };
            Assert.Equal(scoreExpected, scoreResult);
        }

        [Fact]
        public void PlayComputerMove_NoRollsLeft_PassesTheTurnToThePerson()
        {
            GameManager manager = CreateManager(TwosAndThrees);
            int turnNumber = StartWithComputerFirst(manager).TurnNumber;
            var turn = new GameTurn(RoomCode, turnNumber);
            GameActionResult moveResult = GameActionResult.CreateFailed(GameActionStatus.GameNotFound);

            for (int move = 0; move < MovesOfAFullTurn; move++)
            {
                moveResult = manager.PlayComputerMove(turn);
            }

            Assert.Equal(PersonId, moveResult.Snapshot.CurrentPlayerId);
        }

        [Fact]
        public void PlayComputerMove_EasyComputerAfterTheFirstRoll_PassesTheTurnToThePerson()
        {
            GameManager manager = CreateManager(PairOfSixesThenThreeSixes);
            var easyComputer = new GameParticipant(ComputerId, ComputerName)
            {
                IsComputer = true,
                Difficulty = ComputerDifficulty.Easy
            };
            var participants = new[] { easyComputer, CreatePerson() };
            var turn = new GameTurn(RoomCode, manager.Start(RoomCode, participants, GameSpeed.Normal).TurnNumber);
            manager.PlayComputerMove(turn);

            GameActionResult moveResult = manager.PlayComputerMove(turn);

            Assert.Equal(PersonId, moveResult.Snapshot.CurrentPlayerId);
        }

        [Fact]
        public void PlayComputerMove_TurnOfAPerson_ReturnsNotYourTurn()
        {
            GameManager manager = CreateManager(LittleStraightDice);
            var participants = new[] { CreatePerson(), CreateComputer() };
            int turnNumber = manager.Start(RoomCode, participants, GameSpeed.Normal).TurnNumber;

            GameActionResult moveResult = manager.PlayComputerMove(new GameTurn(RoomCode, turnNumber));

            Assert.Equal(GameActionStatus.NotYourTurn, moveResult.Status);
        }

        [Fact]
        public void PlayComputerMove_NullTurn_ThrowsArgumentNullException()
        {
            GameManager manager = CreateManager(LittleStraightDice);

            Assert.Throws<ArgumentNullException>(() => manager.PlayComputerMove(null));
        }

        private static GameManager CreateManager(int[] dieValues)
        {
            var secondsBySpeed = new Dictionary<GameSpeed, int>
            {
                { GameSpeed.Normal, TurnSeconds },
                { GameSpeed.Fast, TurnSeconds }
            };

            return new GameManager(new InMemoryDieRoller(dieValues), new TurnDuration(secondsBySpeed));
        }

        private static GameParticipant CreateComputer()
        {
            return new GameParticipant(ComputerId, ComputerName) { IsComputer = true };
        }

        private static GameParticipant CreatePerson()
        {
            return new GameParticipant(PersonId, PersonName);
        }

        private static GameSnapshot StartWithComputerFirst(GameManager manager)
        {
            return manager.Start(RoomCode, new[] { CreateComputer(), CreatePerson() }, GameSpeed.Normal);
        }

        private static PlayerSnapshot FindComputer(GameSnapshot snapshot)
        {
            Func<PlayerSnapshot, bool> isComputer = player => player.PlayerId == ComputerId;

            return snapshot.Players.Single(isComputer);
        }
    }
}
