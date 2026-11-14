using System;
using System.Collections.Generic;
using System.Linq;

namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// The scorecard of one player. It knows which boxes are open, applies the Joker rule, gives the upper
    /// bonus and the Yacht bonus, and keeps the total. It is not thread-safe.
    /// </summary>
    public sealed class ScoreCard
    {
        private const int UpperBonusThreshold = 63;
        private const int UpperBonusPoints = 35;
        private const int YachtBonusPoints = 100;
        private const int NoPoints = 0;

        private readonly Dictionary<ScoreCategory, int> _scores = new Dictionary<ScoreCategory, int>();
        private readonly ScoreCalculator _calculator;

        /// <summary>
        /// Initializes an empty scorecard.
        /// </summary>
        /// <param name="calculator">Calculator that gives the points of each box.</param>
        public ScoreCard(ScoreCalculator calculator)
        {
            _calculator = calculator;
        }

        /// <summary>
        /// Gets the points that the player earned with the extra Yachts.
        /// </summary>
        public int YachtBonus { get; private set; }

        /// <summary>
        /// Gets the sum of the points of the upper section.
        /// </summary>
        public int UpperSubtotal => _scores
            .Where(entry => ScoreCategories.IsUpper(entry.Key))
            .Sum(entry => entry.Value);

        /// <summary>
        /// Gets the bonus of the upper section, which is earned with sixty-three points or more.
        /// </summary>
        public int UpperBonus => UpperSubtotal >= UpperBonusThreshold ? UpperBonusPoints : NoPoints;

        /// <summary>
        /// Gets the total of the scorecard, with the bonuses.
        /// </summary>
        public int Total => _scores.Values.Sum() + UpperBonus + YachtBonus;

        /// <summary>
        /// Gets a value indicating whether all the boxes have points.
        /// </summary>
        public bool IsComplete => _scores.Count == ScoreCategories.All.Count;

        /// <summary>
        /// Tells whether a box already has points.
        /// </summary>
        /// <param name="category">Box to check.</param>
        /// <returns>True if the box was already used.</returns>
        public bool IsFilled(ScoreCategory category)
        {
            return _scores.ContainsKey(category);
        }

        /// <summary>
        /// Gets the points of a box.
        /// </summary>
        /// <param name="category">Box to read.</param>
        /// <returns>The points of the box, or zero if the box is still open.</returns>
        public int GetScore(ScoreCategory category)
        {
            int score;

            if (IsFilled(category))
            {
                score = _scores[category];
            }
            else
            {
                score = NoPoints;
            }

            return score;
        }

        /// <summary>
        /// Gets the boxes that still have no points.
        /// </summary>
        /// <returns>The open boxes, in the order of the scorecard.</returns>
        public IReadOnlyList<ScoreCategory> GetOpenCategories()
        {
            Func<ScoreCategory, bool> isNotFilled = category => !IsFilled(category);

            return ScoreCategories.All.Where(isNotFilled).ToList();
        }

        /// <summary>
        /// Gets the boxes that the player can choose with some dice, and the points of each one. When the
        /// dice are a Yacht and the Yacht box is already used, the Joker rule limits the boxes.
        /// </summary>
        /// <param name="dice">Values shown by the five dice.</param>
        /// <returns>The boxes that can be chosen.</returns>
        public IReadOnlyList<ScoreEntry> GetOptions(IReadOnlyList<int> dice)
        {
            var isJoker = IsJoker(dice);
            IReadOnlyList<ScoreCategory> categories;

            if (isJoker)
            {
                categories = GetJokerCategories(dice[0]);
            }
            else
            {
                categories = GetOpenCategories();
            }

            return categories.Select(category => new ScoreEntry(category, CalculatePoints(category, dice, isJoker)))
                .ToList();
        }

        /// <summary>
        /// Writes the points of some dice in a box.
        /// </summary>
        /// <param name="category">Box to use.</param>
        /// <param name="dice">Values shown by the five dice.</param>
        /// <returns>The outcome of the operation.</returns>
        public GameActionStatus Score(ScoreCategory category, IReadOnlyList<int> dice)
        {
            Func<ScoreEntry, bool> isSameCategory = option => option.Category == category;
            var options = GetOptions(dice).Where(isSameCategory).ToList();
            GameActionStatus status;

            if (IsFilled(category))
            {
                status = GameActionStatus.CategoryFilled;
            }
            else if (!options.Any())
            {
                status = GameActionStatus.CategoryNotAllowed;
            }
            else
            {
                Write(options.Single(), dice);
                status = GameActionStatus.Success;
            }

            return status;
        }

        private static IReadOnlyList<ScoreCategory> ChooseLowerOrAny(IReadOnlyList<ScoreCategory> openCategories)
        {
            Func<ScoreCategory, bool> isNotUpper = category => !ScoreCategories.IsUpper(category);
            var lowerCategories = openCategories.Where(isNotUpper).ToList();
            IReadOnlyList<ScoreCategory> categories;

            if (lowerCategories.Any())
            {
                categories = lowerCategories;
            }
            else
            {
                categories = openCategories;
            }

            return categories;
        }

        private bool IsJoker(IReadOnlyList<int> dice)
        {
            return _calculator.IsYacht(dice) && IsFilled(ScoreCategory.Yacht);
        }

        private IReadOnlyList<ScoreCategory> GetJokerCategories(int diceValue)
        {
            var matchingCategory = ScoreCategories.GetUpperCategory(diceValue);
            var openCategories = GetOpenCategories();
            IReadOnlyList<ScoreCategory> categories;

            if (openCategories.Contains(matchingCategory))
            {
                categories = new[] { matchingCategory };
            }
            else
            {
                categories = ChooseLowerOrAny(openCategories);
            }

            return categories;
        }

        private int CalculatePoints(ScoreCategory category, IReadOnlyList<int> dice, bool isJoker)
        {
            int points;

            if (isJoker)
            {
                points = _calculator.CalculateJoker(category, dice);
            }
            else
            {
                points = _calculator.Calculate(category, dice);
            }

            return points;
        }

        private void Write(ScoreEntry option, IReadOnlyList<int> dice)
        {
            if (_calculator.IsYacht(dice) && GetScore(ScoreCategory.Yacht) > NoPoints)
            {
                YachtBonus += YachtBonusPoints;
            }

            _scores[option.Category] = option.Points;
        }
    }
}
