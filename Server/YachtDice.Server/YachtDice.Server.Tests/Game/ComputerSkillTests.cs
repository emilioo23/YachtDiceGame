using System.Collections.Generic;
using Xunit;
using YachtDice.Server.Logic.Game;
using YachtDice.Server.Logic.Rooms;

namespace YachtDice.Server.Tests.Game
{
    /// <summary>
    /// Tests of the decisions of the computer players.
    /// </summary>
    public class ComputerSkillTests
    {
        private const int OneRoll = 1;
        private const ComputerDifficulty UnknownDifficulty = (ComputerDifficulty)99;
        private const int NoPoints = 0;
        private const int OnesPoints = 2;
        private const int SixesPoints = 12;
        private const int ChoicePoints = 18;
        private const int TiedPoints = 20;
        private const int YachtOfSixesUpperPoints = 30;
        private const int YachtPoints = 50;
        private const int StraightPoints = 30;
        private static readonly int[] FourInARowWithAPair = { 2, 3, 3, 4, 5 };
        private static readonly int[] ThreeFives = { 2, 5, 5, 1, 5 };
        private static readonly int[] PairOfThreesAndPairOfSixes = { 3, 3, 6, 6, 1 };

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

        [Fact]
        public void Choose_OptionsWithDifferentPoints_ReturnsTheBoxWithMorePoints()
        {
            var options = new List<ScoreEntry>
            {
                new ScoreEntry(ScoreCategory.Ones, OnesPoints),
                new ScoreEntry(ScoreCategory.Choice, ChoicePoints),
                new ScoreEntry(ScoreCategory.Sixes, SixesPoints)
            };

            ScoreCategory category = ComputerSkill.Choose(options);

            Assert.Equal(ScoreCategory.Choice, category);
        }

        [Fact]
        public void Choose_AllOptionsWithZero_SacrificesTheLeastValuableBox()
        {
            var options = new List<ScoreEntry>
            {
                new ScoreEntry(ScoreCategory.Yacht, NoPoints),
                new ScoreEntry(ScoreCategory.BigStraight, NoPoints),
                new ScoreEntry(ScoreCategory.Twos, NoPoints),
                new ScoreEntry(ScoreCategory.Sixes, NoPoints)
            };

            ScoreCategory category = ComputerSkill.Choose(options);

            Assert.Equal(ScoreCategory.Twos, category);
        }

        [Fact]
        public void Choose_OnlyYachtIsOpenWithZero_UsesYacht()
        {
            var options = new List<ScoreEntry> { new ScoreEntry(ScoreCategory.Yacht, NoPoints) };

            ScoreCategory category = ComputerSkill.Choose(options);

            Assert.Equal(ScoreCategory.Yacht, category);
        }

        [Fact]
        public void Choose_TieWithPoints_KeepsTheMostValuableBox()
        {
            var options = new List<ScoreEntry>
            {
                new ScoreEntry(ScoreCategory.FullHouse, TiedPoints),
                new ScoreEntry(ScoreCategory.Choice, TiedPoints)
            };

            ScoreCategory category = ComputerSkill.Choose(options);

            Assert.Equal(ScoreCategory.Choice, category);
        }

        [Fact]
        public void Choose_YachtOfSixesWithYachtOpen_ReturnsYacht()
        {
            var options = new List<ScoreEntry>
            {
                new ScoreEntry(ScoreCategory.Sixes, YachtOfSixesUpperPoints),
                new ScoreEntry(ScoreCategory.Choice, YachtOfSixesUpperPoints),
                new ScoreEntry(ScoreCategory.Yacht, YachtPoints)
            };

            ScoreCategory category = ComputerSkill.Choose(options);

            Assert.Equal(ScoreCategory.Yacht, category);
        }

        [Fact]
        public void ChooseMostRepeatedDice_ThreeOfTheSameValue_HoldsThoseThreeDice()
        {
            bool[] heldDiceExpected = { false, true, true, false, true };

            bool[] heldDiceResult = ComputerSkill.ChooseMostRepeatedDice(ThreeFives);

            Assert.Equal(heldDiceExpected, heldDiceResult);
        }

        [Fact]
        public void ChooseMostRepeatedDice_TwoPairs_HoldsTheHigherPair()
        {
            bool[] heldDiceExpected = { false, false, true, true, false };

            bool[] heldDiceResult = ComputerSkill.ChooseMostRepeatedDice(PairOfThreesAndPairOfSixes);

            Assert.Equal(heldDiceExpected, heldDiceResult);
        }

        [Fact]
        public void IsWorthKeeping_BigStraightIsTheBestBox_ReturnsTrue()
        {
            var options = new List<ScoreEntry>
            {
                new ScoreEntry(ScoreCategory.BigStraight, StraightPoints),
                new ScoreEntry(ScoreCategory.Choice, TiedPoints)
            };

            bool isWorthKeeping = ComputerSkill.IsWorthKeeping(options);

            Assert.True(isWorthKeeping);
        }

        [Fact]
        public void IsWorthKeeping_ChoiceIsTheBestBox_ReturnsFalse()
        {
            var options = new List<ScoreEntry>
            {
                new ScoreEntry(ScoreCategory.Sixes, SixesPoints),
                new ScoreEntry(ScoreCategory.Choice, ChoicePoints),
                new ScoreEntry(ScoreCategory.Yacht, NoPoints)
            };

            bool isWorthKeeping = ComputerSkill.IsWorthKeeping(options);

            Assert.False(isWorthKeeping);
        }
    }
}
