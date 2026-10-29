using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Linq;

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
        /// Finds a list of players matching the given email address.
        /// </summary>
        /// <param name="email">The email address to search for.</param>
        /// <returns>A list of matching players.</returns>
        public List<Player> FindByEmail(string email)
        {
            ValidateText(email, nameof(email));

            try
            {
                using (var context = new YachtDiceContext())
                {
                    return context.Player.Where(player => player.Email == email).ToList();
                }
            }
            catch (EntityException ex)
            {
                throw new DataException("Database error occurred while searching player by email.", ex);
            }
        }

        /// <summary>
        /// Looks for the player whose display name or username matches the given name.
        /// </summary>
        /// <param name="name">Display name or username to look for.</param>
        /// <returns>A list with the matching player, or an empty list if there is none.</returns>
        public List<Player> FindByName(string name)
        {
            ValidateText(name, nameof(name));

            try
            {
                using (var context = new YachtDiceContext())
                {
                    return context.Player
                        .Where(player => player.DisplayName == name || player.Username == name)
                        .Take(SingleResultCount)
                        .ToList();
                }
            }
            catch (EntityException ex)
            {
                throw new DataException("Database error occurred while searching player by name.", ex);
            }
        }

        /// <summary>
        /// Indicates whether the email is already registered.
        /// </summary>
        /// <param name="email">Email address to check.</param>
        /// <returns>True if a player already uses the email.</returns>
        public bool EmailExists(string email)
        {
            ValidateText(email, nameof(email));

            try
            {
                using (var context = new YachtDiceContext())
                {
                    return context.Player.Any(player => player.Email == email);
                }
            }
            catch (EntityException ex)
            {
                throw new DataException("Database error occurred while verifying email existence.", ex);
            }
        }

        /// <summary>
        /// Indicates whether the username is already taken.
        /// </summary>
        /// <param name="username">Username to check.</param>
        /// <returns>True if a player already uses the username.</returns>
        public bool UsernameExists(string username)
        {
            ValidateText(username, nameof(username));

            try
            {
                using (var context = new YachtDiceContext())
                {
                    return context.Player.Any(player => player.Username == username);
                }
            }
            catch (EntityException ex)
            {
                throw new DataException("Database error occurred while verifying username existence.", ex);
            }
        }

        /// <summary>
        /// Saves a new player in the database.
        /// </summary>
        /// <param name="player">Player entity to save.</param>
        public void Add(Player player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player), NullPlayerMessage);
            }

            try
            {
                using (var context = new YachtDiceContext())
                {
                    context.Player.Add(player);
                    context.SaveChanges();
                }
            }
            catch (DbUpdateException ex)
            {
                throw new DataException("Could not save player due to a database update failure.", ex);
            }
            catch (EntityException ex)
            {
                throw new DataException("Database connection error while saving player.", ex);
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