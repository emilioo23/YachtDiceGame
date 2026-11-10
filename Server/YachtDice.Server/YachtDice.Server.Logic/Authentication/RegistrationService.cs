using System;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Logging;
using YachtDice.Server.Logic.Players;
using YachtDice.Server.Logic.Security;

namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Validates and registers new players synchronously.
    /// </summary>
    public class RegistrationService : IRegistrationService
    {
        private const int DefaultAvatarId = 1;
        private const int InitialExperience = 0;
        private const int InitialLevel = 1;
        private const int UsernameMaxLength = 20;
        private const int EmailMaxLength = 100;
        private const int NameMaxLength = 50;
        private const string RegisteredLogMessage = "Player registered successfully.";
        private const string NullPlayerMessage = "The new player cannot be null.";
        private const string NullRepositoryMessage = "The player repository cannot be null.";

        private readonly AppLogger _logger;
        private readonly IPlayerRepository _playerRepository;

        /// <summary>
        /// Initializes the service with the data access it needs.
        /// </summary>
        /// <param name="playerRepository">Repository used to read and save players.</param>
        public RegistrationService(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository
                ?? throw new ArgumentNullException(nameof(playerRepository), NullRepositoryMessage);
            _logger = new AppLogger(GetType());
        }

        /// <summary>
        /// Validates the data and registers the player if everything is correct.
        /// </summary>
        /// <param name="newPlayer">Data of the new player.</param>
        /// <param name="plainPassword">The player's plain text password.</param>
        /// <returns>Result of the registration attempt.</returns>
        public RegistrationResult RegisterPlayer(Player newPlayer, string plainPassword)
        {
            if (newPlayer == null)
            {
                throw new ArgumentNullException(nameof(newPlayer), NullPlayerMessage);
            }

            RegistrationResult result = ValidatePlayer(newPlayer, plainPassword);

            if (result == RegistrationResult.Success)
            {
                result = SavePlayer(newPlayer, plainPassword);
            }

            if (result == RegistrationResult.Success)
            {
                _logger.Info(RegisteredLogMessage);
            }

            return result;
        }

        private static RegistrationResult ValidatePlayer(Player player, string plainPassword)
        {
            RegistrationResult result = RegistrationResult.Success;

            if (!HasRequiredFields(player, plainPassword))
            {
                result = RegistrationResult.MissingRequiredField;
            }
            else if (!HasValidLengths(player))
            {
                result = RegistrationResult.FieldTooLong;
            }
            else if (!EmailAddressValidator.IsValid(player.Email?.Trim()))
            {
                result = RegistrationResult.InvalidEmail;
            }

            return result;
        }

        private static bool HasRequiredFields(Player player, string plainPassword)
        {
            return !string.IsNullOrWhiteSpace(player.FirstName)
                && !string.IsNullOrWhiteSpace(player.LastName)
                && !string.IsNullOrWhiteSpace(player.Username)
                && !string.IsNullOrWhiteSpace(player.Email)
                && !string.IsNullOrWhiteSpace(plainPassword);
        }

        private static bool HasValidLengths(Player player)
        {
            int usernameLength = player.Username?.Trim().Length ?? 0;
            int emailLength = player.Email?.Trim().Length ?? 0;
            int firstNameLength = player.FirstName?.Trim().Length ?? 0;
            int lastNameLength = player.LastName?.Trim().Length ?? 0;

            return usernameLength <= UsernameMaxLength
                && emailLength <= EmailMaxLength
                && firstNameLength <= NameMaxLength
                && lastNameLength <= NameMaxLength;
        }

        private RegistrationResult SavePlayer(Player player, string plainPassword)
        {
            RegistrationResult result;
            string email = player.Email.Trim();
            string username = player.Username.Trim();

            if (_playerRepository.EmailExists(email))
            {
                result = RegistrationResult.EmailAlreadyRegistered;
            }
            else if (_playerRepository.UsernameExists(username))
            {
                result = RegistrationResult.UsernameAlreadyTaken;
            }
            else
            {
                PreparePlayerEntity(player, plainPassword);
                _playerRepository.Add(player);
                result = RegistrationResult.Success;
            }

            return result;
        }

        private static void PreparePlayerEntity(Player player, string plainPassword)
        {
            player.AvatarId = DefaultAvatarId;
            player.Username = player.Username.Trim();
            player.Email = player.Email.Trim();
            player.PwdHash = PasswordHasher.ComputeHash(plainPassword);
            player.State = PlayerState.Active.ToString();
            player.RegDate = DateTime.UtcNow;
            player.XP = InitialExperience;
            player.Level = InitialLevel;
            player.DisplayName = player.Username;
            player.FirstName = player.FirstName.Trim();
            player.LastName = player.LastName.Trim();
        }
    }
}