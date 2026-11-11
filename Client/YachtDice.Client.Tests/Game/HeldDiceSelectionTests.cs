using Xunit;
using YachtDice.Client.Logic.Game;

namespace YachtDice.Client.Tests.Game
{
    /// <summary>
    /// Dice that the player keeps for the next roll.
    /// </summary>
    public class HeldDiceSelectionTests
    {
        private const int FirstDie = 0;
        private const int SecondDie = 1;
        private const int ThirdDie = 2;
        private const int FourthDie = 3;
        private const int FifthDie = 4;

        private static readonly bool[] NoHeldDice = { false, false, false, false, false };
        private static readonly bool[] SecondAndFourthHeld = { false, true, false, true, false };

        [Fact]
        public void Toggle_FreeDieWhenHoldingIsAllowed_HoldsTheDie()
        {
            var selection = new HeldDiceSelection();
            var expected = new { IsChanged = true, IsHeld = true };

            bool isChanged = selection.Toggle(ThirdDie, true);

            var actual = new { IsChanged = isChanged, IsHeld = selection.IsHeld(ThirdDie) };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Toggle_HeldDie_FreesTheDie()
        {
            var selection = new HeldDiceSelection();
            selection.Toggle(ThirdDie, true);

            selection.Toggle(ThirdDie, true);

            Assert.False(selection.IsHeld(ThirdDie));
        }

        [Fact]
        public void Toggle_HoldingNotAllowed_KeepsTheSelection()
        {
            var selection = new HeldDiceSelection();
            var expected = new { IsChanged = false, IsHeld = false };

            bool isChanged = selection.Toggle(FirstDie, false);

            var actual = new { IsChanged = isChanged, IsHeld = selection.IsHeld(FirstDie) };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Clear_HeldDice_FreesAllOfThem()
        {
            var selection = new HeldDiceSelection();
            selection.Toggle(FirstDie, true);
            selection.Toggle(FifthDie, true);

            selection.Clear();

            Assert.Equal(NoHeldDice, selection.ToArray());
        }

        [Fact]
        public void ToArray_TwoHeldDice_ReturnsOneValueForEachDie()
        {
            var selection = new HeldDiceSelection();
            selection.Toggle(SecondDie, true);
            selection.Toggle(FourthDie, true);

            bool[] held = selection.ToArray();

            Assert.Equal(SecondAndFourthHeld, held);
        }

        [Fact]
        public void ToArray_ModifyingTheCopy_DoesNotChangeTheSelection()
        {
            var selection = new HeldDiceSelection();

            selection.ToArray()[FirstDie] = true;

            Assert.False(selection.IsHeld(FirstDie));
        }
    }
}
