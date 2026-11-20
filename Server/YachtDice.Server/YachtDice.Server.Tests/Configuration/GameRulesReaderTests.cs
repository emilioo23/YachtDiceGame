using System;
using System.Linq;
using System.Xml.Linq;
using Xunit;
using YachtDice.Server.Logic.Configuration;
using YachtDice.Server.Logic.Players;
using YachtDice.Server.Logic.Rooms;

namespace YachtDice.Server.Tests.Configuration
{
    /// <summary>
    /// Rules to read the game modes, the levels and the achievements from the configuration file.
    /// </summary>
    public class GameRulesReaderTests
    {
        private const int FastTurnSeconds = 30;
        private const int SecondLevelStart = 200;
        private const int SecondLevel = 2;
        private const int AchievementCount = 23;
        private const int AchievementWithoutTitleId = 22;
        private const string Modes =
            "<gameModes>"
            + "<mode speed=\"Normal\" turnSeconds=\"60\" />"
            + "<mode speed=\"Fast\" turnSeconds=\"30\" />"
            + "</gameModes>";
        private const string ValidRules =
            "<gameRules>"
            + Modes
            + "<levels><level experience=\"0\" /><level experience=\"200\" /></levels>"
            + "<achievements>"
            + "<achievement id=\"1\" metric=\"Wins\" requiredValue=\"1\" />"
            + "<achievement id=\"2\" metric=\"Experience\" requiredValue=\"10\" hasTitle=\"false\" />"
            + "<achievement id=\"3\" metric=\"WinRatePercentage\" requiredValue=\"60\" minimumGames=\"10\" />"
            + "</achievements>"
            + "</gameRules>";
        private const string WithoutAchievements =
            "<gameRules>"
            + Modes
            + "<levels><level experience=\"0\" /></levels>"
            + "</gameRules>";
        private const string UnknownMetric =
            "<gameRules>"
            + Modes
            + "<levels><level experience=\"0\" /></levels>"
            + "<achievements><achievement id=\"1\" metric=\"Yachts\" requiredValue=\"1\" /></achievements>"
            + "</gameRules>";
        private const string NegativeTurnSeconds =
            "<gameRules>"
            + "<gameModes>"
            + "<mode speed=\"Normal\" turnSeconds=\"-60\" />"
            + "<mode speed=\"Fast\" turnSeconds=\"30\" />"
            + "</gameModes>"
            + "<levels><level experience=\"0\" /></levels>"
            + "<achievements />"
            + "</gameRules>";
        private const string InvalidHasTitle =
            "<gameRules>"
            + Modes
            + "<levels><level experience=\"0\" /></levels>"
            + "<achievements>"
            + "<achievement id=\"1\" metric=\"Wins\" requiredValue=\"1\" hasTitle=\"maybe\" />"
            + "</achievements>"
            + "</gameRules>";
        private const int ValidAchievementWithoutTitleId = 2;
        private const int MinimumGamesId = 3;
        private const int MinimumGames = 10;

        [Fact]
        public void Read_ValidRules_ReadsTheSecondsOfTheFastMode()
        {
            GameRules rules = GameRulesReader.Read(XDocument.Parse(ValidRules));

            Assert.Equal(FastTurnSeconds, rules.TurnDuration.GetSeconds(GameSpeed.Fast));
        }

        [Fact]
        public void Read_ValidRules_ReadsTheLevelsInTheirOrder()
        {
            GameRules rules = GameRulesReader.Read(XDocument.Parse(ValidRules));

            Assert.Equal(SecondLevel, rules.LevelTable.GetLevel(SecondLevelStart));
        }

        [Fact]
        public void Read_AchievementWithHasTitleFalse_HasNoTitle()
        {
            GameRules rules = GameRulesReader.Read(XDocument.Parse(ValidRules));

            Assert.False(rules.Achievements.ExistsTitle(ValidAchievementWithoutTitleId));
        }

        [Fact]
        public void Read_AchievementWithMinimumGames_KeepsTheMinimum()
        {
            GameRules rules = GameRulesReader.Read(XDocument.Parse(ValidRules));

            Func<AchievementRule, bool> isMinimumGamesRule = achievement => achievement.Id == MinimumGamesId;
            AchievementRule rule = rules.Achievements.Rules.Single(isMinimumGamesRule);

            Assert.Equal(MinimumGames, rule.MinimumGames);
        }

        [Fact]
        public void Read_MissingAchievementsSection_ThrowsInvalidOperationException()
        {
            XDocument document = XDocument.Parse(WithoutAchievements);

            Assert.Throws<InvalidOperationException>(() => GameRulesReader.Read(document));
        }

        [Fact]
        public void Read_UnknownMetric_ThrowsInvalidOperationException()
        {
            XDocument document = XDocument.Parse(UnknownMetric);

            Assert.Throws<InvalidOperationException>(() => GameRulesReader.Read(document));
        }

        [Fact]
        public void Read_NegativeTurnSeconds_ThrowsInvalidOperationException()
        {
            XDocument document = XDocument.Parse(NegativeTurnSeconds);

            Assert.Throws<InvalidOperationException>(() => GameRulesReader.Read(document));
        }

        [Fact]
        public void Read_HasTitleThatIsNotTrueOrFalse_ThrowsInvalidOperationException()
        {
            XDocument document = XDocument.Parse(InvalidHasTitle);

            Assert.Throws<InvalidOperationException>(() => GameRulesReader.Read(document));
        }

        [Fact]
        public void Read_NullDocument_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => GameRulesReader.Read(null));
        }

        [Fact]
        public void Load_FileOfTheServer_ReadsEveryAchievement()
        {
            GameRules rules = GameRulesFile.Load();

            Assert.Equal(AchievementCount, rules.Achievements.Rules.Count);
        }

        [Fact]
        public void Load_FileOfTheServer_KeepsTheCollectorAchievementWithoutTitle()
        {
            GameRules rules = GameRulesFile.Load();

            Assert.False(rules.Achievements.ExistsTitle(AchievementWithoutTitleId));
        }
    }
}
