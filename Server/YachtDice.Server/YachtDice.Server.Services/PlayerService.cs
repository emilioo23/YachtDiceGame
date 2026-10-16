using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.Server.Logic.Players;
using YachtDice.Server.Services.Mapping;

namespace YachtDice.Server.Services
{
    /// <summary>
    /// Implements the players' data operations on top of the logic layer.
    /// </summary>
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class PlayerService : ServiceBase, IPlayerService
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
        /// <param name="request">Name of the player.</param>
        /// <returns>The statistics of the player.</returns>
        public Task<Contracts.PlayerStatistics> GetStatistics(Contracts.PlayerStatisticsFilter request)
        {
            return ExecuteAsync(request, () => ProcessGetStatisticsAsync(request));
        }

        /// <summary>
        /// Gets the ranking of the best players.
        /// </summary>
        /// <returns>The ranking, from the player with most points to the one with least.</returns>
        public Task<List<Contracts.LeaderboardEntry>> GetLeaderboard()
        {
            return ExecuteAsync(ProcessGetLeaderboardAsync);
        }

        private async Task<Contracts.PlayerStatistics> ProcessGetStatisticsAsync(Contracts.PlayerStatisticsFilter request)
        {
            PlayerStatistics statistics = await _statisticsService.GetStatisticsAsync(request.PlayerName);
            return PlayerStatisticsMapper.ToContract(statistics);
        }

        private async Task<List<Contracts.LeaderboardEntry>> ProcessGetLeaderboardAsync()
        {
            List<LeaderboardEntry> entries = await _leaderboardService.GetTopPlayersAsync();
            return LeaderboardMapper.ToContracts(entries);
        }
    }
}