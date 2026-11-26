using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Logic.Rooms;

namespace YachtDice.Server.Logic.Game
{
    /// <summary>
    /// Decisions of the computer players: how it keeps the dice and which category it picks when a player does not
    /// choose.
    /// </summary>
    public sealed class ComputerSkill
    {
        private const int OneRoll = 1;
        private const int AllRolls = 3;
        private const int MinStraightDraw = 4;
        private const int NextValue = 1;
        private const int NoPoints = 0;
        private static readonly Dictionary<ComputerDifficulty, ComputerSkill> SkillsByDifficulty =
            new Dictionary<ComputerDifficulty, ComputerSkill>
            {
                { ComputerDifficulty.Easy, new ComputerSkill(OneRoll, false) },
                { ComputerDifficulty.Normal, new ComputerSkill(AllRolls, false) },
                { ComputerDifficulty.Hard, new ComputerSkill(AllRolls, true) }
            };
        private static readonly List<ScoreCategory> FixedPointCategories = new List<ScoreCategory>
        {
            ScoreCategory.FullHouse,
            ScoreCategory.LittleStraight,
            ScoreCategory.BigStraight,
            ScoreCategory.Yacht
        };
        private static readonly List<ScoreCategory> SacrificeOrder = new List<ScoreCategory>
        {
            ScoreCategory.Ones,
            ScoreCategory.Twos,
            ScoreCategory.Threes,
            ScoreCategory.Fours,
            ScoreCategory.Fives,
            ScoreCategory.Sixes,
            ScoreCategory.Choice,
            ScoreCategory.FourOfAKind,
            ScoreCategory.FullHouse,
            ScoreCategory.LittleStraight,
            ScoreCategory.BigStraight,
            ScoreCategory.Yacht
        };

        private ComputerSkill(int maxRolls, bool chasesStraights)
        {
            MaxRolls = maxRolls;
            ChasesStraights = chasesStraights;
        }

        /// <summary>
        /// Gets how many rolls the computer player makes at most in a turn.
        /// </summary>
        public int MaxRolls { get; }

        /// <summary>
        /// Gets a value indicating whether the computer player keeps four dice in a row to complete a straight.
        /// </summary>
        public bool ChasesStraights { get; }

        /// <summary>
        /// Gets the rules of a difficulty. A difficulty without rules plays as
        /// <see cref="ComputerDifficulty.Normal"/>.
        /// </summary>
        /// <param name="difficulty">Difficulty chosen by the host.</param>
        /// <returns>The rules of the difficulty.</returns>
        public static ComputerSkill For(ComputerDifficulty difficulty)
        {
            if (!SkillsByDifficulty.TryGetValue(difficulty, out ComputerSkill skill))
            {
                skill = SkillsByDifficulty[ComputerDifficulty.Normal];
            }

            return skill;
        }

        /// <summary>
        /// Chooses the dice that the computer player keeps before rolling again.
        /// </summary>
        /// <param name="dice">Values of the five dice.</param>
        /// <returns>One value for each die: true if the die keeps its value.</returns>
        public bool[] ChooseHeldDice(IReadOnlyList<int> dice)
        {
            List<int> straightDraw = FindLongestRun(dice);
            bool[] heldDice = ComputerSkill.ChooseMostRepeatedDice(dice);

            if (ChasesStraights && straightDraw.Count >= MinStraightDraw)
            {
                heldDice = HoldOneOfEach(dice, straightDraw);
            }

            return heldDice;
        }

        /// <summary>
        /// Chooses the box for a player who did not play in time.
        /// </summary>
        /// <param name="options">Boxes that the player can choose with the current dice.</param>
        /// <returns>The box to use.</returns>
        public static ScoreCategory Choose(IReadOnlyList<ScoreEntry> options)
        {
            return options
                .OrderByDescending(option => option.Points)
                .ThenBy(option => SacrificeOrder.IndexOf(option.Category))
                .First()
                .Category;
        }

        /// <summary>
        /// Chooses the dice that a computer player keeps before rolling again: the dice of the value that
        /// repeats the most, and the highest value when two values repeat the same number of times.
        /// </summary>
        /// <param name="dice">Values of the five dice.</param>
        /// <returns>One value for each die: true if the die keeps its value.</returns>
        public static bool[] ChooseMostRepeatedDice(IReadOnlyList<int> dice)
        {
            int keptValue = dice
                .GroupBy(value => value)
                .OrderByDescending(sameValues => sameValues.Count())
                .ThenByDescending(sameValues => sameValues.Key)
                .First()
                .Key;

            Func<int, bool> isKeptValue = value => value == keptValue;

            return dice.Select(isKeptValue).ToArray();
        }

        /// <summary>
        /// Tells whether a computer player stops rolling because the best box already gives its full points,
        /// like a straight or a Yacht, which another roll could only break.
        /// </summary>
        /// <param name="options">Boxes that the player can choose with the current dice.</param>
        /// <returns>True if the player should write the points now.</returns>
        public static bool IsWorthKeeping(IReadOnlyList<ScoreEntry> options)
        {
            ScoreCategory best = Choose(options);
            Func<ScoreEntry, bool> isBest = option => option.Category == best;

            return FixedPointCategories.Contains(best) && options.First(isBest).Points > NoPoints;
        }

        private static List<int> FindLongestRun(IReadOnlyList<int> dice)
        {
            List<int> values = dice.Distinct().OrderBy(value => value).ToList();
            var longestRun = new List<int>();
            var currentRun = new List<int>();

            foreach (int value in values)
            {
                bool continuesRun = currentRun.Count > 0 && currentRun.Last() + NextValue == value;

                if (!continuesRun)
                {
                    currentRun = new List<int>();
                }

                currentRun.Add(value);

                if (currentRun.Count > longestRun.Count)
                {
                    longestRun = currentRun;
                }
            }

            return longestRun;
        }

        private static bool[] HoldOneOfEach(IReadOnlyList<int> dice, List<int> run)
        {
            var pendingValues = new HashSet<int>(run);
            var heldDice = new bool[dice.Count];

            for (int index = 0; index < dice.Count; index++)
            {
                heldDice[index] = pendingValues.Remove(dice[index]);
            }

            return heldDice;
        }
    }
}
