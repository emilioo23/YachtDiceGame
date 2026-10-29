using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Linq;
using YachtDice.Server.Data.Exceptions;

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
        private const string FindByEmailFailedMessage = "Database error occurred while searching player by email.";
        private const string FindByNameFailedMessage = "Database error occurred while searching player by name.";
        private const string EmailExistsFailedMessage = "Database error occurred while verifying email existence.";
        private const string UsernameExistsFailedMessage =
            "Database error occurred while verifying username existence.";
        private const string UpdateFailedMessage = "Could not save player due to a database update failure.";
        private const string ConnectionFailedMessage = "Database connection error while saving player.";

        /// <summary>
        /// Finds a list of players matching the given email address.
        /// </summary>
        /// <param name="email">The email address to search for.</param>
        /// <returns>A list of matching players.</returns>
        /// <exception cref="DataAccessException">The database could not be queried.</exception>
        public List<Player> FindByEmail(string email)
        {
            ValidateText(email, nameof(email));
            List<Player> players;

            try
            {
                using (var context = new YachtDiceContext())
                {
                    players = context.Player.Where(player => player.Email == email).ToList();
                }
            }
            catch (EntityException ex)
            {
                throw new DataAccessException(FindByEmailFailedMessage, ex);
            }

            return players;
        }

        /// <summary>
        /// Looks for the player whose display name or username matches the given name.
        /// </summary>
        /// <param name="name">Display name or username to look for.</param>
        /// <returns>A list with the matching player, or an empty list if there is none.</returns>
        /// <exception cref="DataAccessException">The database could not be queried.</exception>
        public List<Player> FindByName(string name)
        {
            ValidateText(name, nameof(name));
            List<Player> players;

            try
            {
                using (var context = new YachtDiceContext())
                {
                    players = context.Player
                        .Where(player => player.DisplayName == name || player.Username == name)
                        .Take(SingleResultCount)
                        .ToList();
                }
            }
            catch (EntityException ex)
            {
                throw new DataAccessException(FindByNameFailedMessage, ex);
            }

            return players;
        }

        /// <summary>
        /// Indicates whether the email is already registered.
        /// </summary>
        /// <param name="email">Email address to check.</param>
        /// <returns>True if a player already uses the email.</returns>
        /// <exception cref="DataAccessException">The database could not be queried.</exception>
        public bool EmailExists(string email)
        {
            ValidateText(email, nameof(email));
            bool exists;

            try
            {
                using (var context = new YachtDiceContext())
                {
                    exists = context.Player.Any(player => player.Email == email);
                }
            }
            catch (EntityException ex)
            {
                throw new DataAccessException(EmailExistsFailedMessage, ex);
            }

            return exists;
        }

        /// <summary>
        /// Indicates whether the username is already taken.
        /// </summary>
        /// <param name="username">Username to check.</param>
        /// <returns>True if a player already uses the username.</returns>
        /// <exception cref="DataAccessException">The database could not be queried.</exception>
        public bool UsernameExists(string username)
        {
            ValidateText(username, nameof(username));
            bool exists;

            try
            {
                using (var context = new YachtDiceContext())
                {
                    exists = context.Player.Any(player => player.Username == username);
                }
            }
            catch (EntityException ex)
            {
                throw new DataAccessException(UsernameExistsFailedMessage, ex);
            }

            return exists;
        }

        /// <summary>
        /// Saves a new player in the database.
        /// </summary>
        /// <param name="player">Player entity to save.</param>
        /// <exception cref="DataAccessException">The player could not be saved.</exception>
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
                throw new DataAccessException(UpdateFailedMessage, ex);
            }
            catch (EntityException ex)
            {
                throw new DataAccessException(ConnectionFailedMessage, ex);
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