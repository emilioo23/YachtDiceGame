using System;
using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// Points of each category of the scorecard and the list of the categories.
    /// </summary>
    public sealed class ScoreCalculator
    {
        /// <summary>
        /// Points of the Yacht box and of the straights, which have a fixed value.
        /// </summary>
        public const int YachtPoints = 50;

        private const int StraightPoints = 30;
        private const int FullHouseTripleCount = 3;
        private const int FullHousePairCount = 2;
        private const int FourOfAKindCount = 4;
        private const int NoPoints = 0;
        private const int LittleStraightFirstValue = 1;
        private const int LittleStraightLastValue = 5;
        private const int BigStraightFirstValue = 2;
        private const int BigStraightLastValue = 6;
        private const string UnknownCategoryMessage = "The box does not exist in the scorecard.";
        private const int FaceToCategoryOffset = 1;

        /// <summary>
        /// Gets all the boxes in the order of the scorecard.
        /// </summary>
        public static IReadOnlyList<ScoreCategory> All { get; } =
            Enum.GetValues(typeof(ScoreCategory)).Cast<ScoreCategory>().ToList();

        /// <summary>
        /// Calculates the points that the dice give in a box.
        /// </summary>
        /// <param name="category">Box to calculate.</param>
        /// <param name="dice">Values shown by the five dice.</param>
        /// <returns>The points of the box, or zero if the dice do not fit it.</returns>
        public int Calculate(ScoreCategory category, IReadOnlyList<int> dice)
        {
            var hand = new DiceHand(dice);
            int points;

            switch (category)
            {
                case ScoreCategory.FullHouse:
                    points = ScoreFullHouse(hand);
                    break;
                case ScoreCategory.FourOfAKind:
                    points = ScoreFourOfAKind(hand);
                    break;
                case ScoreCategory.LittleStraight:
                    points = ScoreLittleStraight(hand);
                    break;
                case ScoreCategory.BigStraight:
                    points = ScoreBigStraight(hand);
                    break;
                case ScoreCategory.Choice:
                    points = hand.Sum;
                    break;
                case ScoreCategory.Yacht:
                    points = ScoreYacht(hand);
                    break;
                default:
                    points = ScoreUpper(hand, category);
                    break;
            }

            return points;
        }

        /// <summary>
        /// Calculates the points that a Yacht gives in a box when it is used as a Joker. The Joker lets
        /// the full house and the straights score as if the dice had that shape.
        /// </summary>
        /// <param name="category">Box to calculate.</param>
        /// <param name="dice">Values shown by the five dice.</param>
        /// <returns>The points of the box.</returns>
        public int CalculateJoker(ScoreCategory category, IReadOnlyList<int> dice)
        {
            int points;

            switch (category)
            {
                case ScoreCategory.FullHouse:
                    points = new DiceHand(dice).Sum;
                    break;
                case ScoreCategory.LittleStraight:
                case ScoreCategory.BigStraight:
                    points = StraightPoints;
                    break;
                default:
                    points = Calculate(category, dice);
                    break;
            }

            return points;
        }

        /// <summary>
        /// Tells whether all the dice show the same value.
        /// </summary>
        /// <param name="dice">Values shown by the five dice.</param>
        /// <returns>True if the roll is a Yacht.</returns>
        public bool IsYacht(IReadOnlyList<int> dice)
        {
            return new DiceHand(dice).IsYacht;
        }

        /// <summary>
        /// Tells whether a box belongs to the upper section, the one that gives the bonus.
        /// </summary>
        /// <param name="category">Box to check.</param>
        /// <returns>True if the box is from Ones to Sixes.</returns>
        public static bool IsUpper(ScoreCategory category)
        {
            return category <= ScoreCategory.Sixes;
        }

        /// <summary>
        /// Gets the upper box that counts the dice with a value.
        /// </summary>
        /// <param name="faceValue">Value of the dice, from one to six.</param>
        /// <returns>The box that sums the dice with that value.</returns>
        public static ScoreCategory GetUpperCategory(int faceValue)
        {
            return (ScoreCategory)(faceValue - FaceToCategoryOffset);
        }

        /// <summary>
        /// Gets the value of the dice that an upper box counts.
        /// </summary>
        /// <param name="category">Upper box.</param>
        /// <returns>The value of the dice, from one to six.</returns>
        public static int GetFaceValue(ScoreCategory category)
        {
            return (int)category + FaceToCategoryOffset;
        }

        private static int ScoreUpper(DiceHand hand, ScoreCategory category)
        {
            if (!ScoreCalculator.IsUpper(category))
            {
                throw new ArgumentOutOfRangeException(nameof(category), UnknownCategoryMessage);
            }

            int value = ScoreCalculator.GetFaceValue(category);

            return hand.CountFace(value) * value;
        }

        private static int ScoreFullHouse(DiceHand hand)
        {
            var isFullHouse = hand.HasFaceExactly(FullHouseTripleCount)
                && hand.HasFaceExactly(FullHousePairCount);

            return isFullHouse ? hand.Sum : NoPoints;
        }

        private static int ScoreFourOfAKind(DiceHand hand)
        {
            return hand.GetFaceRepeatedAtLeast(FourOfAKindCount) * FourOfAKindCount;
        }

        private static int ScoreLittleStraight(DiceHand hand)
        {
            var isStraight = hand.ShowsEachFaceOnce(LittleStraightFirstValue, LittleStraightLastValue);

            return isStraight ? StraightPoints : NoPoints;
        }

        private static int ScoreBigStraight(DiceHand hand)
        {
            var isStraight = hand.ShowsEachFaceOnce(BigStraightFirstValue, BigStraightLastValue);

            return isStraight ? StraightPoints : NoPoints;
        }

        private static int ScoreYacht(DiceHand hand)
        {
            return hand.IsYacht ? YachtPoints : NoPoints;
        }
    }
}
