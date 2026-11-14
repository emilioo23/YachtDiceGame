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
        /// Gets the ranking of the best players.
        /// </summary>
        /// <returns>The ranking, from the player with most points to the one with least.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        List<LeaderboardEntry> GetLeaderboard();

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
        /// <param name="displayName">Name of the player.</param>
        /// <param name="avatarId">Identifier of the avatar that the player wants to use.</param>
        /// <returns>The outcome of the change.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        AvatarChangeResultCode ChangeAvatar(string displayName, int avatarId);

        /// <summary>
        /// Changes the username of a player after checking that the new one is valid and free.
        /// </summary>
        /// <param name="currentName">Name that the player uses now.</param>
        /// <param name="newUsername">Username that the player wants.</param>
        /// <returns>The result of the change.</returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        UsernameChangeResultCode ChangeUsername(string currentName, string newUsername);
    }
}