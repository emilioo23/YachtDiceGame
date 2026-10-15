using System.Collections.Generic;
using System.Threading.Tasks;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Data access for the players.
    /// </summary>
    public interface IPlayerRepository
    {
        /// <summary>
        /// Looks for the player registered with the given email.
        /// </summary>
        /// <param name="email">Email address to look for.</param>
        /// <returns>A list with the matching player, or an empty list if there is none.</returns>
        Task<List<Player>> FindByEmailAsync(string email);

        /// <summary>
        /// Looks for the player whose display name or username matches the given name.
        /// </summary>
        /// <param name="name">Display name or username to look for.</param>
        /// <returns>A list with the matching player, or an empty list if there is none.</returns>
        Task<List<Player>> FindByNameAsync(string name);

        /// <summary>
        /// Indicates whether the email is already registered.
        /// </summary>
        /// <param name="email">Email address to check.</param>
        /// <returns>True if a player already uses the email.</returns>
        Task<bool> EmailExistsAsync(string email);

        /// <summary>
        /// Indicates whether the username is already taken.
        /// </summary>
        /// <param name="username">Username to check.</param>
        /// <returns>True if a player already uses the username.</returns>
        Task<bool> UsernameExistsAsync(string username);

        /// <summary>
        /// Saves a new player in the database.
        /// </summary>
        /// <param name="player">Player to save.</param>
        /// <returns>A task that completes when the player has been saved.</returns>
        Task AddAsync(Player player);
    }
}