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
        private const int NoPoints = 0;
        private const int OnesPoints = 2;
        private const int SixesPoints = 12;
        private const int ChoicePoints = 18;
        private const int TiedPoints = 20;
        private const int YachtOfSixesUpperPoints = 30;
        private const int YachtPoints = 50;

        [Fact]
        public void Choose_OptionsWithDifferentPoints_ReturnsTheBoxWithMorePoints()
        {
            var options = new List<ScoreOption>
            {
                new ScoreOption(ScoreCategory.Ones, OnesPoints),
                new ScoreOption(ScoreCategory.Choice, ChoicePoints),
                new ScoreOption(ScoreCategory.Sixes, SixesPoints)
            };

            var category = AutoPlayChooser.Choose(options);

            Assert.Equal(ScoreCategory.Choice, category);
        }

        [Fact]
        public void Choose_AllOptionsWithZero_SacrificesTheLeastValuableBox()
        {
            var options = new List<ScoreOption>
            {
                new ScoreOption(ScoreCategory.Yacht, NoPoints),
                new ScoreOption(ScoreCategory.BigStraight, NoPoints),
                new ScoreOption(ScoreCategory.Twos, NoPoints),
                new ScoreOption(ScoreCategory.Sixes, NoPoints)
            };

            var category = AutoPlayChooser.Choose(options);

            Assert.Equal(ScoreCategory.Twos, category);
        }

        [Fact]
        public void Choose_OnlyYachtIsOpenWithZero_UsesYacht()
        {
            var options = new List<ScoreOption> { new ScoreOption(ScoreCategory.Yacht, NoPoints) };

            var category = AutoPlayChooser.Choose(options);

            Assert.Equal(ScoreCategory.Yacht, category);
        }

        [Fact]
        public void Choose_TieWithPoints_KeepsTheMostValuableBox()
        {
            var options = new List<ScoreOption>
            {
                new ScoreOption(ScoreCategory.FullHouse, TiedPoints),
                new ScoreOption(ScoreCategory.Choice, TiedPoints)
            };

            var category = AutoPlayChooser.Choose(options);

            Assert.Equal(ScoreCategory.Choice, category);
        }

        [Fact]
        public void Choose_YachtOfSixesWithYachtOpen_ReturnsYacht()
        {
            var options = new List<ScoreOption>
            {
                new ScoreOption(ScoreCategory.Sixes, YachtOfSixesUpperPoints),
                new ScoreOption(ScoreCategory.Choice, YachtOfSixesUpperPoints),
                new ScoreOption(ScoreCategory.Yacht, YachtPoints)
            };

            var category = AutoPlayChooser.Choose(options);

            Assert.Equal(ScoreCategory.Yacht, category);
        }
    }
}
