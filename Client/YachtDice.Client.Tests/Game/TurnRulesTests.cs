using Xunit;
using YachtDice.Client.Logic.Game;

namespace YachtDice.Client.Tests.Game
{
    /// <summary>
    /// Phase of the turn and celebration of the yacht.
    /// </summary>
    public class TurnRulesTests
    {
        private const int MaxRolls = 3;
        private const string LocalPlayerId = "a";
        private const string OtherPlayerId = "b";
        private const int YachtPoints = 50;
        private const int NoPoints = 0;

        [Fact]
        public void ResolvePhase_FinishedGame_ReturnsFinished()
        {
            TurnPhase phase = TurnRules.ResolvePhase(true, LocalPlayerId, LocalPlayerId);

            Assert.Equal(TurnPhase.Finished, phase);
        }

        [Fact]
        public void ResolvePhase_NobodyInTurn_ReturnsPaused()
        {
            TurnPhase phase = TurnRules.ResolvePhase(false, string.Empty, LocalPlayerId);

            Assert.Equal(TurnPhase.Paused, phase);
        }

        [Fact]
        public void ResolvePhase_LocalPlayerInTurn_ReturnsLocalTurn()
        {
            TurnPhase phase = TurnRules.ResolvePhase(false, LocalPlayerId, LocalPlayerId);

            Assert.Equal(TurnPhase.LocalTurn, phase);
        }

        [Fact]
        public void ResolvePhase_OtherPlayerInTurn_ReturnsOpponentTurn()
        {
            TurnPhase phase = TurnRules.ResolvePhase(false, OtherPlayerId, LocalPlayerId);

            Assert.Equal(TurnPhase.OpponentTurn, phase);
        }

        [Theory]
        [InlineData(3, TurnHint.PressRoll)]
        [InlineData(2, TurnHint.ChooseOrRoll)]
        [InlineData(1, TurnHint.ChooseOrRoll)]
        [InlineData(0, TurnHint.PickBox)]
        public void ResolveHint_LocalTurn_ReturnsHintOfTheMoment(int rollsLeft, TurnHint hintExpected)
        {
            TurnHint hint = TurnRules.ResolveHint(TurnPhase.LocalTurn, rollsLeft, MaxRolls);

            Assert.Equal(hintExpected, hint);
        }

        [Fact]
        public void ResolveHint_OpponentTurn_ReturnsWaitOpponent()
        {
            TurnHint hint = TurnRules.ResolveHint(TurnPhase.OpponentTurn, MaxRolls, MaxRolls);

            Assert.Equal(TurnHint.WaitOpponent, hint);
        }

        [Fact]
        public void ResolveHint_PausedGame_ReturnsNone()
        {
            TurnHint hint = TurnRules.ResolveHint(TurnPhase.Paused, MaxRolls, MaxRolls);

            Assert.Equal(TurnHint.None, hint);
        }

        [Fact]
        public void IsCelebrationDue_YachtBoxJustFilledWithFifty_ReturnsTrue()
        {
            var isDue = TurnRules.IsCelebrationDue(false, true, YachtPoints);

            Assert.True(isDue);
        }

        [Fact]
        public void IsCelebrationDue_YachtBoxJustFilledWithZero_ReturnsFalse()
        {
            var isDue = TurnRules.IsCelebrationDue(false, true, NoPoints);

            Assert.False(isDue);
        }

        [Fact]
        public void IsCelebrationDue_YachtBoxAlreadyFilled_ReturnsFalse()
        {
            var isDue = TurnRules.IsCelebrationDue(true, true, YachtPoints);

            Assert.False(isDue);
        }

        [Fact]
        public void IsCelebrationDue_YachtBoxStillOpen_ReturnsFalse()
        {
            var isDue = TurnRules.IsCelebrationDue(false, false, NoPoints);

            Assert.False(isDue);
        }

        [Fact]
        public void IsCelebrationDue_FortyNinePoints_ReturnsFalse()
        {
            bool isDue = TurnRules.IsCelebrationDue(false, true, YachtPoints - 1);

            Assert.False(isDue);
        }

        [Fact]
        public void IsCelebrationDue_FiftyOnePoints_ReturnsFalse()
        {
            bool isDue = TurnRules.IsCelebrationDue(false, true, YachtPoints + 1);

            Assert.False(isDue);
        }

        [Fact]
        public void IsCelebrationDue_BoxReopenedWithFifty_ReturnsFalse()
        {
            bool isDue = TurnRules.IsCelebrationDue(true, false, YachtPoints);

            Assert.False(isDue);
        }

        [Fact]
        public void IsCelebrationDue_NegativePoints_ReturnsFalse()
        {
            bool isDue = TurnRules.IsCelebrationDue(false, true, -YachtPoints);

            Assert.False(isDue);
        }

        [Fact]
        public void IsCelebrationDue_BoxStillOpenWithFifty_ReturnsFalse()
        {
            bool isDue = TurnRules.IsCelebrationDue(false, false, YachtPoints);

            Assert.False(isDue);
        }
    }
}
