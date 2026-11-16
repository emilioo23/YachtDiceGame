using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using YachtDice.Server.Data.Repositories;
using YachtDice.Server.Logic.Security;

namespace YachtDice.Server.Logic.TwoFactor
{
    /// <summary>
    /// Generates and validates two-step verification codes, stored in memory. Each player only works with the
    /// entry of its own email, so a concurrent dictionary is enough and the class needs no lock. It also keeps
    /// the backup codes, which are saved in the database and can replace the code sent by email; a wrong backup
    /// code counts as a failed attempt of the same challenge.
    /// </summary>
    public class TwoFactorCodeService
    {
        /// <summary>
        /// Number of digits in the verification code.
        /// </summary>
        public const int CodeLength = 6;

        /// <summary>
        /// Minutes a code stays valid.
        /// </summary>
        public const int ExpirationMinutes = 10;

        /// <summary>
        /// Number of digits in a backup code. It is longer than the code sent by email, so both are told apart.
        /// </summary>
        public const int BackupCodeLength = 8;

        /// <summary>
        /// Number of backup codes that the player receives each time the codes are created.
        /// </summary>
        public const int BackupCodeCount = 10;

        private const int MaxFailedAttempts = 3;
        private const int LockMinutes = 15;
        private const int NoFailedAttempts = 0;
        private const int ResendCooldownSeconds = 30;
        private const string GuidFormat = "N";
        private const string NullCodeMessage = "The verification code cannot be null.";
        private const string NullBackupRepositoryMessage = "The data access of the backup codes cannot be null.";

        private readonly ConcurrentDictionary<string, TwoFactorCode> _entries =
            new ConcurrentDictionary<string, TwoFactorCode>(StringComparer.OrdinalIgnoreCase);

        private readonly IBackupCodeRepository _backupCodeRepository;

        /// <summary>
        /// Initializes the service with the data access of the backup codes.
        /// </summary>
        /// <param name="backupCodeRepository">Repository where the hashes of the backup codes are saved.</param>
        /// <exception cref="ArgumentNullException">The repository is null.</exception>
        public TwoFactorCodeService(IBackupCodeRepository backupCodeRepository)
        {
            _backupCodeRepository = backupCodeRepository
                ?? throw new ArgumentNullException(nameof(backupCodeRepository), NullBackupRepositoryMessage);
        }

        /// <summary>
        /// Generates a new code for the given email and stores it with its challenge.
        /// </summary>
        /// <param name="email">Player's email address.</param>
        /// <param name="challengeId">Unique session challenge identifier.</param>
        /// <returns>Numeric code of <see cref="CodeLength"/> digits.</returns>
        public string GenerateCode(string email, string challengeId)
        {
            string code = NumericCodeGenerator.Generate(CodeLength);
            DateTime now = DateTime.UtcNow;
            DateTime lockedUntil = DateTime.MinValue;

            if (_entries.TryGetValue(email, out TwoFactorCode previousEntry))
            {
                lockedUntil = previousEntry.LockedUntil;
            }

            _entries[email] = new TwoFactorCode
            {
                Code = code,
                ChallengeId = challengeId,
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(ExpirationMinutes),
                FailedAttempts = NoFailedAttempts,
                LockedUntil = lockedUntil
            };

            return code;
        }

        /// <summary>
        /// Creates a new unique challenge identifier.
        /// </summary>
        /// <returns>Challenge string.</returns>
        public string CreateNewChallengeId()
        {
            return Guid.NewGuid().ToString(GuidFormat);
        }

        /// <summary>
        /// Validates the entered code against the last one issued for the email and challenge.
        /// </summary>
        /// <param name="email">Player's email address.</param>
        /// <param name="verificationCode">Code entered by the player and the challenge it answers.</param>
        /// <returns>Validation result.</returns>
        public TwoFactorValidationStatus ValidateCode(string email, VerificationCode verificationCode)
        {
            if (verificationCode == null)
            {
                throw new ArgumentNullException(nameof(verificationCode), NullCodeMessage);
            }

            Func<TwoFactorCode, bool> isEmailCode = issuedCode => issuedCode.Code == verificationCode.Code;

            return ValidateChallenge(email, verificationCode.ChallengeId, isEmailCode);
        }

        /// <summary>
        /// Validates a backup code written instead of the code sent by email. The challenge must still be open,
        /// and a correct backup code is marked as used so it cannot be used again.
        /// </summary>
        /// <param name="email">Player's email address.</param>
        /// <param name="playerId">Identifier of the player, to find the backup codes.</param>
        /// <param name="verificationCode">Backup code entered by the player and the challenge it answers.</param>
        /// <returns>Validation result.</returns>
        /// <exception cref="ArgumentNullException">The code is null.</exception>
        public TwoFactorValidationStatus ValidateBackupCode(
            string email,
            int playerId,
            VerificationCode verificationCode)
        {
            if (verificationCode == null)
            {
                throw new ArgumentNullException(nameof(verificationCode), NullCodeMessage);
            }

            Func<TwoFactorCode, bool> isBackupCode = issuedCode => TryUseBackupCode(playerId, verificationCode.Code);

            return ValidateChallenge(email, verificationCode.ChallengeId, isBackupCode);
        }

        /// <summary>
        /// Indicates whether a code has the shape of a backup code: only digits, with the length of a backup code.
        /// </summary>
        /// <param name="code">Code written by the player.</param>
        /// <returns>True if the code must be checked as a backup code.</returns>
        public static bool IsBackupCodeFormat(string code)
        {
            bool hasBackupLength = code != null && code.Length == BackupCodeLength;

            return hasBackupLength && code.All(char.IsDigit);
        }

        /// <summary>
        /// Creates new backup codes for the player. The codes created before stop working, and only the hash of
        /// each new code is saved.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>The new codes, to show them to the player only once.</returns>
        public List<string> CreateBackupCodes(int playerId)
        {
            var codes = new List<string>();

            for (int codeIndex = 0; codeIndex < BackupCodeCount; codeIndex++)
            {
                codes.Add(NumericCodeGenerator.Generate(BackupCodeLength));
            }

            Func<string, string> hashCode = code => PasswordHasher.ComputeResetCodeHash(playerId, code);

            _backupCodeRepository.Replace(playerId, codes.Select(hashCode).ToList());

            return codes;
        }

        /// <summary>
        /// Counts the backup codes that the player has not used.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        /// <returns>The number of backup codes left.</returns>
        public int CountBackupCodes(int playerId)
        {
            return _backupCodeRepository.CountActive(playerId);
        }

        /// <summary>
        /// Deletes every backup code of the player, for example when the two-step verification is turned off.
        /// </summary>
        /// <param name="playerId">Identifier of the player.</param>
        public void RemoveBackupCodes(int playerId)
        {
            _backupCodeRepository.RemoveAll(playerId);
        }

        /// <summary>
        /// Indicates whether the email is locked because of too many failed attempts.
        /// </summary>
        /// <param name="email">Player's email address.</param>
        /// <returns>True if the lockout is still active.</returns>
        public bool IsLocked(string email)
        {
            var isLocked = false;

            if (_entries.TryGetValue(email, out TwoFactorCode issuedCode))
            {
                isLocked = issuedCode.LockedUntil > DateTime.UtcNow;
            }

            return isLocked;
        }

        /// <summary>
        /// Indicates whether the minimum wait to request another code has passed for the challenge.
        /// </summary>
        /// <param name="email">Player's email address.</param>
        /// <param name="challengeId">Challenge identifier.</param>
        /// <returns>True if the code can be resent.</returns>
        public bool IsResendAllowed(string email, string challengeId)
        {
            var isAllowed = true;

            if (_entries.TryGetValue(email, out TwoFactorCode issuedCode) && issuedCode.ChallengeId == challengeId)
            {
                isAllowed = DateTime.UtcNow >= issuedCode.CreatedAt.AddSeconds(ResendCooldownSeconds);
            }

            return isAllowed;
        }

        private TwoFactorValidationStatus ValidateChallenge(
            string email,
            string challengeId,
            Func<TwoFactorCode, bool> isCorrectAnswer)
        {
            TwoFactorValidationStatus status = TwoFactorValidationStatus.Expired;

            bool isIssued = _entries.TryGetValue(email, out TwoFactorCode issuedCode);

            if (isIssued && issuedCode.ChallengeId == challengeId)
            {
                status = EvaluateIssuedCode(issuedCode, isCorrectAnswer);

                if (status == TwoFactorValidationStatus.Valid || status == TwoFactorValidationStatus.Expired)
                {
                    _entries.TryRemove(email, out _);
                }
            }

            return status;
        }

        private bool TryUseBackupCode(int playerId, string code)
        {
            string codeHash = PasswordHasher.ComputeResetCodeHash(playerId, code);

            return _backupCodeRepository.TryUse(playerId, codeHash);
        }

        private static TwoFactorValidationStatus EvaluateIssuedCode(
            TwoFactorCode issuedCode,
            Func<TwoFactorCode, bool> isCorrectAnswer)
        {
            TwoFactorValidationStatus status;
            DateTime now = DateTime.UtcNow;

            if (issuedCode.LockedUntil > now)
            {
                status = TwoFactorValidationStatus.Locked;
            }
            else if (issuedCode.ExpiresAt < now)
            {
                status = TwoFactorValidationStatus.Expired;
            }
            else if (isCorrectAnswer(issuedCode))
            {
                status = TwoFactorValidationStatus.Valid;
            }
            else
            {
                status = RegisterFailedAttempt(issuedCode, now);
            }

            return status;
        }

        private static TwoFactorValidationStatus RegisterFailedAttempt(TwoFactorCode issuedCode, DateTime now)
        {
            TwoFactorValidationStatus status = TwoFactorValidationStatus.Incorrect;
            issuedCode.FailedAttempts++;

            if (issuedCode.FailedAttempts >= MaxFailedAttempts)
            {
                issuedCode.FailedAttempts = NoFailedAttempts;
                issuedCode.LockedUntil = now.AddMinutes(LockMinutes);
                status = TwoFactorValidationStatus.Locked;
            }

            return status;
        }
    }
}