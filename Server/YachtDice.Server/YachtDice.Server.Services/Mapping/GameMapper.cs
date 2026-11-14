using System.Linq;

namespace YachtDice.Server.Services.Mapping
{
    using Contracts = YachtDice.Server.Contracts.Game;
    using LogicGame = YachtDice.Server.Logic.Game;

    /// <summary>
    /// Converts the games of the logic layer into data contracts and the other way around.
    /// </summary>
    internal static class GameMapper
    {
        /// <summary>
        /// Creates the data contract that is sent to the clients.
        /// </summary>
        /// <param name="snapshot">State of the game in the logic layer.</param>
        /// <param name="turnSecondsLeft">Seconds left for the current turn.</param>
        /// <returns>The state of the game for the clients.</returns>
        public static Contracts.GameState ToContract(LogicGame.GameSnapshot snapshot, int turnSecondsLeft)
        {
            var state = new Contracts.GameState
            {
                Status = EnumMapper.Map<LogicGame.GameStatus, Contracts.GameStatus>(snapshot.Status),
                Version = snapshot.Version,
                CurrentPlayerId = snapshot.CurrentPlayerId,
                TurnNumber = snapshot.TurnNumber,
                Round = snapshot.Round,
                RollsLeft = snapshot.RollsLeft,
                TurnSeconds = snapshot.TurnSeconds,
                TurnSecondsLeft = turnSecondsLeft,
                Dice = snapshot.Dice.ToList(),
                HeldDice = snapshot.HeldDice.ToList(),
                Players = snapshot.Players.Select(ToContract).ToList(),
                Options = snapshot.Options.Select(ToContract).ToList()
            };

            return state;
        }

        /// <summary>
        /// Creates the state that is sent when a player could not join a game.
        /// </summary>
        /// <returns>A state without players.</returns>
        public static Contracts.GameState CreateEmptyState()
        {
            return ToContract(new LogicGame.GameSnapshot(), 0);
        }

        /// <summary>
        /// Converts the outcome of an action into the code that is sent to the clients.
        /// </summary>
        /// <param name="status">Outcome in the logic layer.</param>
        /// <returns>The code for the clients.</returns>
        public static Contracts.GameResultCode ToResultCode(LogicGame.GameActionStatus status)
        {
            return EnumMapper.Map<LogicGame.GameActionStatus, Contracts.GameResultCode>(status);
        }

        /// <summary>
        /// Converts a box sent by a client into the box of the logic layer.
        /// </summary>
        /// <param name="category">Box received from the client.</param>
        /// <returns>The box for the logic layer.</returns>
        public static LogicGame.ScoreCategory ToLogic(Contracts.ScoreCategory category)
        {
            return EnumMapper.Map<Contracts.ScoreCategory, LogicGame.ScoreCategory>(category);
        }

        private static Contracts.PlayerScorecard ToContract(LogicGame.PlayerSnapshot player)
        {
            var scorecard = new Contracts.PlayerScorecard
            {
                PlayerId = player.PlayerId,
                Name = player.Name,
                IsPresent = player.IsPresent,
                UpperSubtotal = player.UpperSubtotal,
                UpperBonus = player.UpperBonus,
                YachtBonus = player.YachtBonus,
                Total = player.Total,
                IsWinner = player.IsWinner,
                ExperienceGained = player.ExperienceGained,
                Scores = player.Scores.Select(ToContract).ToList()
            };

            return scorecard;
        }

        private static Contracts.CategoryScore ToContract(LogicGame.ScoreEntry entry)
        {
            return new Contracts.CategoryScore
            {
                Category = EnumMapper.Map<LogicGame.ScoreCategory, Contracts.ScoreCategory>(entry.Category),
                IsFilled = entry.IsFilled,
                Points = entry.Points
            };
        }

        private static Contracts.ScoreChoice ToContract(LogicGame.ScoreOption option)
        {
            return new Contracts.ScoreChoice
            {
                Category = EnumMapper.Map<LogicGame.ScoreCategory, Contracts.ScoreCategory>(option.Category),
                Points = option.Points
            };
        }
    }
}
