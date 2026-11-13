using System;
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
            TurnPhase phase = TurnPhaseResolver.ResolvePhase(true, CreatePlayers(LocalPlayerId));

            Assert.Equal(TurnPhase.Finished, phase);
        }

        [Fact]
        public void ResolvePhase_NobodyInTurn_ReturnsPaused()
        {
            TurnPhase phase = TurnPhaseResolver.ResolvePhase(false, CreatePlayers(string.Empty));

            Assert.Equal(TurnPhase.Paused, phase);
        }

        [Fact]
        public void ResolvePhase_LocalPlayerInTurn_ReturnsLocalTurn()
        {
            TurnPhase phase = TurnPhaseResolver.ResolvePhase(false, CreatePlayers(LocalPlayerId));

            Assert.Equal(TurnPhase.LocalTurn, phase);
        }

        [Fact]
        public void ResolvePhase_OtherPlayerInTurn_ReturnsOpponentTurn()
        {
            TurnPhase phase = TurnPhaseResolver.ResolvePhase(false, CreatePlayers(OtherPlayerId));

            Assert.Equal(TurnPhase.OpponentTurn, phase);
        }

        [Theory]
        [InlineData(3, TurnHint.PressRoll)]
        [InlineData(2, TurnHint.ChooseOrRoll)]
        [InlineData(1, TurnHint.ChooseOrRoll)]
        [InlineData(0, TurnHint.PickBox)]
        public void ResolveHint_LocalTurn_ReturnsHintOfTheMoment(int rollsLeft, TurnHint expected)
        {
            TurnHint hint = TurnPhaseResolver.ResolveHint(TurnPhase.LocalTurn, rollsLeft, MaxRolls);

            Assert.Equal(expected, hint);
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

        [Fact]
        public void ResolvePhase_NullPlayers_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => TurnPhaseResolver.ResolvePhase(false, null));
        }

        private static TurnPlayers CreatePlayers(string currentPlayerId)
        {
            return new TurnPlayers(currentPlayerId, LocalPlayerId);
        }
    }
}
