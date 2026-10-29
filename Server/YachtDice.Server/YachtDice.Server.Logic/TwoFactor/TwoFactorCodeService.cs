using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;

namespace YachtDice.Server.Logic.TwoFactor
{
    /// <summary>
    /// Generates and validates two-step verification codes, stored in memory.
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
        private const int ResendCooldownSeconds = 30;
        private const int DecimalBase = 10;
        private const int BufferStartIndex = 0;
        private const char PaddingCharacter = '0';

        private readonly object _syncRoot = new object();

        private readonly Dictionary<string, TwoFactorCode> _entries =
            new Dictionary<string, TwoFactorCode>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Generates a new code for the given email and stores it with its challenge.
        /// </summary>
        /// <param name="email">Player's email address.</param>
        /// <param name="challengeId">Unique session challenge identifier.</param>
        /// <returns>Numeric code of <see cref="CodeLength"/> digits.</returns>
        public string GenerateCode(string email, string challengeId)
        {
            string code = CreateRandomCode();
            DateTime now = DateTime.UtcNow;

            lock (_syncRoot)
            {
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
                    FailedAttempts = 0,
                    LockedUntil = lockedUntil
                };
            }

            return code;
        }

        /// <summary>
        /// Creates a new unique challenge identifier.
        /// </summary>
        /// <returns>Challenge string.</returns>
        public string CreateNewChallengeId()
        {
            return Guid.NewGuid().ToString("N");
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

            lock (_syncRoot)
            {
                if (_entries.TryGetValue(email, out TwoFactorCode entry))
                {
                    status = EvaluateEntry(email, entry, code, challengeId);
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

            lock (_syncRoot)
            {
                if (_entries.TryGetValue(email, out TwoFactorCode entry))
                {
                    isLocked = entry.LockedUntil > DateTime.UtcNow;
                }
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

            lock (_syncRoot)
            {
                if (_entries.TryGetValue(email, out TwoFactorCode entry) && entry.ChallengeId == challengeId)
                {
                    isAllowed = DateTime.UtcNow >= entry.CreatedAt.AddSeconds(ResendCooldownSeconds);
                }
            }

            return isAllowed;
        }

        private TwoFactorValidationStatus EvaluateEntry(string email, TwoFactorCode entry, string code, string challengeId)
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
                _entries.Remove(email);
                status = TwoFactorValidationStatus.Expired;
            }
            else if (entry.Code == code)
            {
                _entries.Remove(email);
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
                entry.FailedAttempts = 0;
                entry.LockedUntil = now.AddMinutes(LockMinutes);
                status = TwoFactorValidationStatus.Locked;
            }

            return status;
        }

        private static string CreateRandomCode()
        {
            var upperBound = (long)Math.Pow(DecimalBase, CodeLength);
            var buffer = new byte[sizeof(uint)];

            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(buffer);
            }

            uint randomValue = BitConverter.ToUInt32(buffer, BufferStartIndex);

            return (randomValue % upperBound)
                .ToString(CultureInfo.InvariantCulture)
                .PadLeft(CodeLength, PaddingCharacter);
        }
    }
}