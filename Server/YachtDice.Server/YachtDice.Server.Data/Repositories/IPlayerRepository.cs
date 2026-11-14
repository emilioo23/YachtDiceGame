using System.Collections.Generic;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Data access operations for players.
    /// </summary>
    public interface IPlayerRepository
    {
        /// <summary>
        /// Finds a list of players matching the given email address.
        /// </summary>
        /// <param name="email">The email address to search for.</param>
        /// <returns>A list of matching players.</returns>
        List<Player> FindByEmail(string email);

        /// <summary>
        /// Looks for the player whose display name or username matches the given name.
        /// </summary>
        /// <param name="name">Display name or username to look for.</param>
        /// <returns>A list with the matching player, or an empty list if there is none.</returns>
        List<Player> FindByName(string name);

        /// <summary>
        /// Looks for the player who owns exactly the username. The display name is not considered.
        /// </summary>
        /// <param name="username">Username to look for.</param>
        /// <returns>A list with the matching player, or an empty list if there is none.</returns>
        List<Player> FindByUsername(string username);

        /// <summary>
        /// Indicates whether the email is already registered.
        /// </summary>
        /// <param name="email">Email address to check.</param>
        /// <returns>True if a player already uses the email.</returns>
        bool EmailExists(string email);

        /// <summary>
        /// Indicates whether the username is already taken.
        /// </summary>
        /// <param name="username">Username to check.</param>
        /// <returns>True if a player already uses the username.</returns>
        bool UsernameExists(string username);

        /// <summary>
        /// Saves a new player in the database.
        /// </summary>
        /// <param name="player">Player entity to save.</param>
        void Add(Player player);

        /// <summary>
        /// Indicates whether the player has the two-step verification turned on.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>True if the player has an active two-step verification.</returns>
        bool HasTwoFactorEnabled(int playerId);

        /// <summary>
        /// Turns the two-step verification of the player on or off.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="isEnabled">True to turn it on, false to turn it off.</param>
        void SaveTwoFactorEnabled(int playerId, bool isEnabled);

        /// <summary>
        /// Saves the avatar that the player chose.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="avatarId">Identifier of the avatar the player will use from now on.</param>
        void SaveAvatar(int playerId, int avatarId);

        /// <summary>
        /// Saves the new username of a player, which is also the name that the other players see.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="username">New username of the player.</param>
        /// <exception cref="DataAccessException">The username could not be saved.</exception>
        void SaveUsername(int playerId, string username);

        /// <summary>
        /// Finds the title that the player chose to show in the profile.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>The saved title, or an empty text if the player never chose one.</returns>
        /// <exception cref="DataAccessException">The database could not be queried.</exception>
        string FindTitle(int playerId);

        /// <summary>
        /// Saves the title that the player chose to show in the profile.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="title">Title to save.</param>
        /// <exception cref="DataAccessException">The title could not be saved.</exception>
        void SaveTitle(int playerId, string title);

        /// <summary>
        /// Finds the code of the country flag that the player chose to show in the profile.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>The saved code, or an empty text if the player never chose a flag.</returns>
        /// <exception cref="DataAccessException">The database could not be queried.</exception>
        string FindFlagCode(int playerId);

        /// <summary>
        /// Saves the code of the country flag that the player chose to show in the profile.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="flagCode">Code of the country, or an empty text to remove the flag.</param>
        /// <exception cref="DataAccessException">The flag could not be saved.</exception>
        void SaveFlagCode(int playerId, string flagCode);
    }
}