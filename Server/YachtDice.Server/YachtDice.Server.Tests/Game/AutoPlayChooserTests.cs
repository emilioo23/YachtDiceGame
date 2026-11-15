using System.Collections.Generic;
using Xunit;
using YachtDice.Server.Logic.Game;

namespace YachtDice.Server.Tests.Game
{
    /// <summary>
    /// Choice of the box when the time of a turn is over, and of the dice that a computer player keeps.
    /// </summary>
    public class AutoPlayChooserTests
    {
        private const int NoPoints = 0;
        private const int OnesPoints = 2;
        private const int SixesPoints = 12;
        private const int ChoicePoints = 18;
        private const int TiedPoints = 20;
        private const int YachtOfSixesUpperPoints = 30;
        private const int YachtPoints = 50;
        private const int StraightPoints = 30;

        private static readonly int[] ThreeFives = { 2, 5, 5, 1, 5 };
        private static readonly int[] PairOfThreesAndPairOfSixes = { 3, 3, 6, 6, 1 };

        [Fact]
        public void Choose_OptionsWithDifferentPoints_ReturnsTheBoxWithMorePoints()
        {
            var options = new List<ScoreEntry>
            {
                new ScoreEntry(ScoreCategory.Ones, OnesPoints),
                new ScoreEntry(ScoreCategory.Choice, ChoicePoints),
                new ScoreEntry(ScoreCategory.Sixes, SixesPoints)
            };

            var category = AutoPlayChooser.Choose(options);

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

            var category = AutoPlayChooser.Choose(options);

            Assert.Equal(ScoreCategory.Twos, category);
        }

        [Fact]
        public void Choose_OnlyYachtIsOpenWithZero_UsesYacht()
        {
            var options = new List<ScoreEntry> { new ScoreEntry(ScoreCategory.Yacht, NoPoints) };

            var category = AutoPlayChooser.Choose(options);

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

            var category = AutoPlayChooser.Choose(options);

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

            var category = AutoPlayChooser.Choose(options);

            Assert.Equal(ScoreCategory.Yacht, category);
        }

        [Fact]
        public void ChooseHeldDice_ThreeOfTheSameValue_HoldsThoseThreeDice()
        {
            bool[] heldDiceExpected = { false, true, true, false, true };

            bool[] heldDiceResult = AutoPlayChooser.ChooseHeldDice(ThreeFives);

            Assert.Equal(heldDiceExpected, heldDiceResult);
        }

        [Fact]
        public void ChooseHeldDice_TwoPairs_HoldsTheHigherPair()
        {
            bool[] heldDiceExpected = { false, false, true, true, false };

            bool[] heldDiceResult = AutoPlayChooser.ChooseHeldDice(PairOfThreesAndPairOfSixes);

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

            bool isWorthKeeping = AutoPlayChooser.IsWorthKeeping(options);

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

            bool isWorthKeeping = AutoPlayChooser.IsWorthKeeping(options);

            Assert.False(isWorthKeeping);
        }
    }
}
