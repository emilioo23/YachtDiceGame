using System;

namespace YachtDice.Services
{
    /// <summary>
    /// Estado en memoria de un código de verificación emitido a un correo.
    /// </summary>
    internal class TwoFactorCodeEntry
    {
        /// <summary>
        /// Código numérico emitido al jugador.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Momento (UTC) en que se generó el código; sirve para el tiempo de espera del reenvío.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Momento (UTC) a partir del cual el código ya no es válido.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Intentos fallidos acumulados antes de aplicar el bloqueo.
        /// </summary>
        public int FailedAttempts { get; set; }

        /// <summary>
        /// Momento (UTC) hasta el cual el correo permanece bloqueado.
        /// </summary>
        public DateTime LockedUntil { get; set; }
    }
}