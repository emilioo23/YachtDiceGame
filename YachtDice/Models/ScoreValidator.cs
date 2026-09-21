using System;
using System.Linq;

namespace YachtDice.Models
{
    public enum ScoreCategory
    {
        Ones, Twos, Threes, Fours, Fives, Sixes,
        ThreeOfAKind, FourOfAKind, FullHouse,
        SmallStraight, LargeStraight, Yacht, Choice
    }

    public static class ScoreValidator
    {
        public static int CalculateScore(int[] dice, ScoreCategory category)
        {
            if (dice == null || dice.Length != 5)
                throw new ArgumentException("Debe haber exactamente 5 dados.");

            var groups = dice.GroupBy(d => d).ToDictionary(g => g.Key, g => g.Count());
            int sum = dice.Sum();

            switch (category)
            {
                case ScoreCategory.Ones: return dice.Where(d => d == 1).Sum();
                case ScoreCategory.Twos: return dice.Where(d => d == 2).Sum();
                case ScoreCategory.Threes: return dice.Where(d => d == 3).Sum();
                case ScoreCategory.Fours: return dice.Where(d => d == 4).Sum();
                case ScoreCategory.Fives: return dice.Where(d => d == 5).Sum();
                case ScoreCategory.Sixes: return dice.Where(d => d == 6).Sum();
                
                case ScoreCategory.ThreeOfAKind:
                    return groups.Values.Any(c => c >= 3) ? sum : 0;
                    
                case ScoreCategory.FourOfAKind:
                    return groups.Values.Any(c => c >= 4) ? sum : 0;
                    
                case ScoreCategory.FullHouse:
                    bool hasThree = groups.Values.Contains(3);
                    bool hasTwo = groups.Values.Contains(2);
                    return (hasThree && hasTwo) || groups.Values.Contains(5) ? 25 : 0;
                    
                case ScoreCategory.SmallStraight:
                    var distinct = dice.Distinct().OrderBy(d => d).ToList();
                    bool isSmall = distinct.Count >= 4 &&
                                   ((distinct.Contains(1) && distinct.Contains(2) && distinct.Contains(3) && distinct.Contains(4)) ||
                                    (distinct.Contains(2) && distinct.Contains(3) && distinct.Contains(4) && distinct.Contains(5)) ||
                                    (distinct.Contains(3) && distinct.Contains(4) && distinct.Contains(5) && distinct.Contains(6)));
                    return isSmall ? 30 : 0;
                    
                case ScoreCategory.LargeStraight:
                    var ordered = dice.OrderBy(d => d).ToArray();
                    bool isLarge = ordered.SequenceEqual(new[] { 1, 2, 3, 4, 5 }) ||
                                   ordered.SequenceEqual(new[] { 2, 3, 4, 5, 6 });
                    return isLarge ? 40 : 0;
                    
                case ScoreCategory.Yacht:
                    return groups.Values.Any(c => c == 5) ? 50 : 0;
                    
                case ScoreCategory.Choice:
                    return sum;
                    
                default:
                    return 0;
            }
        }
    }
}