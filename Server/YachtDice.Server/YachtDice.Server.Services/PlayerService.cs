using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts;
    using LogicPlayers = YachtDice.Server.Logic.Players;

    /// <summary>
    /// Implements the players' data operations on top of the logic layer.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class PlayerService : ServiceBase, Contracts.IPlayerService
    {
        private const string NullDependencyMessage = "The dependency cannot be null.";

        private readonly LogicPlayers.PlayerStatisticsService _statisticsService;
        private readonly LogicPlayers.LeaderboardService _leaderboardService;

        /// <summary>
        /// Initializes the service with the logic it uses.
        /// </summary>
        /// <param name="statisticsService">Logic that calculates the statistics of the players.</param>
        /// <param name="leaderboardService">Logic that builds the ranking of the players.</param>
        public PlayerService(
            LogicPlayers.PlayerStatisticsService statisticsService,
            LogicPlayers.LeaderboardService leaderboardService)
        {
            if (statisticsService == null)
            {
                throw new ArgumentNullException(nameof(statisticsService), NullDependencyMessage);
            }

            if (leaderboardService == null)
            {
                throw new ArgumentNullException(nameof(leaderboardService), NullDependencyMessage);
            }

            _statisticsService = statisticsService;
            _leaderboardService = leaderboardService;
        }

        /// <summary>
        /// Gets the statistics of a player.
        /// </summary>
        /// <param name="player">Player whose statistics are requested. Only the username is used.</param>
        /// <returns>The statistics of the player.</returns>
        public Contracts.PlayerStatistics GetPlayerStatistics(Contracts.Player player)
        {
            return Execute(player, () => ProcessPlayerStatistics(player));
        }

        /// <summary>
        /// Gets the ranking of the best players.
        /// </summary>
        /// <returns>The ranking, from the player with most points to the one with least.</returns>
        public List<Contracts.LeaderboardEntry> GetLeaderboard()
        {
            return Execute(ProcessLeaderboard);
        }

        private Contracts.PlayerStatistics ProcessPlayerStatistics(Contracts.Player player)
        {
            LogicPlayers.PlayerStatistics statistics = _statisticsService.GetStatistics(player.Username);

            return Mapping.PlayerStatisticsMapper.ToContract(statistics);
        }

        private List<Contracts.LeaderboardEntry> ProcessLeaderboard()
        {
            List<LogicPlayers.LeaderboardEntry> entries = _leaderboardService.GetTopPlayers();

            return Mapping.LeaderboardMapper.ToContracts(entries);
        }
    }
}