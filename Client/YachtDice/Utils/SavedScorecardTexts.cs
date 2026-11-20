using System;
using System.Collections.Generic;
using YachtDice.PlayerServiceReference;
using YachtDice.Resources;
using GameCategory = YachtDice.GameServiceReference.ScoreCategory;

namespace YachtDice.Utils
{
    /// <summary>
    /// Writes the lines of a scorecard of a past game in the language of the player.
    /// </summary>
    public static class SavedScorecardTexts
    {
        /// <summary>
        /// Writes the title of a scorecard: the name of the player and the final points.
        /// </summary>
        /// <param name="scorecard">Scorecard that the server sent.</param>
        /// <returns>The title of the scorecard.</returns>
        public static string CreateTitle(SavedScorecard scorecard)
        {
            string playerName = string.IsNullOrEmpty(scorecard.PlayerName)
                ? Strings.History_ComputerOpponent
                : scorecard.PlayerName;

            return string.Format(Strings.Scorecard_TitleFormat, playerName, scorecard.Total);
        }

        /// <summary>
        /// Writes one line for each category and one for each bonus.
        /// </summary>
        /// <param name="scorecard">Scorecard that the server sent.</param>
        /// <returns>The lines in the order of the scorecard.</returns>
        public static List<string> CreateLines(SavedScorecard scorecard)
        {
            var lines = new List<string>();

            foreach (SavedScore score in scorecard.Scores ?? Array.Empty<SavedScore>())
            {
                string points = Strings.Scorecard_NotUsed;

                if (score.IsUsed)
                {
                    points = score.Points.ToString();
                }

                lines.Add(string.Format(Strings.Scorecard_LineFormat, ReadCategoryName(score.Category), points));
            }

            string upperBonus = scorecard.HasUpperBonus ? Strings.Scorecard_Yes : Strings.Scorecard_No;
            lines.Add(string.Format(Strings.Scorecard_UpperBonusFormat, upperBonus));
            lines.Add(string.Format(Strings.Scorecard_YachtBonusFormat, scorecard.YachtBonusCount));

            return lines;
        }

        private static string ReadCategoryName(string category)
        {
            string name = category;

            if (Enum.TryParse(category, out GameCategory gameCategory))
            {
                name = CategoryNames.GetName(gameCategory);
            }

            return name;
        }
    }
}
