using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace YachtDice.Server.Data.Repositories
{
    /// <summary>
    /// Entity Framework implementation of the player data access.
    /// </summary>
    public class PlayerRepository : IPlayerRepository
    {
        private const int SingleResultCount = 1;
        private const string EmptyValueMessage = "The value cannot be empty.";
        private const string NullPlayerMessage = "The player cannot be null.";

        /// <summary>
        /// Looks for the player registered with the given email.
        /// </summary>
        /// <param name="email">Email address to look for.</param>
        /// <returns>A list with the matching player, or an empty list if there is none.</returns>
        public async Task<List<Player>> FindByEmailAsync(string email)
        {
            ValidateText(email, nameof(email));

            using (var context = new YachtDiceContext())
            {
                return await context.Player
                    .Where(player => player.Email == email)
                    .Take(SingleResultCount)
                    .ToListAsync();
            }
        }

        /// <summary>
        /// Indicates whether the email is already registered.
        /// </summary>
        /// <param name="email">Email address to check.</param>
        /// <returns>True if a player already uses the email.</returns>
        public async Task<bool> EmailExistsAsync(string email)
        {
            ValidateText(email, nameof(email));

            using (var context = new YachtDiceContext())
            {
                return await context.Player.AnyAsync(player => player.Email == email);
            }
        }

        /// <summary>
        /// Indicates whether the username is already taken.
        /// </summary>
        /// <param name="username">Username to check.</param>
        /// <returns>True if a player already uses the username.</returns>
        public async Task<bool> UsernameExistsAsync(string username)
        {
            ValidateText(username, nameof(username));

            using (var context = new YachtDiceContext())
            {
                return await context.Player.AnyAsync(player => player.Username == username);
            }
        }

        /// <summary>
        /// Saves a new player in the database.
        /// </summary>
        /// <param name="player">Player to save.</param>
        /// <returns>A task that completes when the player has been saved.</returns>
        public async Task AddAsync(Player player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player), NullPlayerMessage);
            }

            using (var context = new YachtDiceContext())
            {
                context.Player.Add(player);
                await context.SaveChangesAsync();
            }
        }

        private static void ValidateText(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(EmptyValueMessage, parameterName);
            }
        }
    }
}