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
        /// <param name="code">Code entered by the player.</param>
        /// <param name="challengeId">Challenge identifier.</param>
        /// <returns>Validation result.</returns>
        public TwoFactorValidationStatus Validate(string email, string code, string challengeId)
        {
            TwoFactorValidationStatus status = TwoFactorValidationStatus.Expired;

            if (_entries.TryGetValue(email, out TwoFactorCode entry))
            {
                status = EvaluateEntry(entry, code, challengeId);

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

            if (_entries.TryGetValue(email, out TwoFactorCode entry))
            {
                isLocked = entry.LockedUntil > DateTime.UtcNow;
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

            if (_entries.TryGetValue(email, out TwoFactorCode entry) && entry.ChallengeId == challengeId)
            {
                isAllowed = DateTime.UtcNow >= entry.CreatedAt.AddSeconds(ResendCooldownSeconds);
            }

            return isAllowed;
        }

        private static TwoFactorValidationStatus EvaluateEntry(TwoFactorCode entry, string code, string challengeId)
        {
            TwoFactorValidationStatus status;
            DateTime now = DateTime.UtcNow;

            if (entry.ChallengeId != challengeId)
            {
                status = TwoFactorValidationStatus.Incorrect;
            }
            else if (entry.LockedUntil > now)
            {
                status = TwoFactorValidationStatus.Locked;
            }
            else if (entry.ExpiresAt < now)
            {
                status = TwoFactorValidationStatus.Expired;
            }
            else if (entry.Code == code)
            {
                status = TwoFactorValidationStatus.Valid;
            }
            else
            {
                status = RegisterFailedAttempt(entry, now);
            }

            return status;
        }

        private static TwoFactorValidationStatus RegisterFailedAttempt(TwoFactorCode entry, DateTime now)
        {
            TwoFactorValidationStatus status = TwoFactorValidationStatus.Incorrect;
            entry.FailedAttempts++;

            if (entry.FailedAttempts >= MaxFailedAttempts)
            {
                entry.FailedAttempts = NoFailedAttempts;
                entry.LockedUntil = now.AddMinutes(LockMinutes);
                status = TwoFactorValidationStatus.Locked;
            }

            return status;
        }
    }
}