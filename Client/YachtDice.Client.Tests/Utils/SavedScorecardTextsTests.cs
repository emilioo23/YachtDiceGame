using System.Collections.Generic;
using Xunit;
using YachtDice.PlayerServiceReference;
using YachtDice.Resources;
using YachtDice.Utils;
using GameCategory = YachtDice.GameServiceReference.ScoreCategory;

namespace YachtDice.Client.Tests.Utils
{
    /// <summary>
    /// Lines and title of a scorecard of a past game.
    /// </summary>
    public class SavedScorecardTextsTests
    {
        private const string PlayerName = "gabriel";
        private const int Total = 210;
        private const int YachtPoints = 50;
        private const int NoYachtBonus = 0;

        [Fact]
        public void CreateTitle_Scorecard_ShowsTheNameAndTheTotal()
        {
            var scorecard = new SavedScorecard { PlayerName = PlayerName, Total = Total };
            string titleExpected = string.Format(Strings.Scorecard_TitleFormat, PlayerName, Total);

            string title = SavedScorecardTexts.CreateTitle(scorecard);

            Assert.Equal(titleExpected, title);
        }

        [Fact]
        public void CreateLines_UsedYachtWithoutBonus_WritesTheCategoryAndBothBonuses()
        {
            var scorecard = new SavedScorecard
            {
                Scores = new[]
                {
                    new SavedScore { Category = GameCategory.Yacht.ToString(), Points = YachtPoints, IsUsed = true }
                },
                HasUpperBonus = false,
                YachtBonusCount = NoYachtBonus
            };
            var linesExpected = new List<string>
            {
                string.Format(Strings.Scorecard_LineFormat, CategoryNames.GetName(GameCategory.Yacht), YachtPoints),
                string.Format(Strings.Scorecard_UpperBonusFormat, Strings.Scorecard_No),
                string.Format(Strings.Scorecard_YachtBonusFormat, NoYachtBonus)
            };

            List<string> lines = SavedScorecardTexts.CreateLines(scorecard);

            Assert.Equal(linesExpected, lines);
        }
    }
}
