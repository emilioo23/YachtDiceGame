using System.Globalization;

namespace YachtDice.Server.Logic.TwoFactor
{
    /// <summary>
    /// Defines the operations that let a player turn the two-step verification on or off.
    /// </summary>
    public interface ITwoFactorSettingsService
    {
        /// <summary>
        /// Indicates whether the player has the two-step verification turned on.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <returns>True if the player must enter a code to log in.</returns>
        bool IsEnabled(string displayName);

        /// <summary>
        /// Sends a code to the player's email to confirm that the player wants to turn the verification on.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="culture">Language in which the email is written.</param>
        /// <returns>Outcome of the request and the data of the challenge.</returns>
        TwoFactorActivationResult StartActivation(string displayName, CultureInfo culture);

        /// <summary>
        /// Checks the code entered by the player and, if it is correct, turns the verification on.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="code">Verification code entered by the player.</param>
        /// <param name="challengeId">Identifier of the activation challenge.</param>
        /// <returns>Result of the validation.</returns>
        TwoFactorValidationStatus ConfirmActivation(string displayName, string code, string challengeId);

        /// <summary>
        /// Sends a new code while the player is turning the verification on.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="cultureName">Name of the language in which the email is written.</param>
        /// <param name="challengeId">Identifier of the activation challenge.</param>
        /// <returns>Outcome of the resend attempt.</returns>
        ResendCodeStatus ResendActivationCode(string displayName, string cultureName, string challengeId);

        /// <summary>
        /// Turns the verification off after checking the player's password.
        /// </summary>
        /// <param name="displayName">Name of the player.</param>
        /// <param name="password">Current password of the player.</param>
        /// <returns>Outcome of the request.</returns>
        TwoFactorChangeStatus Disable(string displayName, string password);
    }
}
