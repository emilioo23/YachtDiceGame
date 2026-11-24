using Xunit;
using YachtDice.Server.Services.Mapping;
using GameContracts = YachtDice.Server.Contracts.Game;
using LogicGame = YachtDice.Server.Logic.Game;

namespace YachtDice.Server.Tests.Mapping
{
    /// <summary>
    /// The state of a game turned into the contract that the clients receive.
    /// </summary>
    public class GameMapperTests
    {
        private const string PlayerId = "player-1";
        private const string PlayerName = "gabrielhernandez";
        private const int ExperienceGained = 230;
        private const int Total = 130;
        private const int SecondsLeft = 42;

        [Fact]
        public void ToContract_FinishedGame_KeepsTheExperienceAndTheTotalOfThePlayer()
        {
            var snapshot = new LogicGame.GameSnapshot { IsFinished = true };
            var player = new LogicGame.PlayerSnapshot(PlayerId, PlayerName, true)
            {
                Total = Total,
                IsWinner = true,
                ExperienceGained = ExperienceGained
            };
            snapshot.Players.Add(player);
            var playerStateExpected = new { Total, IsWinner = true, ExperienceGained };

            GameContracts.GameState state = GameMapper.ToContract(snapshot, SecondsLeft);

            Assert.Equivalent(playerStateExpected, state.Players[0]);
        }

        [Fact]
        public void ToContract_ResultNotSaved_TellsTheClient()
        {
            var snapshot = new LogicGame.GameSnapshot { IsFinished = true };
            snapshot.MarkResultNotSaved();

            GameContracts.GameState state = GameMapper.ToContract(snapshot, SecondsLeft);

            Assert.True(state.HasResultSaveFailed);
        }

        [Fact]
        public void ToContract_AnyGame_KeepsTheSecondsLeftOfTheTurn()
        {
            GameContracts.GameState state = GameMapper.ToContract(new LogicGame.GameSnapshot(), SecondsLeft);

            Assert.Equal(SecondsLeft, state.TurnSecondsLeft);
        }

        [Fact]
        public void CreateEmptyState_Always_ReturnsAStateWithoutPlayers()
        {
            GameContracts.GameState state = GameMapper.CreateEmptyState();

            Assert.Empty(state.Players);
        }

        [Fact]
        public void ToLogic_Yacht_ReturnsTheYachtOfTheLogic()
        {
            LogicGame.ScoreCategory category = GameMapper.ToLogic(GameContracts.ScoreCategory.Yacht);

            Assert.Equal(LogicGame.ScoreCategory.Yacht, category);
        }
    }
}
