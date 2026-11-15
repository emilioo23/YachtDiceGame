using System;
using System.Collections.Concurrent;
using YachtDice.Server.Logic.Security;

namespace YachtDice.Server.Logic.TwoFactor
{
    /// <summary>
    /// Generates and validates two-step verification codes, stored in memory. Each player only works with the
    /// entry of its own email, so a concurrent dictionary is enough and the class needs no lock.
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

        private const int MaxFailedAttempts = 3;
        private const int LockMinutes = 15;
        private const int NoFailedAttempts = 0;
        private const int ResendCooldownSeconds = 30;
        private const string GuidFormat = "N";
        private const string NullCodeMessage = "The verification code cannot be null.";

        private readonly ConcurrentDictionary<string, TwoFactorCode> _entries =
            new ConcurrentDictionary<string, TwoFactorCode>(StringComparer.OrdinalIgnoreCase);

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

            TwoFactorValidationStatus status = TwoFactorValidationStatus.Expired;

            if (_entries.TryGetValue(email, out TwoFactorCode issuedCode))
            {
                status = EvaluateIssuedCode(issuedCode, verificationCode);

                if (status == TwoFactorValidationStatus.Valid || status == TwoFactorValidationStatus.Expired)
                {
                    _entries.TryRemove(email, out _);
                }
            }

            return status;
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

        private static TwoFactorValidationStatus EvaluateIssuedCode(
            TwoFactorCode issuedCode,
            VerificationCode verificationCode)
        {
            TwoFactorValidationStatus status;
            DateTime now = DateTime.UtcNow;

            if (issuedCode.ChallengeId != verificationCode.ChallengeId)
            {
                status = TwoFactorValidationStatus.Incorrect;
            }
            else if (issuedCode.LockedUntil > now)
            {
                status = TwoFactorValidationStatus.Locked;
            }
            else if (issuedCode.ExpiresAt < now)
            {
                status = TwoFactorValidationStatus.Expired;
            }
            else if (issuedCode.Code == verificationCode.Code)
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