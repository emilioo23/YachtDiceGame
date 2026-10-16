using System;
using System.Net.Mail;
using System.Threading.Tasks;
using YachtDice.Server.Data;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Logging;
using YachtDice.Server.Logic.Players;
using YachtDice.Server.Logic.Security;

namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Validates and registers new players.
    /// </summary>
    public class RegistrationService
    {
        private const int DefaultAvatarId = 1;
        private const int InitialExperience = 0;
        private const int InitialLevel = 1;
        private const int UsernameMaxLength = 20;
        private const int EmailMaxLength = 100;
        private const int NameMaxLength = 50;
        private const string FriendCodePrefix = "FRD-";
        private const int FriendCodeStartIndex = 0;
        private const int FriendCodeLength = 6;
        private const string GuidFormat = "N";
        private const string RegisteredLogMessage = "Player registered successfully.";
        private const string NullRequestMessage = "The registration request cannot be null.";
        private const string NullRepositoryMessage = "The player repository cannot be null.";

        private readonly AppLogger _logger = new AppLogger(typeof(RegistrationService));
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
        }

        /// <summary>
        /// Validates the data and registers the player if everything is correct.
        /// </summary>
        /// <param name="request">Data of the new player.</param>
        /// <returns>Result of the registration attempt.</returns>
        public async Task<RegistrationResult> RegisterAsync(PlayerRegistrationInfo request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), NullRequestMessage);
            }

            RegistrationResult result = ValidateRequest(request);

            if (result == RegistrationResult.Success)
            {
                result = await SavePlayerAsync(request);
            }

            if (result == RegistrationResult.Success)
            {
                _logger.Info(RegisteredLogMessage);
            }

            return result;
        }

        private static RegistrationResult ValidateRequest(PlayerRegistrationInfo request)
        {
            RegistrationResult result = RegistrationResult.Success;

            if (!HasRequiredFields(request))
            {
                result = RegistrationResult.MissingRequiredField;
            }
            else if (!HasValidLengths(request))
            {
                result = RegistrationResult.FieldTooLong;
            }
            else if (!IsValidEmail(request.Email.Trim()))
            {
                result = RegistrationResult.InvalidEmail;
            }

            return result;
        }

        private static bool HasRequiredFields(PlayerRegistrationInfo request)
        {
            return !string.IsNullOrWhiteSpace(request.FirstName)
                && !string.IsNullOrWhiteSpace(request.LastName)
                && !string.IsNullOrWhiteSpace(request.Username)
                && !string.IsNullOrWhiteSpace(request.Email)
                && !string.IsNullOrWhiteSpace(request.Password);
        }

        private static bool HasValidLengths(PlayerRegistrationInfo request)
        {
            return request.Username.Trim().Length <= UsernameMaxLength
                && request.Email.Trim().Length <= EmailMaxLength
                && request.FirstName.Trim().Length <= NameMaxLength
                && request.LastName.Trim().Length <= NameMaxLength;
        }

        private static bool IsValidEmail(string email)
        {
            bool isValid;

            try
            {
                var address = new MailAddress(email);
                isValid = address.Address == email;
            }
            catch (FormatException)
            {
                isValid = false;
            }

            return isValid;
        }

        private async Task<RegistrationResult> SavePlayerAsync(PlayerRegistrationInfo request)
        {
            RegistrationResult result;

            if (await _playerRepository.EmailExistsAsync(request.Email.Trim()))
            {
                result = RegistrationResult.EmailAlreadyRegistered;
            }
            else if (await _playerRepository.UsernameExistsAsync(request.Username.Trim()))
            {
                result = RegistrationResult.UsernameAlreadyTaken;
            }
            else
            {
                await _playerRepository.AddAsync(CreatePlayer(request));
                result = RegistrationResult.Success;
            }

            return result;
        }

        private static Player CreatePlayer(PlayerRegistrationInfo request)
        {
            string username = request.Username.Trim();

            return new Player
            {
                AvatarId = DefaultAvatarId,
                Username = username,
                Email = request.Email.Trim(),
                PwdHash = PasswordHasher.ComputeHash(request.Password),
                State = PlayerState.Active.ToString(),
                RegDate = DateTime.UtcNow,
                XP = InitialExperience,
                Level = InitialLevel,
                DisplayName = username,
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                FriendCode = CreateFriendCode()
            };
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