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
        /// <param name="verificationCode">The code provided and the session challenge it answers.</param>
        /// <returns>The result status of the verification process.</returns>
        TwoFactorValidationStatus VerifyPlayerCode(string email, VerificationCode verificationCode);

        /// <summary>
        /// Resends a two-factor verification code.
        /// </summary>
        /// <param name="email">The player's email address.</param>
        /// <param name="challengeId">The session challenge identifier.</param>
        /// <param name="cultureName">The culture name for email localization.</param>
        /// <returns>The result status of the resend process.</returns>
        ResendCodeStatus ResendPlayerCode(string email, string challengeId, string cultureName);
    }
}