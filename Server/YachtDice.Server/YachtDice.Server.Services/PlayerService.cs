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

        public List<Contracts.Achievement> GetAchievements(string sessionToken, string playerName)
        {
            List<Contracts.Achievement> achievements =
                Execute(playerName, () => ProcessAchievements(ResolvePlayerName(sessionToken, playerName)));

            return achievements;
        }

        public Contracts.TitleChangeResultCode ChangeTitle(string sessionToken, int titleId)
        {
            Contracts.TitleChangeResultCode result =
                ExecuteForPlayer(sessionToken, playerName => ProcessTitleChange(playerName, titleId));

            return result;
        }

        public List<Contracts.GameHistoryEntry> GetGameHistory(string sessionToken, string playerName)
        {
            List<Contracts.GameHistoryEntry> history =
                Execute(playerName, () => ProcessGameHistory(ResolvePlayerName(sessionToken, playerName)));

            return history;
        }

        public List<Contracts.SavedScorecard> GetGameScorecards(string sessionToken, int gameId)
        {
            List<Contracts.SavedScorecard> scorecards = ExecuteForPlayer(
                sessionToken,
                playerName => ToScorecardContracts(_statisticsService.GetScorecards(playerName, gameId)));

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
            return ToContracts(LogicAvatars.AvatarService.GetAll());
        }

        private Contracts.PlayerStatistics ProcessPlayerStatistics(Contracts.Player player)
        {
            LogicPlayers.PlayerStatistics statistics = _statisticsService.GetStatistics(player.Username);

            return ToContract(statistics);
        }

        private Contracts.PlayerProfile ProcessProfile(string displayName)
        {
            LogicAvatars.PlayerProfile profile = _avatarService.GetProfile(displayName);

            return ToContract(profile);
        }

        private Contracts.AvatarChangeResultCode ProcessAvatarChange(string displayName, int avatarId)
        {
            LogicAvatars.AvatarChangeStatus status = _avatarService.ChangeAvatar(displayName, avatarId);

            return ToResultCode(status);
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

            return ToHistoryContracts(games);
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

        /// <summary>
        /// Creates the profile that is sent back to the client.
        /// </summary>
        /// <param name="profile">Profile produced by the logic layer.</param>
        /// <returns>Answer for the client.</returns>
        private static Contracts.PlayerProfile ToContract(LogicAvatars.PlayerProfile profile)
        {
            var contract = new Contracts.PlayerProfile
            {
                AvatarId = profile.AvatarId,
                Level = profile.Level,
                Experience = profile.Experience,
                LevelStartExperience = profile.LevelStartExperience,
                NextLevelExperience = profile.NextLevelExperience,
                TitleId = profile.TitleId,
                FlagCode = profile.FlagCode
            };

            return contract;
        }

        /// <summary>
        /// Creates the list of avatars that is sent back to the client.
        /// </summary>
        /// <param name="avatars">Avatars of the catalog.</param>
        /// <returns>Answer for the client.</returns>
        private static List<Contracts.UnlockableAvatar> ToContracts(List<LogicAvatars.UnlockableAvatar> avatars)
        {
            Func<LogicAvatars.UnlockableAvatar, Contracts.UnlockableAvatar> createUnlockableAvatar = ToContract;
            List<Contracts.UnlockableAvatar> contracts = avatars
                .Select(createUnlockableAvatar)
                .ToList();

            return contracts;
        }

        /// <summary>
        /// Converts the result of changing an avatar into the code that the client receives.
        /// </summary>
        /// <param name="status">Result produced by the logic layer.</param>
        /// <returns>The code for the client.</returns>
        private static Contracts.AvatarChangeResultCode ToResultCode(LogicAvatars.AvatarChangeStatus status)
        {
            return Mapping.EnumMapper.Map<LogicAvatars.AvatarChangeStatus, Contracts.AvatarChangeResultCode>(status);
        }

        /// <summary>
        /// Converts an avatar of the catalog into the version that travels to the clients.
        /// </summary>
        /// <param name="avatar">Avatar of the catalog in the logic layer.</param>
        /// <returns>The avatar for the clients.</returns>
        private static Contracts.UnlockableAvatar ToContract(LogicAvatars.UnlockableAvatar avatar)
        {
            var contract = new Contracts.UnlockableAvatar
            {
                Id = avatar.Id,
                RequiredLevel = avatar.RequiredLevel
            };

            return contract;
        }
    

        /// <summary>
        /// Creates the data contract that is sent back to the client.
        /// </summary>
        /// <param name="statistics">Statistics produced by the logic layer.</param>
        /// <returns>Answer for the client.</returns>
        private static Contracts.PlayerStatistics ToContract(LogicPlayers.PlayerStatistics statistics)
        {
            var contract = new Contracts.PlayerStatistics
            {
                GamesPlayed = statistics.GamesPlayed,
                Wins = statistics.Wins,
                BestScore = statistics.BestScore,
                WinRatePercentage = statistics.WinRatePercentage,
                Level = statistics.Level,
                Experience = statistics.Experience,
                GamesAgainstComputer = statistics.GamesAgainstComputer,
                WinsAgainstComputer = statistics.WinsAgainstComputer
            };

            return contract;
        }

        /// <summary>
        /// Converts the recent games of a player into the version that travels to the clients.
        /// </summary>
        /// <param name="games">Games of the logic layer.</param>
        /// <returns>The games for the clients.</returns>
        private static List<Contracts.GameHistoryEntry> ToHistoryContracts(List<LogicPlayers.PlayedGameSummary> games)
        {
            Func<LogicPlayers.PlayedGameSummary, Contracts.GameHistoryEntry> createEntry = ToHistoryContract;

            return games.Select(createEntry).ToList();
        }

        /// <summary>
        /// Converts the scorecards of a past game into the objects that travel to the clients.
        /// </summary>
        /// <param name="scorecards">Scorecards of the logic layer.</param>
        /// <returns>The scorecards for the clients.</returns>
        private static List<Contracts.SavedScorecard> ToScorecardContracts(List<LogicPlayers.SavedScorecard> scorecards)
        {
            Func<LogicPlayers.SavedScorecard, Contracts.SavedScorecard> createScorecard = ToScorecardContract;

            return scorecards.Select(createScorecard).ToList();
        }

        private static Contracts.SavedScorecard ToScorecardContract(LogicPlayers.SavedScorecard scorecard)
        {
            Func<LogicPlayers.SavedScore, Contracts.SavedScore> createScore = score => new Contracts.SavedScore
            {
                Category = score.Category,
                Points = score.Points,
                IsUsed = score.IsUsed
            };

            return new Contracts.SavedScorecard
            {
                PlayerName = scorecard.PlayerName,
                Total = scorecard.Total,
                HasUpperBonus = scorecard.HasUpperBonus,
                YachtBonusCount = scorecard.YachtBonusCount,
                Scores = scorecard.Scores.Select(createScore).ToList()
            };
        }

        private static Contracts.GameHistoryEntry ToHistoryContract(LogicPlayers.PlayedGameSummary game)
        {
            var contract = new Contracts.GameHistoryEntry
            {
                PlayedAt = game.PlayedAt,
                GameId = game.GameId,
                IsWin = game.IsWin,
                OwnScore = game.OwnScore,
                BestOpponentScore = game.BestOpponentScore,
                Opponents = game.Opponents,
                IsAgainstComputer = game.IsAgainstComputer
            };

            return contract;
        }
    
    }
}