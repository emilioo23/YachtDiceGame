using YachtDice.Server.Logic.TwoFactor;

namespace YachtDice.Server.Logic.Authentication
{
    /// <summary>
    /// Defines the contract for two-factor verification operations.
    /// </summary>
    public interface IVerificationService
    {
        /// <summary>
        /// Verifies a two-factor code.
        /// </summary>
        /// <param name="email">The player's email address.</param>
        /// <param name="code">The verification code provided.</param>
        /// <param name="challengeId">The session challenge identifier.</param>
        /// <returns>The result status of the verification process.</returns>
        TwoFactorValidationStatus VerifyCode(string email, string code, string challengeId);

        /// <summary>
        /// Resends a two-factor verification code.
        /// </summary>
        /// <param name="email">The player's email address.</param>
        /// <param name="cultureName">The culture name for email localization.</param>
        /// <param name="challengeId">The session challenge identifier.</param>
        /// <returns>The result status of the resend process.</returns>
        ResendCodeStatus ResendCode(string email, string cultureName, string challengeId);
    }
}