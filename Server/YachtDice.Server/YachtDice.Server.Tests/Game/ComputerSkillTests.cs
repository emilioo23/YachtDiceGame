using Xunit;
using YachtDice.Server.Logic.Game;
using YachtDice.Server.Logic.Rooms;

namespace YachtDice.Server.Tests.Game
{
    /// <summary>
    /// Rules of each difficulty of the computer player: how many times it rolls and which dice it keeps.
    /// </summary>
    public class ComputerSkillTests
    {
        private const int OneRoll = 1;
        private const ComputerDifficulty UnknownDifficulty = (ComputerDifficulty)99;

        private static readonly int[] FourInARowWithAPair = { 2, 3, 3, 4, 5 };

        [Fact]
        public void For_Easy_RollsOnlyOnce()
        {
            int maxRollsResult = ComputerSkill.For(ComputerDifficulty.Easy).MaxRolls;

            Assert.Equal(OneRoll, maxRollsResult);
        }

        [Fact]
        public void For_DifficultyWithoutRules_PlaysAsNormal()
        {
            ComputerSkill skillExpected = ComputerSkill.For(ComputerDifficulty.Normal);

            ComputerSkill skillResult = ComputerSkill.For(UnknownDifficulty);

            Assert.Same(skillExpected, skillResult);
        }

        [Fact]
        public void ChooseHeldDice_HardWithFourInARow_HoldsOneDieOfEachValueOfTheStraight()
        {
            bool[] heldDiceExpected = { true, true, false, true, true };

            bool[] heldDiceResult = ComputerSkill.For(ComputerDifficulty.Hard).ChooseHeldDice(FourInARowWithAPair);

            Assert.Equal(heldDiceExpected, heldDiceResult);
        }

        [Fact]
        public void ChooseHeldDice_NormalWithFourInARow_HoldsTheRepeatedValue()
        {
            bool[] heldDiceExpected = { false, true, true, false, false };

            bool[] heldDiceResult = ComputerSkill.For(ComputerDifficulty.Normal).ChooseHeldDice(FourInARowWithAPair);

            Assert.Equal(heldDiceExpected, heldDiceResult);
        }
    }
}
