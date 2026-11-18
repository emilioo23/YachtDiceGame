using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using YachtDice.PlayerServiceReference;

namespace YachtDice.Gateway
{
    /// <summary>
    /// Single entry point of the client to the player operations of the server.
    /// </summary>
    public class PlayerGateway : GatewayBase<PlayerServiceClient, IPlayerService>
    {
        /// <summary>
        /// Gets the game statistics of a player.
        /// </summary>
        /// <param name="username">Username of the player.</param>
        /// <returns>The statistics of the player.</returns>
        public async Task<PlayerStatistics> GetStatisticsAsync(string username)
        {
            var player = new Player { Username = username };

            PlayerStatistics statistics = await ExecuteAsync(client => client.GetPlayerStatistics(player));

            return statistics;
        }

        /// <summary>
        /// Gets the leaderboard ranking entries and, when the player is not among the best ones, the own position.
        /// </summary>
        /// <param name="displayName">Name of the player who looks at the leaderboard.</param>
        /// <returns>The ranking, from the player with most points to the one with least.</returns>
        public async Task<List<LeaderboardEntry>> GetLeaderboardAsync(string displayName)
        {
            LeaderboardEntry[] entries = await ExecuteAsync(client => client.GetLeaderboard(displayName));

            return entries.ToList();
        }

        /// <summary>
        /// Gets the avatar, level and experience of a player.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The profile of the player.</returns>
        public async Task<PlayerProfile> GetProfileAsync(string displayName)
        {
            PlayerProfile profile = await ExecuteAsync(client => client.GetProfile(displayName));

            return profile;
        }

        /// <summary>
        /// Gets every avatar of the game with the level that each one needs.
        /// </summary>
        /// <returns>The avatars of the catalog.</returns>
        public async Task<List<UnlockableAvatar>> GetAvatarCatalogAsync()
        {
            UnlockableAvatar[] avatars = await ExecuteAsync(client => client.GetAvatarCatalog());

            return avatars.ToList();
        }

        /// <summary>
        /// Asks the server to change the avatar of a player.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="avatarId">Identifier of the avatar that the player wants to use.</param>
        /// <returns>The outcome of the change.</returns>
        public async Task<AvatarChangeResultCode> ChangeAvatarAsync(string sessionToken, int avatarId)
        {
            AvatarChangeResultCode result = await ExecuteAsync(client => client.ChangeAvatar(sessionToken, avatarId));

            return result;
        }

        /// <inheritdoc />
        public async Task<UsernameChangeResultCode> ChangeUsernameAsync(string sessionToken, string newUsername)
        {
            UsernameChangeResultCode result =
                await ExecuteAsync(client => client.ChangeUsername(sessionToken, newUsername));

            return result;
        }

        public async Task<List<Achievement>> GetAchievementsAsync(string playerName)
        {
            Achievement[] achievements = await ExecuteAsync(client => client.GetAchievements(playerName));

            return achievements.ToList();
        }

        public async Task<TitleChangeResultCode> ChangeTitleAsync(string sessionToken, int titleId)
        {
            TitleChangeResultCode result = await ExecuteAsync(client => client.ChangeTitle(sessionToken, titleId));

            return result;
        }

        public async Task<List<GameHistoryEntry>> GetGameHistoryAsync(string playerName)
        {
            GameHistoryEntry[] games = await ExecuteAsync(client => client.GetGameHistory(playerName));

            return games.ToList();
        }

        public async Task<List<SavedScorecard>> GetGameScorecardsAsync(string playerName, int gameId)
        {
            SavedScorecard[] scorecards = await ExecuteAsync(client => client.GetGameScorecards(playerName, gameId));

            return scorecards.ToList();
        }

        public async Task<FlagChangeResultCode> ChangeFlagAsync(string sessionToken, string flagCode)
        {
            FlagChangeResultCode result = await ExecuteAsync(client => client.ChangeFlag(sessionToken, flagCode));

            return result;
        }

        public async Task<AccountDeletionResultCode> DeleteAccountAsync(string sessionToken, string password)
        {
            AccountDeletionResultCode result =
                await ExecuteAsync(client => client.DeleteAccount(sessionToken, password));

            return result;
        }

        protected override GatewayErrorCode GetErrorCode(FaultException fault)
        {
            var typedFault = fault as FaultException<ServiceFault>;

            return GatewayErrorCodeParser.Parse(typedFault?.Detail.Code);
        }
    }
}