using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace YachtDice.Server.Services
{
    using Contracts = YachtDice.Server.Contracts.Players;
    using LogicAvatars = YachtDice.Server.Logic.Avatars;
    using LogicPlayers = YachtDice.Server.Logic.Players;

    /// <summary>
    /// Implements the players' data operations on top of the logic layer.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class PlayerService : ServiceBase, Contracts.IPlayerService
    {
        private readonly LogicPlayers.PlayerStatisticsService _statisticsService;
        private readonly LogicAvatars.AvatarService _avatarService;

        /// <summary>
        /// Initializes the service with the logic it uses.
        /// </summary>
        /// <param name="statisticsService">Logic that calculates the statistics of the players.</param>
        /// <param name="leaderboardService">Logic that builds the ranking of the players.</param>
        public PlayerService(
            LogicPlayers.PlayerStatisticsService statisticsService,
            LogicAvatars.AvatarService avatarService)
        {
            _statisticsService = statisticsService
                ?? throw new ArgumentNullException(nameof(statisticsService));
            _avatarService = avatarService
                ?? throw new ArgumentNullException(nameof(avatarService));
        }

        /// <summary>
        /// Gets the statistics of a player.
        /// </summary>
        /// <param name="player">Player whose statistics are requested. Only the username is used.</param>
        /// <returns>The statistics of the player.</returns>
        public Contracts.PlayerStatistics GetPlayerStatistics(Contracts.Player player)
        {
            Contracts.PlayerStatistics statistics = Execute(player, () => ProcessPlayerStatistics(player));

            return statistics;
        }

        /// <summary>
        /// Gets the ranking of the best players.
        /// </summary>
        /// <returns>The ranking, from the player with most points to the one with least.</returns>
        public List<Contracts.LeaderboardEntry> GetLeaderboard()
        {
            return Execute(ProcessLeaderboard);
        }

        /// <summary>
        /// Gets the avatar, level and experience of a player.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The profile of the player.</returns>
        public Contracts.PlayerProfile GetProfile(string displayName)
        {
            Contracts.PlayerProfile profile = Execute(displayName, () => ProcessProfile(displayName));

            return profile;
        }

        /// <summary>
        /// Gets every avatar of the game with the level that each one needs.
        /// </summary>
        /// <returns>The avatars, ordered by identifier.</returns>
        public List<Contracts.UnlockableAvatar> GetAvatarCatalog()
        {
            List<Contracts.UnlockableAvatar> catalog = Execute(ProcessCatalog);

            return catalog;
        }

        /// <summary>
        /// Changes the avatar of a player. The avatar must exist and the player must have unlocked it.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="avatarId">Identifier of the avatar that the player wants to use.</param>
        /// <returns>The outcome of the change.</returns>
        public Contracts.AvatarChangeResultCode ChangeAvatar(string displayName, int avatarId)
        {
            Contracts.AvatarChangeResultCode result =
                Execute(displayName, () => ProcessAvatarChange(displayName, avatarId));

            return result;
        }

        private static List<Contracts.UnlockableAvatar> ProcessCatalog()
        {
            return Mapping.AvatarMapper.ToContracts(LogicAvatars.AvatarCatalog.GetAll());
        }

        private Contracts.PlayerStatistics ProcessPlayerStatistics(Contracts.Player player)
        {
            LogicPlayers.PlayerStatistics statistics = _statisticsService.GetStatistics(player.Username);

            return Mapping.PlayerStatisticsMapper.ToContract(statistics);
        }

        private Contracts.PlayerProfile ProcessProfile(string displayName)
        {
            LogicAvatars.PlayerProfile profile = _avatarService.GetProfile(displayName);

            return Mapping.AvatarMapper.ToContract(profile);
        }

        private Contracts.AvatarChangeResultCode ProcessAvatarChange(string displayName, int avatarId)
        {
            LogicAvatars.AvatarChangeStatus status = _avatarService.ChangeAvatar(displayName, avatarId);

            return Mapping.AvatarMapper.ToResultCode(status);
        }

        private List<Contracts.LeaderboardEntry> ProcessLeaderboard()
        {
            List<LogicPlayers.LeaderboardEntry> entries = _statisticsService.GetTopPlayers();

            return Mapping.LeaderboardMapper.ToContracts(entries);
        }
    }
}