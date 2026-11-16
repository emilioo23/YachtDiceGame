using System.Collections.Generic;
using System.ServiceModel;

namespace YachtDice.Server.Contracts.Players
{
    /// <summary>
    /// Operations that the clients can use to query the players' data.
    /// </summary>
    [ServiceContract(Namespace = ServiceNamespaces.Contracts)]
    public interface IPlayerService
    {
        /// <summary>
        /// Gets the statistics of a player.
        /// </summary>
        /// <param name="player">Player whose statistics are requested. Only the username is used.</param>
        /// <returns>The statistics of the player.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        PlayerStatistics GetPlayerStatistics(Player player);

        /// <summary>
        /// Gets the ranking of the best players and, when it is not among them, the position of the player who asks.
        /// </summary>
        /// <param name="displayName">Name of the player who looks at the ranking.</param>
        /// <returns>The ranking, from the player with most points to the one with least.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        List<LeaderboardEntry> GetLeaderboard(string displayName);

        /// <summary>
        /// Gets the avatar, level and experience of a player.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>The profile of the player.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        PlayerProfile GetProfile(string displayName);

        /// <summary>
        /// Gets every avatar of the game with the level that each one needs.
        /// </summary>
        /// <returns>The avatars, ordered by identifier.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        List<UnlockableAvatar> GetAvatarCatalog();

        /// <summary>
        /// Changes the avatar of a player. The avatar must exist and the player must have unlocked it.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="avatarId">Identifier of the avatar that the player wants to use.</param>
        /// <returns>The outcome of the change.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        AvatarChangeResultCode ChangeAvatar(string sessionToken, int avatarId);

        /// <summary>
        /// Changes the username of a player after checking that the new one is valid and free.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="newUsername">Username that the player wants.</param>
        /// <returns>The result of the change.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        UsernameChangeResultCode ChangeUsername(string sessionToken, string newUsername);

        /// <summary>
        /// Gets every achievement and whether the player has unlocked it.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        /// <returns>The achievements, in the order in which they are shown.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        List<AchievementInfo> GetAchievements(string playerName);

        /// <summary>
        /// Chooses the title that the player shows in the profile. The player must have unlocked it.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="titleId">Identifier of the title.</param>
        /// <returns>The result of the change.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        TitleChangeResultCode ChangeTitle(string sessionToken, int titleId);

        /// <summary>
        /// Gets the most recent games of a player, from the newest to the oldest.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        /// <returns>The games of the player.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        List<GameHistoryEntry> GetGameHistory(string playerName);

        /// <summary>
        /// Gets the scorecards of a past game. Only a player who took part in the game can see them.
        /// </summary>
        /// <param name="playerName">Name of the player who asks.</param>
        /// <param name="gameId">Identifier of the game.</param>
        /// <returns>The scorecards of the registered players, or an empty list.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        List<SavedScorecard> GetGameScorecards(string playerName, int gameId);

        /// <summary>
        /// Chooses the country flag that the player shows next to the avatar. Any country can be chosen.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="flagCode">Code of the country with two letters, or an empty text to remove the flag.</param>
        /// <returns>The result of the change.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        FlagChangeResultCode ChangeFlag(string sessionToken, string flagCode);

        /// <summary>
        /// Deletes the account of a player with all its data. It cannot be undone.
        /// </summary>
        /// <param name="sessionToken">Token of the session of the player.</param>
        /// <param name="password">Password that the player typed to confirm the deletion.</param>
        /// <returns>The result of the deletion.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        AccountDeletionResultCode DeleteAccount(string sessionToken, string password);
    }
}