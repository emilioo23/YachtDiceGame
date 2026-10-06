using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;

namespace YachtDice.Services
{
    /// <summary>
    /// Genera y valida códigos de verificación en dos pasos, guardados en memoria.
    /// </summary>
    public class TwoFactorCodeService
    {
        /// <summary>
        /// Cantidad de dígitos que tiene el código de verificación.
        /// </summary>
        public const int CodeLength = 6;

        /// <summary>
        /// Minutos que un código permanece vigente.
        /// </summary>
        public const int ExpirationMinutes = 10;

        private const int MaxFailedAttempts = 3;
        private const int LockMinutes = 15;
        private const int ResendCooldownSeconds = 30;
        private const int DecimalBase = 10;
        private const int BufferStartIndex = 0;
        private const char PaddingCharacter = '0';

        private readonly object _syncRoot = new object();

        private readonly Dictionary<string, TwoFactorCodeEntry> _entries =
            new Dictionary<string, TwoFactorCodeEntry>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Genera un código nuevo para el correo indicado y lo guarda.
        /// </summary>
        /// <param name="email">Correo del jugador.</param>
        /// <returns>Código numérico de <see cref="CodeLength"/> dígitos.</returns>
        public string GenerateCode(string email)
        {
            string code = CreateRandomCode();
            DateTime now = DateTime.UtcNow;

            lock (_syncRoot)
            {
                DateTime lockedUntil = DateTime.MinValue;
                TwoFactorCodeEntry previousEntry;

                // Se conserva el bloqueo vigente para que reenviar no lo salte
                if (_entries.TryGetValue(email, out previousEntry))
                {
                    lockedUntil = previousEntry.LockedUntil;
                }

                _entries[email] = new TwoFactorCodeEntry
                {
                    Code = code,
                    CreatedAt = now,
                    ExpiresAt = now.AddMinutes(ExpirationMinutes),
                    FailedAttempts = 0,
                    LockedUntil = lockedUntil
                };
            }

            return code;
        }

        /// <summary>
        /// Valida el código ingresado contra el último emitido para el correo.
        /// </summary>
        /// <param name="email">Correo del jugador.</param>
        /// <param name="code">Código ingresado por el jugador.</param>
        /// <returns>Resultado de la validación.</returns>
        public TwoFactorValidationResult Validate(string email, string code)
        {
            TwoFactorValidationResult result = TwoFactorValidationResult.Expired;

            lock (_syncRoot)
            {
                TwoFactorCodeEntry entry;

                if (_entries.TryGetValue(email, out entry))
                {
                    result = EvaluateEntry(email, entry, code);
                }
            }

            return result;
        }

        /// <summary>
        /// Indica si el correo está bloqueado por demasiados intentos fallidos.
        /// </summary>
        /// <param name="email">Correo del jugador.</param>
        /// <returns>True si el bloqueo sigue vigente.</returns>
        public bool IsLocked(string email)
        {
            var isLocked = false;

            lock (_syncRoot)
            {
                TwoFactorCodeEntry entry;

                if (_entries.TryGetValue(email, out entry))
                {
                    isLocked = entry.LockedUntil > DateTime.UtcNow;
                }
            }

            return isLocked;
        }

        /// <summary>
        /// Indica si ya pasó el tiempo mínimo para solicitar otro código.
        /// </summary>
        /// <param name="email">Correo del jugador.</param>
        /// <returns>True si se puede reenviar el código.</returns>
        public bool IsResendAllowed(string email)
        {
            var isAllowed = true;

            lock (_syncRoot)
            {
                TwoFactorCodeEntry entry;

                if (_entries.TryGetValue(email, out entry))
                {
                    isAllowed = DateTime.UtcNow >= entry.CreatedAt.AddSeconds(ResendCooldownSeconds);
                }
            }

            return isAllowed;
        }

        private TwoFactorValidationResult EvaluateEntry(string email, TwoFactorCodeEntry entry, string code)
        {
            TwoFactorValidationResult result;
            DateTime now = DateTime.UtcNow;

            if (entry.LockedUntil > now)
            {
                result = TwoFactorValidationResult.Locked;
            }
            else if (entry.ExpiresAt < now)
            {
                _entries.Remove(email);
                result = TwoFactorValidationResult.Expired;
            }
            else if (entry.Code == code)
            {
                _entries.Remove(email);
                result = TwoFactorValidationResult.Valid;
            }
            else
            {
                result = RegisterFailedAttempt(entry, now);
            }

            return result;
        }

        private static TwoFactorValidationResult RegisterFailedAttempt(TwoFactorCodeEntry entry, DateTime now)
        {
            TwoFactorValidationResult result = TwoFactorValidationResult.Incorrect;
            entry.FailedAttempts++;

            if (entry.FailedAttempts >= MaxFailedAttempts)
            {
                entry.FailedAttempts = 0;
                entry.LockedUntil = now.AddMinutes(LockMinutes);
                result = TwoFactorValidationResult.Locked;
            }

            return result;
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