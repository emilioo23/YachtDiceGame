using System.Collections.Generic;
using Xunit;
using YachtDice.Server.Logic.Game;

namespace YachtDice.Server.Tests.Game
{
    /// <summary>
    /// Choice of the box when the time of a turn is over.
    /// </summary>
    public class AutoPlayChooserTests
    {
        [Fact]
        public void Choose_OptionsWithDifferentPoints_ReturnsTheBoxWithMorePoints()
        {
            var options = new List<ScoreOption>
            {
                new ScoreOption(ScoreCategory.Ones, 2),
                new ScoreOption(ScoreCategory.Choice, 18),
                new ScoreOption(ScoreCategory.Sixes, 12)
            };

            var category = AutoPlayChooser.Choose(options);

            Assert.Equal(ScoreCategory.Choice, category);
        }

        [Fact]
        public void Choose_AllOptionsWithZero_SacrificesTheLeastValuableBox()
        {
            var options = new List<ScoreOption>
            {
                new ScoreOption(ScoreCategory.Yacht, 0),
                new ScoreOption(ScoreCategory.BigStraight, 0),
                new ScoreOption(ScoreCategory.Twos, 0),
                new ScoreOption(ScoreCategory.Sixes, 0)
            };

            var category = AutoPlayChooser.Choose(options);

            Assert.Equal(ScoreCategory.Twos, category);
        }

        [Fact]
        public void Choose_OnlyYachtIsOpenWithZero_UsesYacht()
        {
            var options = new List<ScoreOption> { new ScoreOption(ScoreCategory.Yacht, 0) };

            var category = AutoPlayChooser.Choose(options);

            Assert.Equal(ScoreCategory.Yacht, category);
        }

        [Fact]
        public void Choose_TieWithPoints_KeepsTheMostValuableBox()
        {
            var options = new List<ScoreOption>
            {
                new ScoreOption(ScoreCategory.FullHouse, 20),
                new ScoreOption(ScoreCategory.Choice, 20)
            };

            var category = AutoPlayChooser.Choose(options);

            Assert.Equal(ScoreCategory.Choice, category);
        }

        [Fact]
        public void Choose_YachtOfSixesWithYachtOpen_ReturnsYacht()
        {
            var options = new List<ScoreOption>
            {
                new ScoreOption(ScoreCategory.Sixes, 30),
                new ScoreOption(ScoreCategory.Choice, 30),
                new ScoreOption(ScoreCategory.Yacht, 50)
            };

            var category = AutoPlayChooser.Choose(options);

            Assert.Equal(ScoreCategory.Yacht, category);
        }
    }
}
