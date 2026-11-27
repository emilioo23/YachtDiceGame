using System.Collections.Generic;
using System.Linq;
using Xunit;
using YachtDice.Models;
using YachtDice.Resources;
using YachtDice.Utils;

namespace YachtDice.Client.Tests.Utils
{
    /// <summary>
    /// Content of the window that explains the rules of the game.
    /// </summary>
    public class HowToPlayContentTests
    {
        private const int FirstStep = 1;
        private const int SecondStep = 2;
        private const int ThirdStep = 3;
        private const string UpperBonusImageName = "Rule_UpperBonus.png";
        private const string JokerImageName = "Rule_Joker.png";

        [Fact]
        public void GetTurnSteps_ThreeTexts_NumbersThemFromOne()
        {
            List<RuleStep> steps = HowToPlayContent.GetTurnSteps();

            Assert.Equal(new[] { FirstStep, SecondStep, ThirdStep }, steps.Select(step => step.Number).ToArray());
        }

        [Fact]
        public void GetUpperRules_Rules_EndWithTheBonus()
        {
            List<CategoryRule> rules = HowToPlayContent.GetUpperRules();

            Assert.Equal(Strings.Game_Category_Bonus, rules.Last().Title);
        }

        [Fact]
        public void GetLowerRules_Rules_StartWithFourOfAKind()
        {
            List<CategoryRule> rules = HowToPlayContent.GetLowerRules();

            Assert.Equal(Strings.Game_Category_FourOfAKind, rules.First().Title);
        }

        [Fact]
        public void GetLowerRules_Rules_EndWithTheJoker()
        {
            List<CategoryRule> rules = HowToPlayContent.GetLowerRules();

            Assert.Equal(Strings.HowToPlay_JokerTitle, rules.Last().Title);
        }

        [Fact]
        public void GetUpperRules_Bonus_ShowsItsPicture()
        {
            List<CategoryRule> rules = HowToPlayContent.GetUpperRules();

            Assert.EndsWith(UpperBonusImageName, rules.Last().ImageSource);
        }

        [Fact]
        public void GetLowerRules_Joker_ShowsItsPicture()
        {
            List<CategoryRule> rules = HowToPlayContent.GetLowerRules();

            Assert.EndsWith(JokerImageName, rules.Last().ImageSource);
        }

        [Fact]
        public void GetLowerRules_RuleWithDice_HasNoPicture()
        {
            List<CategoryRule> rules = HowToPlayContent.GetLowerRules();

            Assert.Null(rules.First().ImageSource);
        }

        [Fact]
        public void GetUpperRules_Bonus_ShowsNoDice()
        {
            List<CategoryRule> rules = HowToPlayContent.GetUpperRules();

            Assert.Empty(rules.Last().Dice);
        }
    }
}
