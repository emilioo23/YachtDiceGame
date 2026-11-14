using Xunit;
using YachtDice.Client.Logic.Game;

namespace YachtDice.Client.Tests.Game
{
    /// <summary>
    /// Phase and hint of the turn that the game window shows.
    /// </summary>
    public class TurnPhaseResolverTests
    {
        private const int MaxRolls = 3;
        private const string LocalPlayerId = "a";
        private const string OtherPlayerId = "b";

        [Fact]
        public void ResolvePhase_FinishedGame_ReturnsFinished()
        {
            TurnPhase phase = TurnPhaseResolver.ResolvePhase(true, LocalPlayerId, LocalPlayerId);

            Assert.Equal(TurnPhase.Finished, phase);
        }

        [Fact]
        public void ResolvePhase_NobodyInTurn_ReturnsPaused()
        {
            TurnPhase phase = TurnPhaseResolver.ResolvePhase(false, string.Empty, LocalPlayerId);

            Assert.Equal(TurnPhase.Paused, phase);
        }

        [Fact]
        public void ResolvePhase_LocalPlayerInTurn_ReturnsLocalTurn()
        {
            TurnPhase phase = TurnPhaseResolver.ResolvePhase(false, LocalPlayerId, LocalPlayerId);

            Assert.Equal(TurnPhase.LocalTurn, phase);
        }

        [Fact]
        public void ResolvePhase_OtherPlayerInTurn_ReturnsOpponentTurn()
        {
            TurnPhase phase = TurnPhaseResolver.ResolvePhase(false, OtherPlayerId, LocalPlayerId);

            Assert.Equal(TurnPhase.OpponentTurn, phase);
        }

        [Theory]
        [InlineData(3, TurnHint.PressRoll)]
        [InlineData(2, TurnHint.ChooseOrRoll)]
        [InlineData(1, TurnHint.ChooseOrRoll)]
        [InlineData(0, TurnHint.PickBox)]
        public void ResolveHint_LocalTurn_ReturnsHintOfTheMoment(int rollsLeft, TurnHint expectedHint)
        {
            TurnHint hint = TurnPhaseResolver.ResolveHint(TurnPhase.LocalTurn, rollsLeft, MaxRolls);

            Assert.Equal(expectedHint, hint);
        }

        [Fact]
        public void ResolveHint_OpponentTurn_ReturnsWaitOpponent()
        {
            TurnHint hint = TurnPhaseResolver.ResolveHint(TurnPhase.OpponentTurn, MaxRolls, MaxRolls);

            Assert.Equal(TurnHint.WaitOpponent, hint);
        }

        [Fact]
        public void ResolveHint_PausedGame_ReturnsNone()
        {
            TurnHint hint = TurnPhaseResolver.ResolveHint(TurnPhase.Paused, MaxRolls, MaxRolls);

            Assert.Equal(TurnHint.None, hint);
        }
    }
}
