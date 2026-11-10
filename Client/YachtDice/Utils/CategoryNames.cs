using System;
using System.Collections.Generic;
using YachtDice.GameServiceReference;
using YachtDice.Resources;

namespace YachtDice.Utils
{
    /// <summary>
    /// Chooses the localized name of each box of the scorecard.
    /// </summary>
    public static class CategoryNames
    {
        private static readonly Dictionary<ScoreCategory, Func<string>> NameByCategory =
            new Dictionary<ScoreCategory, Func<string>>
            {
                { ScoreCategory.Ones, () => Strings.Game_Category_Ones },
                { ScoreCategory.Twos, () => Strings.Game_Category_Twos },
                { ScoreCategory.Threes, () => Strings.Game_Category_Threes },
                { ScoreCategory.Fours, () => Strings.Game_Category_Fours },
                { ScoreCategory.Fives, () => Strings.Game_Category_Fives },
                { ScoreCategory.Sixes, () => Strings.Game_Category_Sixes },
                { ScoreCategory.FullHouse, () => Strings.Game_Category_FullHouse },
                { ScoreCategory.FourOfAKind, () => Strings.Game_Category_FourOfAKind },
                { ScoreCategory.LittleStraight, () => Strings.Game_Category_LittleStraight },
                { ScoreCategory.BigStraight, () => Strings.Game_Category_BigStraight },
                { ScoreCategory.Choice, () => Strings.Game_Category_Choice },
                { ScoreCategory.Yacht, () => Strings.Game_Category_Yacht }
            };

        /// <summary>
        /// Gets the localized name of a box.
        /// </summary>
        /// <param name="category">Box of the scorecard.</param>
        /// <returns>The text to show to the player.</returns>
        public static string GetName(ScoreCategory category)
        {
            return NameByCategory[category]();
        }
    }
}
