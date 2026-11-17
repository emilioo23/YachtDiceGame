using System;
using YachtDice.Server.Logic.Game;
using YachtDice.Server.Logic.Players;

namespace YachtDice.Server.Logic.Configuration
{
    /// <summary>
    /// Rules of the game that are read from the configuration file: the seconds of a turn for each game mode, the
    /// experience of each level and the achievements.
    /// </summary>
    public sealed class GameRules
    {
        private const string NullRuleMessage = "A rule of the game cannot be null.";

        /// <summary>
        /// Initializes the rules of the game.
        /// </summary>
        /// <param name="turnDuration">Seconds that a turn lasts in each game mode.</param>
        /// <param name="levelTable">Experience that each level needs.</param>
        /// <param name="achievements">Achievements that the players can unlock.</param>
        /// <exception cref="ArgumentNullException">Any of the parameters is null.</exception>
        public GameRules(TurnDuration turnDuration, LevelTable levelTable, AchievementCatalog achievements)
        {
            TurnDuration = turnDuration ?? throw new ArgumentNullException(nameof(turnDuration), NullRuleMessage);
            LevelTable = levelTable ?? throw new ArgumentNullException(nameof(levelTable), NullRuleMessage);
            Achievements = achievements ?? throw new ArgumentNullException(nameof(achievements), NullRuleMessage);
        }

        /// <summary>
        /// Gets the seconds that a turn lasts in each game mode.
        /// </summary>
        public TurnDuration TurnDuration { get; }

        /// <summary>
        /// Gets the experience that each level needs.
        /// </summary>
        public LevelTable LevelTable { get; }

        /// <summary>
        /// Gets the achievements that the players can unlock.
        /// </summary>
        public AchievementCatalog Achievements { get; }
    }
}
