using Xunit;
using YachtDice.Client.Logic.Game;

namespace YachtDice.Client.Tests.Game
{
    /// <summary>
    /// Dice that the player keeps for the next roll.
    /// </summary>
    public class HeldDiceSelectionTests
    {
        [Fact]
        public void Toggle_FreeDieWhenHoldingIsAllowed_HoldsTheDie()
        {
            var selection = new HeldDiceSelection();

            var changed = selection.Toggle(2, true);

            Assert.True(changed);
            Assert.True(selection.IsHeld(2));
        }

        [Fact]
        public void Toggle_HeldDie_FreesTheDie()
        {
            var selection = new HeldDiceSelection();
            selection.Toggle(2, true);

            selection.Toggle(2, true);

            Assert.False(selection.IsHeld(2));
        }

        [Fact]
        public void Toggle_HoldingNotAllowed_KeepsTheSelection()
        {
            var selection = new HeldDiceSelection();

            var changed = selection.Toggle(0, false);

            Assert.False(changed);
            Assert.False(selection.IsHeld(0));
        }

        [Fact]
        public void Clear_HeldDice_FreesAllOfThem()
        {
            var selection = new HeldDiceSelection();
            selection.Toggle(0, true);
            selection.Toggle(4, true);

            selection.Clear();

            Assert.Equal(new[] { false, false, false, false, false }, selection.ToArray());
        }

        [Fact]
        public void ToArray_TwoHeldDice_ReturnsOneValueForEachDie()
        {
            var selection = new HeldDiceSelection();
            selection.Toggle(1, true);
            selection.Toggle(3, true);

            var held = selection.ToArray();

            Assert.Equal(new[] { false, true, false, true, false }, held);
        }

        [Fact]
        public void ToArray_ModifyingTheCopy_DoesNotChangeTheSelection()
        {
            var selection = new HeldDiceSelection();

            selection.ToArray()[0] = true;

            Assert.False(selection.IsHeld(0));
        }
    }
}
