namespace YachtDice.Services
{
    /// <summary>
    /// Resultado de validar un código de verificación en dos pasos.
    /// </summary>
    public enum TwoFactorValidationResult
    {
        /// <summary>El código es correcto y vigente.</summary>
        Valid,

        /// <summary>El código no coincide con el emitido.</summary>
        Incorrect,

        /// <summary>El código ya venció o no existe uno emitido.</summary>
        Expired,

        /// <summary>El correo está bloqueado por demasiados intentos fallidos.</summary>
        Locked
    }
}