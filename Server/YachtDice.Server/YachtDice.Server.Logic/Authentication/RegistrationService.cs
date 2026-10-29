using System;
using System.Net.Mail;
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
        private const int FriendCodeStartIndex = 0;
        private const int FriendCodeLength = 6;
        private const string FriendCodePrefix = "FRD-";
        private const string GuidFormat = "N";
        private const string RegisteredLogMessage = "Player registered successfully.";
        private const string NullRequestMessage = "The registration request cannot be null.";
        private const string NullRepositoryMessage = "The player repository cannot be null.";

        private readonly AppLogger _logger;
        private readonly IPlayerRepository _playerRepository;

        /// <summary>
        /// Initializes the service with the data access it needs.
        /// </summary>
        /// <param name="playerRepository">Repository used to read and save players.</param>
        public RegistrationService(IPlayerRepository playerRepository)
        {
            if (playerRepository == null)
            {
                throw new ArgumentNullException(nameof(playerRepository), NullRepositoryMessage);
            }

            _playerRepository = playerRepository;
            _logger = new AppLogger(GetType());
        }

        /// <summary>
        /// Validates the data and registers the player if everything is correct.
        /// </summary>
        /// <param name="player">Data of the new player.</param>
        /// <param name="plainPassword">The player's plain text password.</param>
        /// <returns>Result of the registration attempt.</returns>
        public RegistrationResult Register(Player player, string plainPassword)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player), NullRequestMessage);
            }

            RegistrationResult result = ValidateRequest(player, plainPassword);

            if (result == RegistrationResult.Success)
            {
                result = SavePlayer(player, plainPassword);
            }

            if (result == RegistrationResult.Success)
            {
                _logger.Info(RegisteredLogMessage);
            }

            return result;
        }

        private static RegistrationResult ValidateRequest(Player player, string plainPassword)
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
            else if (!IsValidEmail(player.Email?.Trim()))
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

        private static bool IsValidEmail(string email)
        {
            bool isValid = false;

            if (!string.IsNullOrWhiteSpace(email))
            {
                try
                {
                    var address = new MailAddress(email);
                    isValid = address.Address == email;
                }
                catch (FormatException)
                {
                    isValid = false;
                }
            }

            return isValid;
        }

        private RegistrationResult SavePlayer(Player player, string plainPassword)
        {
            RegistrationResult result;
            string email = player.Email.Trim();
            string username = player.Username.Trim();

            if (_playerRepository.EmailExistsAsync(email).GetAwaiter().GetResult())
            {
                result = RegistrationResult.EmailAlreadyRegistered;
            }
            else if (_playerRepository.UsernameExistsAsync(username).GetAwaiter().GetResult())
            {
                result = RegistrationResult.UsernameAlreadyTaken;
            }
            else
            {
                PreparePlayerEntity(player, plainPassword);
                _playerRepository.AddAsync(player).GetAwaiter().GetResult();
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
            player.FriendCode = CreateFriendCode();
        }

        private static string CreateFriendCode()
        {
            string randomPart = Guid.NewGuid()
                .ToString(GuidFormat)
                .Substring(FriendCodeStartIndex, FriendCodeLength)
                .ToUpperInvariant();

            return FriendCodePrefix + randomPart;
        }
    }
}