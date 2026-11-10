using YachtDice.GameServiceReference;
using YachtDice.Resources;

namespace YachtDice.Utils
{
    /// <summary>
    /// Chooses the localized name of each box of the scorecard.
    /// </summary>
    public static class CategoryNames
    {
        /// <summary>
        /// Gets the localized name of a box.
        /// </summary>
        /// <param name="category">Box of the scorecard.</param>
        /// <returns>The text to show to the player.</returns>
        public static string GetName(ScoreCategory category)
        {
            string name;

            switch (category)
            {
                case ScoreCategory.Ones:
                    name = Strings.Game_Category_Ones;
                    break;
                case ScoreCategory.Twos:
                    name = Strings.Game_Category_Twos;
                    break;
                case ScoreCategory.Threes:
                    name = Strings.Game_Category_Threes;
                    break;
                case ScoreCategory.Fours:
                    name = Strings.Game_Category_Fours;
                    break;
                case ScoreCategory.Fives:
                    name = Strings.Game_Category_Fives;
                    break;
                case ScoreCategory.Sixes:
                    name = Strings.Game_Category_Sixes;
                    break;
                case ScoreCategory.FullHouse:
                    name = Strings.Game_Category_FullHouse;
                    break;
                case ScoreCategory.FourOfAKind:
                    name = Strings.Game_Category_FourOfAKind;
                    break;
                case ScoreCategory.LittleStraight:
                    name = Strings.Game_Category_LittleStraight;
                    break;
                case ScoreCategory.BigStraight:
                    name = Strings.Game_Category_BigStraight;
                    break;
                case ScoreCategory.Choice:
                    name = Strings.Game_Category_Choice;
                    break;
                case ScoreCategory.Yacht:
                    name = Strings.Game_Category_Yacht;
                    break;
                default:
                    name = category.ToString();
                    break;
            }

            return name;
        }
    }
}
