using System;
using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// Calculates the points that a roll gives in each box of the scorecard. It only knows the rules of the
    /// boxes: which boxes are still open is decided by <see cref="ScoreCard"/>.
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

        private readonly Dictionary<ScoreCategory, Func<DiceTally, int>> _rules;
        private readonly Dictionary<ScoreCategory, Func<DiceTally, int>> _jokerRules;

        /// <summary>
        /// Initializes a calculator with the rules of the twelve boxes.
        /// </summary>
        public ScoreCalculator()
        {
            _rules = CreateRules();
            _jokerRules = CreateJokerRules();
        }

        /// <summary>
        /// Calculates the points that the dice give in a box.
        /// </summary>
        /// <param name="category">Box to calculate.</param>
        /// <param name="dice">Values shown by the five dice.</param>
        /// <returns>The points of the box, or zero if the dice do not fit it.</returns>
        public int Calculate(ScoreCategory category, IReadOnlyList<int> dice)
        {
            return FindRule(_rules, category)(new DiceTally(dice));
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

            if (_jokerRules.ContainsKey(category))
            {
                points = _jokerRules[category](new DiceTally(dice));
            }
            else
            {
                points = Calculate(category, dice);
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
            return new DiceTally(dice).HighestCount == DiceRules.DiceCount;
        }

        private static Func<DiceTally, int> FindRule(
            Dictionary<ScoreCategory, Func<DiceTally, int>> rules,
            ScoreCategory category)
        {
            if (!rules.ContainsKey(category))
            {
                throw new ArgumentOutOfRangeException(nameof(category), UnknownCategoryMessage);
            }

            return rules[category];
        }

        private static int SumOfValue(DiceTally tally, int value)
        {
            return tally.CountOf(value) * value;
        }

        private static int ScoreFullHouse(DiceTally tally)
        {
            var isFullHouse = tally.HasValueExactly(FullHouseTripleCount)
                && tally.HasValueExactly(FullHousePairCount);

            return isFullHouse ? tally.Sum : NoPoints;
        }

        private static int ScoreFourOfAKind(DiceTally tally)
        {
            return tally.GetValueRepeatedAtLeast(FourOfAKindCount) * FourOfAKindCount;
        }

        private static int ScoreLittleStraight(DiceTally tally)
        {
            var isStraight = tally.ShowsEachValueOnce(LittleStraightFirstValue, LittleStraightLastValue);

            return isStraight ? StraightPoints : NoPoints;
        }

        private static int ScoreBigStraight(DiceTally tally)
        {
            var isStraight = tally.ShowsEachValueOnce(BigStraightFirstValue, BigStraightLastValue);

            return isStraight ? StraightPoints : NoPoints;
        }

        private static int ScoreYacht(DiceTally tally)
        {
            var isYacht = tally.HighestCount == DiceRules.DiceCount;

            return isYacht ? YachtPoints : NoPoints;
        }

        private static int ScoreSum(DiceTally tally)
        {
            return tally.Sum;
        }

        private static int ScoreStraightJoker(DiceTally tally)
        {
            return StraightPoints;
        }

        private static Func<DiceTally, int> CreateUpperRule(ScoreCategory category)
        {
            var value = ScoreCategories.GetFaceValue(category);

            return tally => SumOfValue(tally, value);
        }

        private static Dictionary<ScoreCategory, Func<DiceTally, int>> CreateRules()
        {
            var rules = new Dictionary<ScoreCategory, Func<DiceTally, int>>
            {
                { ScoreCategory.FullHouse, ScoreFullHouse },
                { ScoreCategory.FourOfAKind, ScoreFourOfAKind },
                { ScoreCategory.LittleStraight, ScoreLittleStraight },
                { ScoreCategory.BigStraight, ScoreBigStraight },
                { ScoreCategory.Choice, ScoreSum },
                { ScoreCategory.Yacht, ScoreYacht }
            };

            foreach (var category in ScoreCategories.All.Where(ScoreCategories.IsUpper))
            {
                rules[category] = CreateUpperRule(category);
            }

            return rules;
        }

        private static Dictionary<ScoreCategory, Func<DiceTally, int>> CreateJokerRules()
        {
            return new Dictionary<ScoreCategory, Func<DiceTally, int>>
            {
                { ScoreCategory.FullHouse, ScoreSum },
                { ScoreCategory.LittleStraight, ScoreStraightJoker },
                { ScoreCategory.BigStraight, ScoreStraightJoker }
            };
        }
    }
}
