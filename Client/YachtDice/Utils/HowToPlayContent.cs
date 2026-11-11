using System;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Models;
using YachtDice.Resources;

namespace YachtDice.Utils
{
    /// <summary>
    /// The texts and the examples of the "How to play" window, taken from the language resources.
    /// </summary>
    public static class HowToPlayContent
    {
        private const string NoExample = "";
        private const string UpperExample = "3 3 3 5- 6-";
        private const string FourOfAKindExample = "5 5 5 5 1-";
        private const string FullHouseExample = "2 2 3 3 3";
        private const string LittleStraightExample = "1 2 3 4 5";
        private const string BigStraightExample = "2 3 4 5 6";
        private const string YachtExample = "6 6 6 6 6";
        private const string ChoiceExample = "1 3 4 5 6";

        /// <summary>
        /// Gets the steps of a turn.
        /// </summary>
        /// <returns>The steps, in the order in which they happen.</returns>
        public static List<RuleStep> GetTurnSteps()
        {
            string[] stepTexts = { Strings.HowToPlay_Step1, Strings.HowToPlay_Step2, Strings.HowToPlay_Step3 };
            Func<string, int, RuleStep> createRuleStep = CreateStep;

            return stepTexts
                .Select(createRuleStep)
                .ToList();
        }

        /// <summary>
        /// Gets the rules of the upper section of the scorecard.
        /// </summary>
        /// <returns>The rules of the numbers and of the bonus.</returns>
        public static List<CategoryRule> GetUpperRules()
        {
            return new List<CategoryRule>
            {
                CreateRule(Strings.HowToPlay_UpperRuleTitle, Strings.HowToPlay_UpperRuleText, UpperExample),
                CreateRule(Strings.Game_Category_Bonus, Strings.HowToPlay_BonusText, NoExample)
            };
        }

        /// <summary>
        /// Gets the rules of the lower section of the scorecard.
        /// </summary>
        /// <returns>The rules of the combinations.</returns>
        public static List<CategoryRule> GetLowerRules()
        {
            List<CategoryRule> rules = GetSameNumberRules();

            rules.AddRange(GetStraightAndFreeRules());

            return rules;
        }

        private static List<CategoryRule> GetSameNumberRules()
        {
            return new List<CategoryRule>
            {
                CreateRule(
                    Strings.Game_Category_FourOfAKind,
                    Strings.HowToPlay_FourOfAKindText,
                    FourOfAKindExample),
                CreateRule(
                    Strings.Game_Category_FullHouse,
                    Strings.HowToPlay_FullHouseText,
                    FullHouseExample)
            };
        }

        private static List<CategoryRule> GetStraightAndFreeRules()
        {
            return new List<CategoryRule>
            {
                CreateRule(
                    Strings.Game_Category_LittleStraight,
                    Strings.HowToPlay_LittleStraightText,
                    LittleStraightExample),
                CreateRule(
                    Strings.Game_Category_BigStraight,
                    Strings.HowToPlay_BigStraightText,
                    BigStraightExample),
                CreateRule(Strings.Game_Category_Yacht, Strings.HowToPlay_YachtText, YachtExample),
                CreateRule(Strings.Game_Category_Choice, Strings.HowToPlay_ChoiceText, ChoiceExample),
                CreateRule(Strings.Game_Category_YachtBonus, Strings.HowToPlay_YachtBonusText, YachtExample),
                CreateRule(Strings.HowToPlay_JokerTitle, Strings.HowToPlay_JokerText, NoExample)
            };
        }

        private static CategoryRule CreateRule(string title, string description, string dice)
        {
            return new CategoryRule { Title = title, Description = description, Dice = dice };
        }

        /// <summary>
        /// Creates a numbered step of the turn.
        /// </summary>
        /// <param name="text">Text of the step.</param>
        /// <param name="stepIndex">Position of the step in the list, starting at zero.</param>
        /// <returns>The step with its number, starting at one.</returns>
        private static RuleStep CreateStep(string text, int stepIndex)
        {
            return new RuleStep { Number = stepIndex + 1, Text = text };
        }
    }
}
