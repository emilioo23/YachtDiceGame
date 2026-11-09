using System;
using System.Collections.Generic;
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
        private const string EmailMethod = "Email";
        private const string ActiveStatus = "Active";
        private const string InactiveStatus = "Inactive";
        private const string NoSecretKey = "";
        private const string EmptyValueMessage = "The value cannot be empty.";
        private const string NullPlayerMessage = "The player cannot be null.";
        private const string FindByEmailFailedMessage = "Database error occurred while searching player by email.";
        private const string FindByNameFailedMessage = "Database error occurred while searching player by name.";
        private const string FindByUsernameFailedMessage =
            "Database error occurred while searching player by username.";
        private const string EmailExistsFailedMessage = "Database error occurred while verifying email existence.";
        private const string UsernameExistsFailedMessage =
            "Database error occurred while verifying username existence.";
        private const string SaveFailedMessage = "Could not save the player due to a database failure.";
        private const string TwoFactorQueryFailedMessage =
            "Database error occurred while checking the two-step verification.";
        private const string TwoFactorUpdateFailedMessage =
            "Could not save the two-step verification setting due to a database failure.";
        private const string AvatarUpdateFailedMessage = "Could not save the avatar due to a database failure.";

        /// <summary>
        /// Finds a list of players matching the given email address.
        /// </summary>
        /// <param name="email">The email address to search for.</param>
        /// <returns>A list of matching players.</returns>
        /// <exception cref="DataAccessException">The database could not be queried.</exception>
        public List<Player> FindByEmail(string email)
        {
            ValidateText(email, nameof(email));

            List<Player> players = DatabaseOperation.Query(
                context => SelectByEmail(context, email),
                FindByEmailFailedMessage);

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

            List<Player> players = DatabaseOperation.Query(
                context => SelectByName(context, name),
                FindByNameFailedMessage);

            return players;
        }

        /// <summary>
        /// Looks for the player who owns exactly the username. The display name is not considered.
        /// </summary>
        /// <param name="username">Username to look for.</param>
        /// <returns>A list with the matching player, or an empty list if there is none.</returns>
        /// <exception cref="DataAccessException">The database could not be queried.</exception>
        public List<Player> FindByUsername(string username)
        {
            ValidateText(username, nameof(username));

            List<Player> players = DatabaseOperation.Query(
                context => SelectByUsername(context, username),
                FindByUsernameFailedMessage);

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

            bool isRegistered = DatabaseOperation.Query(
                context => IsEmailRegistered(context, email),
                EmailExistsFailedMessage);

            return isRegistered;
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

            bool isTaken = DatabaseOperation.Query(
                context => IsUsernameTaken(context, username),
                UsernameExistsFailedMessage);

            return isTaken;
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

            DatabaseOperation.Save(context => context.Player.Add(player), SaveFailedMessage);
        }

        /// <summary>
        /// Indicates whether the player has the two-step verification turned on.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>True if the player has an active two-step verification.</returns>
        /// <exception cref="DataAccessException">The database could not be queried.</exception>
        public bool HasTwoFactorEnabled(int playerId)
        {
            bool isEnabled = DatabaseOperation.Query(
                context => IsTwoFactorActive(context, playerId),
                TwoFactorQueryFailedMessage);

            return isEnabled;
        }

        /// <summary>
        /// Turns the two-step verification of the player on or off.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="isEnabled">True to turn it on, false to turn it off.</param>
        /// <exception cref="DataAccessException">The setting could not be saved.</exception>
        public void SaveTwoFactorEnabled(int playerId, bool isEnabled)
        {
            DatabaseOperation.Save(
                context => ApplyTwoFactorStatus(context, playerId, isEnabled),
                TwoFactorUpdateFailedMessage);
        }

        /// <summary>
        /// Saves the avatar that the player chose.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <param name="avatarId">Identifier of the avatar the player will use from now on.</param>
        /// <exception cref="DataAccessException">The avatar could not be saved.</exception>
        public void SaveAvatar(int playerId, int avatarId)
        {
            DatabaseOperation.Save(
                context => ApplyAvatar(context, playerId, avatarId),
                AvatarUpdateFailedMessage);
        }

        private static List<Player> SelectByEmail(YachtDiceContext context, string email)
        {
            List<Player> players = context.Player.Where(player => player.Email == email).ToList();

            return players;
        }

        private static List<Player> SelectByName(YachtDiceContext context, string name)
        {
            List<Player> players = context.Player
                .Where(player => player.DisplayName == name || player.Username == name)
                .Take(SingleResultCount)
                .ToList();

            return players;
        }

        private static List<Player> SelectByUsername(YachtDiceContext context, string username)
        {
            List<Player> players = context.Player
                .Where(player => player.Username == username)
                .Take(SingleResultCount)
                .ToList();

            return players;
        }

        private static bool IsEmailRegistered(YachtDiceContext context, string email)
        {
            bool isRegistered = context.Player.Any(player => player.Email == email);

            return isRegistered;
        }

        private static bool IsUsernameTaken(YachtDiceContext context, string username)
        {
            bool isTaken = context.Player.Any(player => player.Username == username);

            return isTaken;
        }

        private static bool IsTwoFactorActive(YachtDiceContext context, int playerId)
        {
            bool isActive = context.TwoFactorAuth.Any(
                auth => auth.PlayerId == playerId && auth.Method == EmailMethod && auth.Status == ActiveStatus);

            return isActive;
        }

        private static void ApplyTwoFactorStatus(YachtDiceContext context, int playerId, bool isEnabled)
        {
            TwoFactorAuth auth = context.TwoFactorAuth
                .FirstOrDefault(row => row.PlayerId == playerId && row.Method == EmailMethod);

            if (auth == null)
            {
                auth = new TwoFactorAuth { PlayerId = playerId, Method = EmailMethod, SecretKey = NoSecretKey };
                context.TwoFactorAuth.Add(auth);
            }

            auth.Status = isEnabled ? ActiveStatus : InactiveStatus;
        }

        private static void ApplyAvatar(YachtDiceContext context, int playerId, int avatarId)
        {
            Player player = context.Player.FirstOrDefault(row => row.Id == playerId);

            if (player != null)
            {
                player.AvatarId = avatarId;
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
