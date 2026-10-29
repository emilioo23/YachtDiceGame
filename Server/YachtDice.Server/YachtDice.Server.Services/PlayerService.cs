using System;
using System.Collections.Generic;
using System.ServiceModel;
using YachtDice.Server.Logic.Players;
using YachtDice.Server.Services.Mapping;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts;

    /// <summary>
    /// Implements the players' data operations on top of the logic layer.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class PlayerService : ServiceBase, Contracts.IPlayerService
    {
        private const string NullDependencyMessage = "The dependency cannot be null.";

        private readonly PlayerStatisticsService _statisticsService;
        private readonly LeaderboardService _leaderboardService;

        /// <summary>
        /// Initializes the service with the logic it uses.
        /// </summary>
        /// <param name="statisticsService">Logic that calculates the statistics of the players.</param>
        /// <param name="leaderboardService">Logic that builds the ranking of the players.</param>
        public PlayerService(PlayerStatisticsService statisticsService, LeaderboardService leaderboardService)
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
        /// <param name="username">Username of the player.</param>
        /// <returns>The statistics of the player.</returns>
        public Contracts.PlayerStatistics GetStatistics(string username)
        {
            return Execute(username, () => ProcessGetStatistics(username));
        }

        /// <summary>
        /// Gets the ranking of the best players.
        /// </summary>
        /// <returns>The ranking, from the player with most points to the one with least.</returns>
        public List<Contracts.LeaderboardEntry> GetLeaderboard()
        {
            return Execute(ProcessGetLeaderboard);
        }

        private Contracts.PlayerStatistics ProcessGetStatistics(string username)
        {
            PlayerStatistics statistics = _statisticsService.GetStatistics(username);
            return PlayerStatisticsMapper.ToContract(statistics);
        }

        private List<Contracts.LeaderboardEntry> ProcessGetLeaderboard()
        {
            List<LeaderboardEntry> entries = _leaderboardService.GetTopPlayers();
            return LeaderboardMapper.ToContracts(entries);
        }
    }
}