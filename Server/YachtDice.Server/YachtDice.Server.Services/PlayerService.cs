using System;
using System.Collections.Generic;
using System.Linq;
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
        private const string NullDependencyMessage = "A dependency of the player service cannot be null.";

        private readonly LogicPlayers.PlayerStatisticsService _statisticsService;
        private readonly LogicAvatars.AvatarService _avatarService;
        private readonly LogicPlayers.ProfileService _profileService;

        /// <summary>
        /// Initializes the service with the logic it uses.
        /// </summary>
        /// <param name="statisticsService">Logic that calculates the statistics of the players.</param>
        /// <param name="leaderboardService">Logic that builds the ranking of the players.</param>
        public PlayerService(
            LogicPlayers.PlayerStatisticsService statisticsService,
            LogicAvatars.AvatarService avatarService,
            LogicPlayers.ProfileService profileService)
        {
            _statisticsService = statisticsService
                ?? throw new ArgumentNullException(nameof(statisticsService), NullDependencyMessage);
            _avatarService = avatarService
                ?? throw new ArgumentNullException(nameof(avatarService), NullDependencyMessage);
            _profileService = profileService
                ?? throw new ArgumentNullException(nameof(profileService), NullDependencyMessage);
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
        /// Gets the ranking of the best players and, when it is not among them, the position of the player who asks.
        /// </summary>
        /// <param name="displayName">Name of the player who looks at the ranking.</param>
        /// <returns>The ranking, from the player with most points to the one with least.</returns>
        public List<Contracts.LeaderboardEntry> GetLeaderboard(string displayName)
        {
            return Execute(displayName, () => ProcessLeaderboard(displayName));
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
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="avatarId">Identifier of the avatar that the player wants to use.</param>
        /// <returns>The outcome of the change.</returns>
        public Contracts.AvatarChangeResultCode ChangeAvatar(string sessionToken, int avatarId)
        {
            Contracts.AvatarChangeResultCode result =
                ExecuteForPlayer(sessionToken, playerName => ProcessAvatarChange(playerName, avatarId));

            return result;
        }

        public Contracts.UsernameChangeResultCode ChangeUsername(string sessionToken, string newUsername)
        {
            Contracts.UsernameChangeResultCode result =
                ExecuteForPlayer(sessionToken, playerName => ProcessUsernameChange(playerName, newUsername));

            return result;
        }

        public List<Contracts.Achievement> GetAchievements(string playerName)
        {
            List<Contracts.Achievement> achievements =
                Execute(playerName, () => ProcessAchievements(playerName));

            return achievements;
        }

        public Contracts.TitleChangeResultCode ChangeTitle(string sessionToken, int titleId)
        {
            Contracts.TitleChangeResultCode result =
                ExecuteForPlayer(sessionToken, playerName => ProcessTitleChange(playerName, titleId));

            return result;
        }

        public List<Contracts.GameHistoryEntry> GetGameHistory(string playerName)
        {
            List<Contracts.GameHistoryEntry> history = Execute(playerName, () => ProcessGameHistory(playerName));

            return history;
        }

        public List<Contracts.SavedScorecard> GetGameScorecards(string playerName, int gameId)
        {
            List<Contracts.SavedScorecard> scorecards = Execute(
                playerName,
                () => Mapping.PlayerStatisticsMapper.ToScorecardContracts(
                    _statisticsService.GetScorecards(playerName, gameId)));

            return scorecards;
        }

        public Contracts.FlagChangeResultCode ChangeFlag(string sessionToken, string flagCode)
        {
            Contracts.FlagChangeResultCode result =
                ExecuteForPlayer(sessionToken, playerName => ProcessFlagChange(playerName, flagCode));

            return result;
        }

        /// <summary>
        /// Deletes the account of a player with all its data.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="password">Password that the player typed to confirm the deletion.</param>
        /// <returns>The result of the deletion.</returns>
        public Contracts.AccountDeletionResultCode DeleteAccount(string sessionToken, string password)
        {
            Contracts.AccountDeletionResultCode result =
                ExecuteForPlayer(sessionToken, playerName => ProcessAccountDeletion(playerName, password));

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

        private Contracts.UsernameChangeResultCode ProcessUsernameChange(string currentName, string newUsername)
        {
            LogicPlayers.UsernameChangeStatus status = _profileService.ChangeUsername(currentName, newUsername);

            if (status == LogicPlayers.UsernameChangeStatus.Success)
            {
                Sessions.Rename(currentName, newUsername.Trim());
            }

            return Mapping.EnumMapper
                .Map<LogicPlayers.UsernameChangeStatus, Contracts.UsernameChangeResultCode>(status);
        }

        private Contracts.FlagChangeResultCode ProcessFlagChange(string playerName, string flagCode)
        {
            LogicPlayers.FlagChangeStatus status = _profileService.ChangeFlag(playerName, flagCode);

            return Mapping.EnumMapper.Map<LogicPlayers.FlagChangeStatus, Contracts.FlagChangeResultCode>(status);
        }

        private Contracts.AccountDeletionResultCode ProcessAccountDeletion(string playerName, string password)
        {
            LogicPlayers.AccountDeletionStatus status = _profileService.DeleteAccount(playerName, password);

            if (status == LogicPlayers.AccountDeletionStatus.Success)
            {
                Sessions.EndFor(playerName);
            }

            return Mapping.EnumMapper.Map<LogicPlayers.AccountDeletionStatus, Contracts.AccountDeletionResultCode>(
                status);
        }

        private List<Contracts.GameHistoryEntry> ProcessGameHistory(string playerName)
        {
            List<LogicPlayers.PlayedGameSummary> games = _statisticsService.GetRecentGames(playerName);

            return Mapping.PlayerStatisticsMapper.ToHistoryContracts(games);
        }

        private List<Contracts.Achievement> ProcessAchievements(string playerName)
        {
            List<LogicPlayers.AchievementStatus> achievements = _profileService.GetAchievements(playerName);

            return achievements.Select(ToAchievementContract).ToList();
        }

        private Contracts.TitleChangeResultCode ProcessTitleChange(string playerName, int titleId)
        {
            LogicPlayers.TitleChangeStatus status = _profileService.ChangeTitle(playerName, titleId);

            return Mapping.EnumMapper.Map<LogicPlayers.TitleChangeStatus, Contracts.TitleChangeResultCode>(status);
        }

        private List<Contracts.LeaderboardEntry> ProcessLeaderboard(string displayName)
        {
            List<LogicPlayers.LeaderboardEntry> entries = _statisticsService.GetTopPlayers(displayName);

            return entries.Select(ToLeaderboardContract).ToList();
        }

        private static Contracts.Achievement ToAchievementContract(LogicPlayers.AchievementStatus achievement)
        {
            var contract = new Contracts.Achievement
            {
                Id = achievement.Id,
                IsUnlocked = achievement.IsUnlocked,
                HasTitle = achievement.HasTitle
            };

            return contract;
        }

        private static Contracts.LeaderboardEntry ToLeaderboardContract(LogicPlayers.LeaderboardEntry entry)
        {
            var contract = new Contracts.LeaderboardEntry
            {
                Position = entry.Position,
                DisplayName = entry.DisplayName,
                AvatarId = entry.AvatarId,
                TotalPoints = entry.TotalPoints,
                Wins = entry.Wins,
                GamesPlayed = entry.GamesPlayed,
                Level = entry.Level
            };

            return contract;
        }
    }
}