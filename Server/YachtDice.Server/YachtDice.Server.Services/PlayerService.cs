using System;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.Server.Logic.Players;
using YachtDice.Server.Services.Contracts;
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

        /// <summary>
        /// Initializes the service with the logic it uses.
        /// </summary>
        /// <param name="statisticsService">Logic that calculates the statistics of the players.</param>
        public PlayerService(PlayerStatisticsService statisticsService)
        {
            if (statisticsService == null)
            {
                throw new ArgumentNullException(nameof(statisticsService), NullDependencyMessage);
            }

            _statisticsService = statisticsService;
        }

        /// <summary>
        /// Gets the statistics of a player.
        /// </summary>
        /// <param name="request">Name of the player.</param>
        /// <returns>The statistics of the player.</returns>
        public Task<PlayerStatisticsDto> GetStatisticsAsync(PlayerStatisticsRequestDto request)
        {
            return ExecuteAsync(request, () => ProcessGetStatisticsAsync(request));
        }

        private async Task<PlayerStatisticsDto> ProcessGetStatisticsAsync(PlayerStatisticsRequestDto request)
        {
            PlayerStatistics statistics = await _statisticsService.GetStatisticsAsync(request.PlayerName);

            return PlayerStatisticsMapper.ToDto(statistics);
        }
    }
}